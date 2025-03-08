using _Content.InGame.Managers;
using UnityEngine;

namespace _Content.InGame.Enemies
{
	public class RangeEnemyController : SimpleEnemyController
	{
		[SerializeField] private float _minPlayerDistanceToAttack;
		[SerializeField] private float _maxPlayerDistanceToAttack;
		[SerializeField] private Vector2 _minMaxTimeoutCooldown;
		[SerializeField] private EnemyProjectile _enemyProjectilePrefab;
		[SerializeField] private float _attackTime = 0.75f;
		[SerializeField] private float _yOffset = 1f;
		private float _attackCooldownTimer;
		private bool _attackInProgress;
		private static readonly int AttackAnimatorParameter = Animator.StringToHash("Attack");
		private float _attackTimer;

		protected override void Initialize()
		{
			base.Initialize();
			_attackInProgress = false;
			_attackCooldownTimer = Random.Range(_minMaxTimeoutCooldown.x / 3f, _minMaxTimeoutCooldown.y / 3f);
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
			_lastTranslation = Vector3.zero;
			var direction = playersTransform.position - transform.position;
			direction.y = 0f;
			transform.rotation = Quaternion.LookRotation(direction);
			if (playersTransform != null && _movementEnabled && !_attackInProgress &&
			    direction.magnitude >= _minPlayerDistanceToAttack)
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
			}
			else
			{
				_isMove = false;
			}

			if (!_attackInProgress && _attackCooldownTimer <= 0f && direction.magnitude < _maxPlayerDistanceToAttack)
			{
				StartAttack();
			}

			UpdateAnimator();
		}

		private void StopAttack()
		{
			_attackInProgress = false;
			_attackCooldownTimer = Random.Range(_minMaxTimeoutCooldown.x, _minMaxTimeoutCooldown.y);
		}

		private void StartAttack()
		{
			_animator.SetTrigger(AttackAnimatorParameter);
			_attackTimer = 0f;
			_attackInProgress = true;
		}

		public void ShootProjectile()
		{
			var playersTransform = EnemyManager.Instance.GetPlayersTransform();
			var direction = playersTransform.position - transform.position;
			direction.y = 0f;
			direction.Normalize();
			var pos = transform.position + Vector3.up * _yOffset;
			var projectile = Instantiate(_enemyProjectilePrefab, pos, Quaternion.identity);
			projectile.Shoot(pos, direction, GetDamage());
		}

		private int GetDamage()
		{
			return _initialDamage;
		}
	}
}