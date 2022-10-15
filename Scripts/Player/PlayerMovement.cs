﻿using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	private Touch _touch;
	private Rigidbody2D _rigidbody;
	private GameManager _gameManager;
	private float gravity;
	private bool _alreadyPlayed = false;
	[HideInInspector] public bool dead;
	[SerializeField] private GameObject _gameOverMenu;
	[SerializeField] private GameObject _playerNose;

	[SerializeField] private ParticleSystem ps;
	private ParticleSystem.EmissionModule emission;
	[SerializeField] private ParticleSystem flame;
	private ParticleSystem.EmissionModule flameEmission;
	[SerializeField] private ParticleSystem death;

	public static PlayerMovement instance;

	private void Awake()
	{
		Application.targetFrameRate = 60;
		instance = this;
		_rigidbody = GetComponent<Rigidbody2D>();
		_gameManager = FindObjectOfType<GameManager>();
		gravity = _rigidbody.gravityScale;
		emission = ps.emission;
		flameEmission = flame.emission;
		Time.timeScale = 0;
	}

	private void FixedUpdate()
	{
		if (dead) return;

		Vector2 velocity = _rigidbody.velocity;
		float angle = Mathf.Atan2(velocity.y, 10) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 88));

		// Touch Controls:
		if (Input.touchCount > 0)
		{
			_touch = Input.GetTouch(0);
			if (_touch.phase == TouchPhase.Began || _touch.phase == TouchPhase.Stationary)
			{
				FindObjectOfType<AudioManager>().PlaySFX("Fuel");
				flameEmission.enabled = true;
				_rigidbody.AddForce(Vector2.up * gravity * Time.deltaTime * 1200f);
			}
			else if (_touch.phase == TouchPhase.Ended)
			{
				FindObjectOfType<AudioManager>().StopSFX("Fuel");
				flameEmission.enabled = false;
			}
		}

		// Keyboard Controls:
		if (Input.GetKey(KeyCode.Space))
		{
			FindObjectOfType<AudioManager>().PlaySFX("Fuel");
			flameEmission.enabled = true;
			_rigidbody.AddForce(Vector2.up * gravity * Time.deltaTime * 1200f);
		}
		else if (Input.GetKeyUp(KeyCode.Space))
		{
			FindObjectOfType<AudioManager>().StopSFX("Fuel");
			flameEmission.enabled = false;
		}
	}

	private void DeathEffect()
	{
		Instantiate(death, _playerNose.transform.position, Quaternion.identity);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.CompareTag("ScoreDetector"))
		{
			_gameManager.IncreaseScore();
		}
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Obstruction"))
		{
			dead = true;
			FindObjectOfType<AudioManager>().StopSFX("Fuel");
			DeathEffect();
			if (!_alreadyPlayed)
			{
				FindObjectOfType<AudioManager>().PlaySFX("Dead");
				_alreadyPlayed = true;
			}
			emission.enabled = false;
			flameEmission.enabled = false;
			_rigidbody.AddForce(Vector2.down * 130f);
			_rigidbody.AddTorque(30f);
			StartCoroutine(FreezeTime());
		}
	}
	private IEnumerator FreezeTime()
	{
		yield return new WaitForSeconds(1.5f);
		_rigidbody.velocity = Vector2.zero;
		_rigidbody.freezeRotation = true;
		Time.timeScale = 0;
		dead = false;
		_gameManager.GetScore();
		_gameOverMenu.SetActive(true);
	}
}
