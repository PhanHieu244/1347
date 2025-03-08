using System;
using System.Collections;
using System.Collections.Generic;
using _Content.InGame.Managers;
using Common;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace _Content.InGame.Damage
{
	public class DamageOnTouch : MonoBehaviour
	{
		[SerializeField] private ChildTrigger _damageBox;
		[SerializeField] private List<ChildTrigger> _damageBoxes;
		[Header("Damage Caused")] public LayerMask TargetLayerMask;
		public bool PerfectImpact = false;
		public int DamageCaused = 10;
		public bool _damageOncePerStay;
		public float InvincibilityDuration = 0.5f;

		[Space] [Header("Damage Taken")] public int DamageTakenDamageable = 0;
		public float DamageTakenInvincibilityDuration = 0.5f;
		public bool DamageTakenAfterFrame;


		public MMFeedbacks HitDamageableFeedback;
		public MMFeedbacks HitNonDamageableFeedback;
		public MMFeedbacks ShieldFeedback;

		public Action OnCollide;
		public Action<Health> CollideWithDamagable;

		private Health _health;
		private BoxCollider _boxCollider;
		private SphereCollider _sphereCollider;

		private Vector3 _lastDamagePosition;
		private List<GameObject> _ignoredGameObjects;
		private Vector3 _lastPosition;
		private Vector3 _velocity;
		private Vector3 _damageDirection;
		private float _startTime;
		private Vector3 _collisionPoint;
		private Health _colliderHealth;
		public int PlayerId { get; set; }

		protected virtual void Awake()
		{
			InitializeIgnoreList();

			_health = GetComponent<Health>() ?? GetComponentInParent<Health>();
			_boxCollider = GetComponent<BoxCollider>();
			_sphereCollider = GetComponent<SphereCollider>();
			_lastDamagePosition = this.transform.position;

			InitializeFeedbacks();
		}

		protected virtual void OnEnable()
		{
			_startTime = Time.time;
			_lastPosition = this.transform.position;
			_lastDamagePosition = this.transform.position;
			if (_damageBox != null)
			{
				_damageBox.TriggerEntered += OnChildTriggerCollision;
				_damageBox.TriggerStay += OnChildTriggerCollision;
				_damageBox.TriggerExit += OnChildTriggerExitCollision;
			}

			if (_damageBoxes.Count > 0)
			{
				foreach (var damageBox in _damageBoxes)
				{
					damageBox.TriggerEntered += OnChildTriggerCollision;
					damageBox.TriggerStay += OnChildTriggerCollision;
					damageBox.TriggerExit += OnChildTriggerExitCollision;
				}
			}
		}

		private void OnDisable()
		{
			if (_damageBox != null)
			{
				_damageBox.TriggerEntered -= OnChildTriggerCollision;
				_damageBox.TriggerStay -= OnChildTriggerCollision;
				_damageBox.TriggerExit -= OnChildTriggerExitCollision;
			}

			if (_damageBoxes.Count > 0)
			{
				foreach (var damageBox in _damageBoxes)
				{
					damageBox.TriggerEntered -= OnChildTriggerCollision;
					damageBox.TriggerStay -= OnChildTriggerCollision;
					damageBox.TriggerExit -= OnChildTriggerExitCollision;
				}
			}
		}

		protected virtual void Update()
		{
			ComputeVelocity();
		}

		public void ResetIgnoredGo()
		{
			_ignoredGameObjects.Clear();
		}
		public virtual void OnChildTriggerCollision(Collider collider)
		{
			Colliding(collider.gameObject);
			if (_damageOncePerStay)
				_ignoredGameObjects.AddIfNotExist(collider.gameObject);
		}

		public virtual void OnTriggerStay(Collider collider)
		{
			Colliding(collider.gameObject);
			if (_damageOncePerStay)
				_ignoredGameObjects.AddIfNotExist(collider.gameObject);
		}

		public virtual void OnTriggerEnter(Collider collider)
		{
			Colliding(collider.gameObject);
			if (_damageOncePerStay)
				_ignoredGameObjects.AddIfNotExist(collider.gameObject);
		}

		private void OnTriggerExit(Collider other)
		{
			if (_ignoredGameObjects.Contains(other.gameObject))
				_ignoredGameObjects.Remove(other.gameObject);
		}

		private void OnChildTriggerExitCollision(Collider other)
		{
			if (_ignoredGameObjects.Contains(other.gameObject))
				_ignoredGameObjects.Remove(other.gameObject);
		}

		private void InitializeFeedbacks()
		{
			HitDamageableFeedback?.Initialization(this.gameObject);
			HitNonDamageableFeedback?.Initialization(this.gameObject);
		}

		protected virtual void InitializeIgnoreList()
		{
			if (_ignoredGameObjects == null)
			{
				_ignoredGameObjects = new List<GameObject>();
			}
		}

		public virtual void IgnoreGameObject(GameObject newIgnoredGameObject)
		{
			InitializeIgnoreList();
			_ignoredGameObjects.Add(newIgnoredGameObject);
		}

		public virtual void StopIgnoringObject(GameObject ignoredGameObject)
		{
			if (_ignoredGameObjects != null)
			{
				_ignoredGameObjects.Remove(ignoredGameObject);
			}
		}

		public virtual void ClearIgnoreList()
		{
			InitializeIgnoreList();
			_ignoredGameObjects.Clear();
		}

		protected virtual void ComputeVelocity()
		{
			var deltaTime = Time.deltaTime * GameManager.Instance.TimeScale;
			if (deltaTime != 0f)
			{
				_velocity = (_lastPosition - (Vector3)transform.position) / deltaTime;

				if (Vector3.Distance(_lastDamagePosition, this.transform.position) > 0.5f)
				{
					_damageDirection = this.transform.position - _lastDamagePosition;
					_lastDamagePosition = this.transform.position;
				}

				_lastPosition = this.transform.position;
			}
		}

		protected virtual void Colliding(GameObject collider)
		{
			if (!this.isActiveAndEnabled)
			{
				return;
			}

			if (_ignoredGameObjects.Contains(collider))
			{
				return;
			}

			if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask))
			{
				return;
			}

			// if we're on our first frame, we don't apply damage
			if (Time.time == 0f)
			{
				return;
			}

			_collisionPoint = this.transform.position;
			_colliderHealth = collider.gameObject.GetComponent<Health>() ??
			                  collider.gameObject.GetComponentInParent<Health>();

			// if what we're colliding with is damageable
			if (_colliderHealth != null)
			{
				if (_colliderHealth.CurrentHealth > 0)
				{
					OnCollideWithDamageable(_colliderHealth);
					CollideWithDamagable?.Invoke(_colliderHealth);
					OnCollide?.Invoke();
				}
			}

			// if what we're colliding with can't be damaged
			else
			{
				/*OnCollideWithNonDamageable(collider);
				OnCollide?.Invoke();*/
			}
		}


		protected virtual void OnCollideWithDamageable(Health health)
		{
			var feedbackPosition = this.transform.position;
			HitDamageableFeedback?.PlayFeedbacks(feedbackPosition);

			_colliderHealth.Damage(DamageCaused, gameObject, InvincibilityDuration, InvincibilityDuration,
				_damageDirection, gameObject, PlayerId);
			if (_colliderHealth.Invulnerable && _colliderHealth.CurrentHealth > 0)
			{
				ShieldFeedback?.PlayFeedbacks(this.transform.position);
			}
		}

		/*protected virtual void OnCollideWithNonDamageable(GameObject obj)
		{
			if (DamageTakenEveryTime + DamageTakenNonDamageable > 0)
			{
				SelfDamage(DamageTakenEveryTime + DamageTakenNonDamageable);
			}

			HitNonDamageableFeedback?.PlayFeedbacks(this.transform.position);
		}*/

		protected virtual void SelfDamage(int damage)
		{
			StartCoroutine(SelfDamageRoutine(damage));
		}

		private IEnumerator SelfDamageRoutine(int damage)
		{
			if (DamageTakenAfterFrame)
				yield return null;
			if (_health != null)
			{
				_damageDirection = Vector3.up;
				_health.Damage(damage, gameObject, 0f, DamageTakenInvincibilityDuration, _damageDirection, gameObject,
					PlayerId);

				if ((_health.CurrentHealth <= 0) && PerfectImpact)
				{
					this.transform.position = _collisionPoint;
				}
			}
		}
	}
}