using System;
using System.Collections.Generic;
using System.Linq;
using _Content.Events;
using _Content.InGame.Damage;
using _Content.InGame.Managers;
using Common;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Unity.VisualScripting;
using UnityEngine;
using EventHandler = Opsive.Shared.Events.EventHandler;
using Random = UnityEngine.Random;

namespace _Content.InGame.Enemies
{
	public class SimpleEnemyController : MonoBehaviour
	{
		private static readonly int TimeScaleAnimatorParameter = Animator.StringToHash("TimeScale");
		private static readonly int RandomOffsetAnimatorParameter = Animator.StringToHash("RandomOffset");
		private static readonly int IsMoveAnimatorParameter = Animator.StringToHash("IsMove");
		protected static readonly int MovementAnimatorState = Animator.StringToHash("Move");

		[SerializeField] private string _enemyId;
		[SerializeField] private ParticleSystem _hitParticleSystem;
		[SerializeField] private ParticleSystem _deathParticleSystem;

		[Header("Experience")] [SerializeField]
		private int experienceForKill = 1;

		[SerializeField] private List<int> _gemsToDrop = new List<int> { 1 };

		[Header("Settings")] [SerializeField] protected int _initialDamage;
		[SerializeField] private int _initialHealth;

		[Header("Slices")] [SerializeField] private List<GameObject> _bodySlices;
		[SerializeField] private List<Rigidbody> _lastSlices;
		[SerializeField] private Vector2 _lastSlicesForce;
		[SerializeField] private float _timeToDestroyLastSlices = 2f;
		[Header("Movement")] [SerializeField] private LayerMask _enemyObstaclesMask;
		[SerializeField] private float _hitboxRadius = 1f;
		[SerializeField] private float _sphereCheckRadius = 2f;
		[SerializeField] protected float _speed;
		[SerializeField] protected Animator _animator;
		[SerializeField] protected AnimationCurve _speedCurve;
		[SerializeField] private bool _drawGizmos;

		protected bool _movementEnabled = true;
		private GameObject _currentBodyState;
		protected Vector3 _lastTranslation;
		protected bool _isMove;
		private Health _health;
		private Collider[] _obstaclesResult = new Collider[10];
		private Tween _forceFromPointTween;
		private DamageOnTouch _damageOnTouch;
		private int _additionalDamage;
		private int _additionalHealth;

		public string EnemyId => _enemyId;
		public ParticleSystem HitParticleSystem => _hitParticleSystem;
		public ParticleSystem DeathParticleSystem => _deathParticleSystem;
		public int ExperienceForKill => experienceForKill;
		public List<int> GemsToDrop => _gemsToDrop;

		private void Awake()
		{
			Initialize();
		}
		
		protected virtual void Initialize()
		{
			if (_animator == null)
				_animator = GetComponent<Animator>();

			if (_animator != null)
				_animator.SetFloat(RandomOffsetAnimatorParameter, Random.Range(0f, 1f));

			_bodySlices.ForEach(s => s.SetActive(false));
			_currentBodyState = _bodySlices.First();
			_currentBodyState.SetActive(true);

			_health = GetComponent<Health>();
			if (_health != null)
			{
				UpdateHealth(true);
			}

			_damageOnTouch = GetComponent<DamageOnTouch>();
			if (_damageOnTouch != null)
			{
				UpdateDamage();
			}

			_movementEnabled = true;
		}

		private void UpdateDamage()
		{
			var damage = GetDamage();
			_damageOnTouch.DamageCaused = damage;
		}

		private int GetDamage()
		{
			return _initialDamage + _additionalDamage;
		}

		private void UpdateHealth(bool initialize)
		{
			var health = GetHealth();
			_health.MaximumHealth = health;
			_health.InitialHealth = health;
			if (initialize)
				_health.CurrentHealth = health;
		}

		private int GetHealth()
		{
			return _initialHealth + _additionalHealth;
		}

		private void OnDestroy()
		{
			transform.DOKill();
		}

		private void OnEnable()
		{
			_health.OnHit += OnHit;
			_health.OnDeath += OnDeath;
			EventHandler.RegisterEvent<float>(InGameEvents.OnTimeScaleChanged, OnTimeScaleChanged);
		}

		private void OnDisable()
		{
			_health.OnHit -= OnHit;
			_health.OnDeath -= OnDeath;
			EventHandler.UnregisterEvent<float>(InGameEvents.OnTimeScaleChanged, OnTimeScaleChanged);
		}

		private void OnTimeScaleChanged(float newTimeScale)
		{
			_animator.SetFloat(TimeScaleAnimatorParameter, newTimeScale);
			if (_forceFromPointTween != null && _forceFromPointTween.IsActive())
				_forceFromPointTween.timeScale = GameManager.Instance.TimeScale;
		}

		private void OnDeath(Transform other, int playerId)
		{
			foreach (var slice in _lastSlices)
			{
				slice.gameObject.SetActive(true);
				slice.transform.SetParent(null);
				slice.isKinematic = false;
				slice.AddForce(
					(_health.LastDamageDirection + Vector3.up * 2f) *
					Random.Range(_lastSlicesForce.x, _lastSlicesForce.y),
					ForceMode.Impulse);
				slice.AddTorque(Random.insideUnitSphere * Random.Range(10f, 100f));
				var dat = slice.AddComponent<DestroyAfterTime>();
				dat._time = _timeToDestroyLastSlices;
			}

			EnemyManager.Instance.OnEnemyDeath(this);
		}

