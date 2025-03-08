using _Content.InGame.Managers;
using UnityEngine;

namespace _Content.InGame.Enemies
{
	public class DashEnemyController : SimpleEnemyController
	{
		private static readonly int AttackAnimatorParameter = Animator.StringToHash("Attack");
		[SerializeField] private Vector2 _minMaxTimeoutCooldown;
		[SerializeField] private float _attackTime = 0.75f;
		[SerializeField] private float _maxPlayerDistanceToAttack = 20f;
		[SerializeField] private float _dashSpeed = 20f;
		[SerializeField] private AnimationCurve _dashSpeedCurve;
		[SerializeField] private LayerMask _obstacleMaskWhenDash;
		[SerializeField] private GameObject _dashIndicator;
		private bool _attackInProgress;
		private float _attackCooldownTimer;
		private float _attackTimer;
		private Vector3 _attackDirection;

		protected override void Initialize()
		{
			base.Initialize();
			_attackInProgress = false;
			_attackCooldownTimer = Random.Range(_minMaxTimeoutCooldown.x / 3f, _minMaxTimeoutCooldown.y / 3f);
			_dashIndicator.SetActive(false);
		}

		protected override void Update()
		{
			var deltaTime = Time.deltaTime * GameManager.Instance.TimeScale;

			if (_attackCooldownTimer > 0f)
				_attackCooldownTimer -= deltaTime;

			if (_attackInProgress)
			{
				_attackTimer += deltaTime;
				if (_attackTimer > _attackTime)
					StopAttack();
			}

			var playersTransform = EnemyManager.Instance.GetPlayersTransform();
			if (playersTransform == null)
				return;

			_lastTranslation = Vector3.zero;
			var direction = playersTransform.position - transform.position;
			direction.y = 0f;

			if (playersTransform != null && _movementEnabled && !_attackInProgress)
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

				direction.y = 0f;
				_lastTranslation = direction.normalized * deltaMagnitude;
				_lastTranslation = CheckObstacles(direction, deltaMagnitude);
				transform.position += _lastTranslation;
				transform.rotation = Quaternion.LookRotation(direction);
			}
			else
			{
				if (_attackInProgress)
				{
					var t = _attackTimer / _attackTime;
					var dashSpeedRatio = _dashSpeedCurve.Evaluate(t);
					var deltaMagnitude = (_dashSpeed * dashSpeedRatio * deltaTime);

					_lastTranslation = _attackDirection * deltaMagnitude;
					_lastTranslation = CheckObstacles(_attackDirection, deltaMagnitude, _obstacleMaskWhenDash);
					if (_lastTranslation != Vector3.zero && _dashIndicator.activeSelf)
					{
						_dashIndicator.SetActive(false);
					}

					transform.position += _lastTranslation;
					transform.rotation = Quaternion.LookRotation(_attackDirection);
				}

				_isMove = false;
			}

			if (CanAttack(direction.magnitude))
			{
				StartAttack(direction);
			}

			UpdateAnimator();
		}

		private bool CanAttack(float distanceToPlayer)
		{
			if (!_attackInProgress && _attackCooldownTimer <= 0f && distanceToPlayer < _maxPlayerDistanceToAttack)
			{
				return true;
			}

			return false;
		}

		private void StopAttack()
		{
			_dashIndicator.SetActive(false);
			_attackInProgress = false;
			_attackCooldownTimer = Random.Range(_minMaxTimeoutCooldown.x, _minMaxTimeoutCooldown.y);
		}

		private void StartAttack(Vector3 dir)
		{
			//_animator.SetTrigger(AttackAnimatorParameter);
			_attackTimer = 0f;
			_attackDirection = dir.normalized;
			_attackInProgress = true;
			_dashIndicator.SetActive(true);
		}

		private int GetDamage()
		{
			return _initialDamage;
		}
	}
}