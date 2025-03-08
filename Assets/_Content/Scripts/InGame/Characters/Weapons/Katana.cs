using _Content.Events;
using _Content.InGame.Damage;
using _Content.InGame.Enemies;
using _Content.InGame.Managers;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using MoreMountains.Feedbacks;
using Opsive.Shared.Events;
using UnityEngine;

namespace _Content.InGame.Characters.Weapons
{
	public class Katana : MonoBehaviour
	{
		[SerializeField] private Transform _meshTransform;
		[SerializeField] private MMF_Player _onRotationStartFeedback;
		[SerializeField] private MMF_Player _onRotationCompleteFeedback;
		private bool _initialized;
		private DamageOnTouch _damageOnTouch;
		private bool _shooted;
		private Vector3 _direction;
		private float _angle;
		private Vector3 _initialDirection;
		private Vector3 _destinationDirection;
		private Vector3 _currentDirection;
		private float _timer;
		private float _time;
		private AnimationCurve _curve;
		private float _rotationTime;
		private SimpleEnemyController _currentEnemy;
		private TweenerCore<Quaternion, Quaternion, NoOptions> _rotationTween;
		private float _offset;
		private float _currentAngle;
		private float _currentSign;
		private float _destinationAngle;
		private bool _onlyUp;
		private int _directionChanges;
		private int _directionChangesCount;

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
			var deltaTime = Time.deltaTime * GameManager.Instance.TimeScale;
			_timer += deltaTime;

			if (_currentEnemy == null)
			{
				var enemy = EnemyManager.Instance.GetNearestEnemy(transform.position, 0f);
				if (enemy != null)
				{
					_currentEnemy = enemy;
					//SetRotation();
				}
			}

			if (GameplayManager.Instance.Character == null)
			{
				Destroy(gameObject);
				return;
			}
			
			if (_shooted)
			{
				var charPos = GameplayManager.Instance.Character.transform.position;
				if (!_onlyUp && _currentEnemy != null)
				{
					var direction = _currentEnemy.transform.position - charPos;
					direction.y = 0f;
					direction.Normalize();
					_direction = direction;
					transform.position = charPos + _direction * _offset;
				}
				else
				{
					_direction = GameplayManager.Instance.Character.Model.forward;
					transform.position = charPos + _direction * _offset;
				}

				_currentAngle += _currentSign * _angle * deltaTime / _rotationTime;
				if (Mathf.Abs(Mathf.DeltaAngle(_currentAngle, _destinationAngle)) < 4f ||
				    _destinationAngle < 0f && _currentAngle < _destinationAngle ||
				    _destinationAngle > 0f && _currentAngle > _destinationAngle)
				{
					ChangeDirection();
					if (_directionChanges > _directionChangesCount)
					{
						
						_onRotationCompleteFeedback?.PlayFeedbacks();
						Destroy(gameObject);
					}
					_damageOnTouch.ResetIgnoredGo();
				}

				transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(Quaternion.Euler(0f, _currentAngle, 0f) * _direction), deltaTime * 10f);
				//transform.rotation = Quaternion.LookRotation(Quaternion.Euler(0f, _currentAngle, 0f) * _direction);
			}

			/*if (_timer > _time)
			{
				_onRotationCompleteFeedback?.PlayFeedbacks();
				Destroy(gameObject);
			}*/
		}

		private void ChangeDirection()
		{
			if (_currentSign < 0f)
			{
				_currentAngle = -_angle / 2f;
				_destinationAngle = _angle / 2f;
				_currentSign = 1f;
				_meshTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);
			}
			else
			{
				_currentAngle = _angle / 2f;
				_destinationAngle = -_angle / 2f;
				_currentSign = -1f;
				_meshTransform.localRotation = Quaternion.identity;
			}

			_directionChanges++;
		}

		private void Initialize()
		{
			if (_initialized)
				return;

			_damageOnTouch = GetComponent<DamageOnTouch>();
			_onRotationStartFeedback?.Initialization(gameObject);
			_onRotationCompleteFeedback?.Initialization(gameObject);
			_initialized = true;
		}

		public void Shoot(Vector3 direction, float angle, int damage, float rotationTime, AnimationCurve rotationCurve,
			float time, float characterOffset, SimpleEnemyController enemy, bool onlyUp, int directionChangesCount)
		{
			Initialize();
			_damageOnTouch.DamageCaused = damage;
			_directionChangesCount = directionChangesCount;
			_currentEnemy = enemy;
			_time = time;
			_direction = direction;
			_angle = angle;
			_curve = rotationCurve;
			_rotationTime = rotationTime;
			_offset = characterOffset;
			_onlyUp = onlyUp;

			if (onlyUp)
				_direction = GameplayManager.Instance.Character.Model.forward;
			_currentSign = 1f;
			ChangeDirection();

			_onRotationStartFeedback?.PlayFeedbacks();
			transform.rotation = Quaternion.LookRotation(Quaternion.Euler(0f, _currentAngle, 0f) * _direction);
			//SetRotation();

			_timer = 0f;
			_shooted = true;
		}

		private void SetRotation()
		{
			_initialDirection = Quaternion.Euler(0f, _angle / 2f, 0f) * _direction;
			_destinationDirection = Quaternion.Euler(0f, -_angle / 2f, 0f) * _direction;
			_meshTransform.localRotation = Quaternion.identity;
			if (Vector3.SignedAngle(Vector3.forward, _direction, Vector3.up) >= 0)
			{
				_initialDirection = Quaternion.Euler(0f, -_angle / 2f, 0f) * _direction;
				_destinationDirection = Quaternion.Euler(0f, _angle / 2f, 0f) * _direction;
				_meshTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);
			}

			if (_rotationTween == null || !_rotationTween.IsActive())
			{
				transform.rotation = Quaternion.LookRotation(_initialDirection);
				_rotationTween = transform
					.DORotateQuaternion(Quaternion.LookRotation(_destinationDirection), _rotationTime)
					.SetEase(_curve)
					.SetLoops(-1, LoopType.Yoyo);
			}
			else
			{
				_rotationTween.ChangeStartValue(Quaternion.LookRotation(_initialDirection));
				_rotationTween.ChangeEndValue(Quaternion.LookRotation(_destinationDirection), true)
					.Restart();
			}
		}
	}
}