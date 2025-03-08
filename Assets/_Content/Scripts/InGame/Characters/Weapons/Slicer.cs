using _Content.Events;
using _Content.InGame.Damage;
using _Content.InGame.Managers;
using Opsive.Shared.Events;
using UnityEngine;

namespace _Content.InGame.Characters.Weapons
{
	public class Slicer: MonoBehaviour
	{
		[SerializeField] private float _speed;
		private bool _shooted;
		private Vector3 _currentDirection;
		private bool _initialized;
		private Health _health;
		private DamageOnTouch _damageOnTouch;
		private Vector3 _initialDirection;
		private Vector3 _initialPosition;

		private void Awake()
		{
			Initialize();
		}
		private void OnEnable()
		{
			EventHandler.RegisterEvent(InGameEvents.DeinitializeLevel, OnLevelDeinitialized);
		}

		private void OnDisable()
		{
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
				var dif = _currentDirection * (_speed * deltaTime);
				transform.position += dif;
				if (Vector3.Distance(transform.position, _initialPosition) > 50f)
				{
					DestroySlicer();
				}
			}
		}

		private void Initialize()
		{
			if (_initialized)
				return;

			_health = GetComponent<Health>();
			_damageOnTouch = GetComponent<DamageOnTouch>();
			_initialized = true;
		}
		
		private void DestroySlicer()
		{
			_health.Damage(1000, gameObject, 0f, 0f, Vector3.zero, gameObject, 1);
		}
		
		public void Shoot(Vector3 initialPosition, Vector3 direction, int damage)
		{
			Initialize();
			
			_initialPosition = initialPosition;
			_initialDirection = direction;
			_currentDirection = direction;
			_damageOnTouch.DamageCaused = damage;
			transform.forward = _initialDirection;
			_shooted = true;
		}
	}
}