using _Content.Events;
using _Content.InGame.Damage;
using _Content.InGame.Managers;
using Opsive.Shared.Events;
using UnityEngine;

namespace _Content.InGame.Characters.Weapons
{
	public class PizzaKnife : MonoBehaviour
	{
		[SerializeField] private bool _rotateInDirection;
		[SerializeField] private float _speed = 15f;
		private Vector3 _currentDirection;
		private int _damage;
		private bool _shooted;
		private DamageOnTouch _damageOnTouch;
		private Health _health;
		private bool _backDirection;
		private bool _initialized;
		private Vector3 _targetVelocity;
		private Vector3 _currentVelocity;
		private float _ratioToTargetVelocity;

		private void Awake()
		{
			Initialize();
		}

		private void OnEnable()
		{
			_damageOnTouch.CollideWithDamagable += OnDamage;
			EventHandler.RegisterEvent(InGameEvents.DeinitializeLevel, OnLevelDeinitialized);
		}

		private void OnDisable()
		{
			_damageOnTouch.CollideWithDamagable -= OnDamage;
			EventHandler.UnregisterEvent(InGameEvents.DeinitializeLevel, OnLevelDeinitialized);
		}

		private void Update()
		{
			if (_shooted)
			{
				var deltaTime = Time.deltaTime * GameManager.Instance.TimeScale;
				if (_targetVelocity != _currentVelocity)
					_currentVelocity = Vector3.Lerp(_currentVelocity, _targetVelocity, deltaTime * _ratioToTargetVelocity);
				
				var dif = _currentVelocity * deltaTime;
				transform.position += dif;

				if (_rotateInDirection)
				{
					var angle = Vector3.Angle(transform.forward, _currentDirection);
					transform.forward = Vector3.Slerp(transform.forward, _currentDirection,
						deltaTime * Mathf.Lerp(5f, 8f, Mathf.Clamp01(angle / 90f)));
					//transform.forward = _currentDirection;
				}

				if (_backDirection && Vector3.Dot(_currentVelocity, _targetVelocity) > 0)
					_ratioToTargetVelocity = 3f;
			}
		}

		private void OnLevelDeinitialized()
		{
			Destroy(gameObject);
		}

		private void Initialize()
		{
			if (_initialized)
				return;

			_backDirection = false;
			_health = GetComponent<Health>();
			_damageOnTouch = GetComponent<DamageOnTouch>();
			_initialized = true;
		}


		public void Shoot(Vector3 direction, int damage)
		{
			_currentDirection = direction;
			_targetVelocity = _currentDirection * _speed;
			_currentVelocity = Vector3.zero;
			_ratioToTargetVelocity = 5f;
			_damage = damage;
			_shooted = true;
		}

		private void OnDamage(Health health)
		{
			if (_backDirection)
			{
				return;
			}
			else
			{
				ChangeDirection();
			}
		}

		private void ChangeDirection()
		{
			_currentDirection = -_currentDirection;
			_targetVelocity = _currentDirection * _speed * 1.5f;
			_backDirection = true;
			_ratioToTargetVelocity = .5f;
		}

		private void DestroyBoomerang()
		{
			_health.Damage(1000, gameObject, 0f, 0f, Vector3.zero, gameObject, 1);
		}
	}
}