		private void OnHit(int damage, int playerId)
		{
			var slicesPerDamage = _bodySlices.Count / (float)_health.MaximumHealth;
			var index = Mathf.FloorToInt(slicesPerDamage * (_health.MaximumHealth - _health.CurrentHealth));
			for (int i = 0; i < _bodySlices.Count; i++)
			{
				var slice = _bodySlices[i];
				var enable = i == index;
				if (enable && !slice.activeSelf)
					slice.SetActive(true);
				else if (!enable && slice.activeSelf)
					slice.SetActive(false);
			}
		}

		protected virtual void Update()
		{
			var deltaTime = Time.deltaTime * GameManager.Instance.TimeScale;
			var playersTransform = EnemyManager.Instance.GetPlayersTransform();
			_lastTranslation = Vector3.zero;
			if (playersTransform != null && _movementEnabled)
			{
				_isMove = true;
				var animationTime = 0f;
				if (_animator != null)
				{
					var state = _animator.GetCurrentAnimatorStateInfo(0);
					if (state.shortNameHash == MovementAnimatorState)
					{
						var normalizedTime = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
						animationTime = normalizedTime - Mathf.Floor(normalizedTime);
					}
				}

				var speedRatio = _speedCurve.Evaluate(animationTime);
				var deltaMagnitude = (_speed * speedRatio * deltaTime);

				var direction = playersTransform.position - transform.position;
				direction.y = 0f;
				_lastTranslation = direction.normalized * deltaMagnitude;
				_lastTranslation = CheckObstacles(direction, deltaMagnitude);
				transform.position += _lastTranslation;

				transform.rotation = Quaternion.LookRotation(direction);
			}
			else
			{
				_isMove = false;
			}

			UpdateAnimator();
		}

		protected Vector3 CheckObstacles(Vector3 direction, float magnitude)
		{
			return CheckObstacles(direction, magnitude, _enemyObstaclesMask);
		}

		protected Vector3 CheckObstacles(Vector3 direction, float magnitude, LayerMask obstacleMask)
		{
			var count = Physics.OverlapSphereNonAlloc(transform.position + Vector3.up * 1f, _sphereCheckRadius,
				_obstaclesResult,
				obstacleMask);
			if (count > 0)
			{
				var resultVectors = new List<Vector3>();
				var resultAngle = 0f;
				for (int i = 0; i < count; i++)
				{
					var col = _obstaclesResult[i];
					if (col.transform.root == transform.root)
						continue;
					var dif = col.transform.position - transform.position;
					var angle = Vector3.SignedAngle(direction, dif.normalized, Vector3.up);
					var absAngle = Mathf.Abs(angle);
					if (absAngle < 110f)
					{
						var addAngle = -Mathf.Sign(angle) * (110f - absAngle);
						resultAngle += -Mathf.Sign(angle) * (110f - absAngle);
						resultVectors.Add(Quaternion.Euler(0f, addAngle, 0f) * direction);
					}
				}

				if (resultVectors.Count > 0)
				{
					var resultDirection = Vector3.zero;
					foreach (var resultVector in resultVectors)
					{
						resultDirection += resultVector;
					}

					direction = resultDirection / resultVectors.Count;
				}
				//direction = Quaternion.Euler(0f, resultAngle, 0f) * direction;
			}

			var ratio = 1f;
			var maxDistance = magnitude * 5f;
			if (Physics.SphereCast(transform.position + Vector3.up * 1f, _hitboxRadius, direction, out var hit,
				    maxDistance,
				    obstacleMask))
			{
				if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacles") ||
				    hit.collider.gameObject.layer == LayerMask.NameToLayer("CameraBounds"))
				{
					direction = hit.normal;
				}

				ratio = hit.distance / maxDistance;
			}

			return direction.normalized * (magnitude * ratio);
		}

		protected void UpdateAnimator()
		{
			if (_animator == null)
				return;

			//_animator.SetBool(IsMoveAnimatorParameter, _isMove);
		}

		private void OnDrawGizmos()
		{
			if (!_drawGizmos)
				return;

			Gizmos.DrawWireSphere(transform.position, _sphereCheckRadius);
		}

		public void ForceFromPoint(Vector3 position, float distance)
		{
			_movementEnabled = false;
			var dir = transform.position - position;
			dir.y = 0f;
			dir.Normalize();
			_forceFromPointTween = transform.DOMove(transform.position + dir * distance, 0.3f)
				.OnComplete(() => _movementEnabled = true);
			GameManager.Instance.OnDamageTimeScale();
			_forceFromPointTween.timeScale = GameManager.Instance.TimeScale;
		}

		public void SetAdditionalSettings(int health, int damage)
		{
			_additionalHealth = health;
			_additionalDamage = damage;
			UpdateHealth(true);
			UpdateDamage();
		}
	}
}