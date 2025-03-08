using System.Collections.Generic;
using System.Linq;
using _Content.Events;
using _Content.InGame.Damage;
using _Content.InGame.Enemies;
using _Content.InGame.Managers;
using Opsive.Shared.Events;
using UnityEngine;

namespace _Content.InGame.Characters.Weapons
{
	public class Boomerang : MonoBehaviour
	{
		[SerializeField] private bool _rotateInDirection;
		[SerializeField] private float _speed = 5f;
		private bool _initialized;
		private DamageOnTouch _damageOnTouch;
		private int _ricochet;
		private int _ricochetCount;
		private Vector3 _currentDirection;
		private Vector3 _initialDirection;
		private bool _shooted;
		private Health _health;
		private List<SimpleEnemyController> _enemiesAlreadyShooted = new List<SimpleEnemyController>();

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

		private void OnLevelDeinitialized()
		{
			Destroy(gameObject);
		}


		private void Update()
		{
			if (_shooted)
			{
				var deltaTime = Time.deltaTime * GameManager.Instance.TimeScale;
				var dif = _currentDirection * _speed * deltaTime;
				transform.position += dif;

				if (_rotateInDirection)
				{
					var angle = Vector3.Angle(transform.forward, _currentDirection);
					transform.forward = Vector3.Slerp(transform.forward, _currentDirection, deltaTime * Mathf.Lerp(5f, 8f, Mathf.Clamp01(angle/ 90f)));
					//transform.forward = _currentDirection;
				}
			}
		}

		private void Initialize()
		{
			if (_initialized)
				return;

			_ricochet = 0;
			_health = GetComponent<Health>();
			_damageOnTouch = GetComponent<DamageOnTouch>();
			_initialized = true;
		}


		private void OnDamage(Health health)
		{
			if (_ricochet >= _ricochetCount - 1)
			{
				DestroyBoomerang();
				return;
			}
			else
			{
				_ricochet++;
			}

			var enemy = health.GetComponent<SimpleEnemyController>() ??
			            health.GetComponentInParent<SimpleEnemyController>();
			var newDirection = _currentDirection;

			var nextEnemy = EnemyManager.Instance.GetNearestEnemy(transform.position, 5f,
				_enemiesAlreadyShooted.Union(new List<SimpleEnemyController>() { enemy }).ToList());
			var pos = Vector3.zero;
			if (nextEnemy != null)
			{
				AddEnemies(nextEnemy);
				pos = nextEnemy.transform.position;
			}

			newDirection = pos - transform.position;
			newDirection.y = 0f;
			newDirection.Normalize();

			_currentDirection = newDirection;
		}

		private void DestroyBoomerang()
		{
			_health.Damage(1000, gameObject, 0f, 0f, Vector3.zero, gameObject, 1);
		}

		public void Shoot(Vector3 direction, int damage, int ricochetCount)
		{
			Initialize();
			_initialDirection = direction;
			_currentDirection = direction;
			_damageOnTouch.DamageCaused = damage;
			_ricochetCount = ricochetCount;
			_shooted = true;
		}

		public void AddEnemies(SimpleEnemyController enemy)
		{
			_enemiesAlreadyShooted.Add(enemy);
		}
	}
}