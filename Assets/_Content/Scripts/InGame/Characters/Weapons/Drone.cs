using System.Collections.Generic;
using _Content.Events;
using _Content.InGame.Damage;
using _Content.InGame.Enemies;
using _Content.InGame.Managers;
using DG.Tweening;
using Opsive.Shared.Events;
using UnityEngine;

namespace _Content.InGame.Characters.Weapons
{
	public class Drone : MonoBehaviour
	{
		[SerializeField] private int _damage = 1;
		[SerializeField] private float _attackDelayTime = 0.5f;
		[SerializeField] private float _speed = 5f;
		private SimpleEnemyController _currentEnemy;
		private float _changeEnemyTimer;
		private Dictionary<int, float> _objectsToTrack = new Dictionary<int, float>();
		private bool _started;
		private float _lifeTime;
		private float _lifeTimer;
		private bool _destroyed;

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

		private void Start()
		{
			StartDrone();
		}

		private void Update()
		{
			if (!_started)
				return;
			var deltaTime = Time.deltaTime * GameManager.Instance.TimeScale;
			_changeEnemyTimer += deltaTime;
			_lifeTimer += deltaTime;
			if (_currentEnemy == null || _changeEnemyTimer > 1f)
			{
				var enemy = EnemyManager.Instance.GetNearestEnemy(transform.position, 0f);
				if (enemy != null)
				{
					_currentEnemy = enemy;
				}

				_changeEnemyTimer = 0f;
			}

			if (_currentEnemy != null)
			{
				var dir = _currentEnemy.transform.position - transform.position;
				dir.y = 0f;
				dir.Normalize();

				if (dir != Vector3.zero)
					transform.position += dir * _speed * deltaTime;
			}

			if (!_destroyed && _lifeTimer > _lifeTime)
			{
				_destroyed = true;
				transform.DOScale(Vector3.zero, 0.5f)
					.OnComplete(() => Destroy(gameObject));
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			var id = other.gameObject.GetInstanceID();
			if (!_objectsToTrack.ContainsKey(id))
				_objectsToTrack.Add(id, 0f);
		}

		private void OnTriggerStay(Collider other)
		{
			var deltaTime = Time.deltaTime * GameManager.Instance.TimeScale;
			var id = other.gameObject.GetInstanceID();
			if (_objectsToTrack.TryGetValue(id, out float timer))
			{
				_objectsToTrack[id] -= deltaTime;
				if (_objectsToTrack[id] <= 0f)
				{
					DamageCollider(other);
					_objectsToTrack[id] = _attackDelayTime;
				}
			}
		}

		private void DamageCollider(Collider other)
		{
			var colliderHealth = other.gameObject.GetComponent<Health>() ??
			                     other.gameObject.GetComponentInParent<Health>();

			if (colliderHealth != null)
			{
				colliderHealth.Damage(_damage, gameObject, 0f, 0f,
					Vector3.forward, gameObject, 1);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			var id = other.gameObject.GetInstanceID();
			if (_objectsToTrack.ContainsKey(id))
				_objectsToTrack.Remove(id);
		}

		public void StartDrone()
		{
			_currentEnemy = EnemyManager.Instance.GetNearestEnemy(transform.position, 0f);
			_changeEnemyTimer = 0f;
			_started = true;
		}

		public void SetProperties(float lifeTime, int damage)
		{
			_lifeTime = lifeTime;
			_damage = damage;
		}
	}
}