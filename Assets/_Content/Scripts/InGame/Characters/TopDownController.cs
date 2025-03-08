using System.Collections;
using _Content.InGame.Managers;
using NaughtyAttributes;
using UnityEngine;

namespace _Content.InGame.Characters
{
	[RequireComponent(typeof(CharacterController))]
	public class TopDownController : MonoBehaviour
	{
		[ReadOnly] public Vector3 Velocity;
		[SerializeField] private bool _gravityActive;
		[SerializeField] private float _gravity = 9.8f;
		[SerializeField] private float _maximumFallSpeed = 10f;
		[SerializeField] private bool _arenaRestrictionsEnabled;
		[SerializeField] [ReadOnly] private bool _grounded;

		private Vector3 _newVelocity;
		private Vector3 _positionLastFrame;
		private Vector3 _velocityLastFrame;
		private Vector3 _motion;
		private Vector3 _horizontalVelocityDelta;
		private CharacterController _characterController;
		private float _stickyOffset;
		private CollisionFlags _collisionFlags;
		private bool _groundedLastFrame;
		private Vector3 _groundNormal;
		private Vector3 _lastGroundNormal;
		private Vector3 _lastHitPoint;
		private Vector3 _hitPoint;
		private Vector3 _frameVelocity;
		private bool _controllerIsActive;

		public bool Grounded => _grounded;
		public bool JustGotGrounded { get; private set; }

		public bool GravityActive
		{
			get => _gravityActive;
			set => _gravityActive = value;
		}

		public Vector3 CurrentMovementInput { get; private set; }

		protected virtual void Awake()
		{
			Initialize();
		}

		private void Initialize()
		{
			_characterController = GetComponent<CharacterController>();
			Activate(true);
		}

		protected virtual void Update()
		{
			CheckIfGrounded();
			UpdateMovement();
		}

		private void UpdateMovement()
		{
			var deltaTime = Time.deltaTime * GameManager.Instance.TimeScale;
			_newVelocity = Velocity;
			_positionLastFrame = transform.position;

			if (!_controllerIsActive)
				return;

			AddInput();
			AddGravity(deltaTime);
			CalculateMotion(deltaTime);
			MoveCharacterController();

			StickToTheGround();
			Velocity = _newVelocity;
		}

		private void AddInput()
		{
			var idealVelocity = CurrentMovementInput;

			idealVelocity += _frameVelocity;
			idealVelocity.y = 0;

			if (Grounded)
			{
				Vector3 sideways = Vector3.Cross(Vector3.up, idealVelocity);
				idealVelocity = Vector3.Cross(sideways, _groundNormal).normalized * idealVelocity.magnitude;
			}

			_newVelocity = idealVelocity;
			_newVelocity.y = Grounded ? Mathf.Min(_newVelocity.y, 0) : _newVelocity.y;
		}

		protected virtual void LateUpdate()
		{
			_velocityLastFrame = Velocity;
		}

		public void SetMovementInput(Vector3 movementInput)
		{
			CurrentMovementInput = movementInput;
		}

		private void CalculateMotion(float deltaTime)
		{
			_motion = _newVelocity * deltaTime;
			_horizontalVelocityDelta.x = _motion.x;
			_horizontalVelocityDelta.y = 0f;
			_horizontalVelocityDelta.z = _motion.z;
			_stickyOffset = Mathf.Max(_characterController.stepOffset, _horizontalVelocityDelta.magnitude);
			if (Grounded)
			{
				_motion -= _stickyOffset * Vector3.up;
			}

			if (_motion != Vector3.zero && _arenaRestrictionsEnabled)
			{
				var nextPos = transform.position + _motion;
				var dif = nextPos - GameplayManager.Instance.ArenaCenter;
				if (dif.magnitude >
				    GameplayManager.Instance.ArenaSize)
				{
					var dir = dif.normalized;
					var magn = _motion.magnitude;
					var signedAngle = Vector3.SignedAngle(dir, _motion, Vector3.up);
					nextPos = Quaternion.Euler(0f, Mathf.Sign(signedAngle) * 3f * deltaTime, 0f) *
					          (GameplayManager.Instance.ArenaCenter +
					           dir * GameplayManager.Instance.ArenaSize);
					/*var angle = Mathf.Sign(signedAngle) * Mathf.Asin(magn / 2f / GameplayManager.Instance.ArenaSize) *
					            Mathf.Rad2Deg * 2f;
					var newAngle = signedAngle + angle;
					nextPos = Quaternion.Euler(0f, newAngle, 0f) * (GameplayManager.Instance.ArenaCenter +
					                                                dir * GameplayManager.Instance.ArenaSize);*/
					dif = nextPos - transform.position;
					_motion = dif;
					/*var velMagn = _newVelocity.magnitude;
					
					_newVelocity.x = dif.x;
					_newVelocity.z = dif.z;
					Vector3.ClampMagnitude(_newVelocity, velMagn);
					_motion = _newVelocity * deltaTime;*/
				}
			}
		}

		protected virtual void MoveCharacterController()
		{
			_groundNormal = Vector3.zero;

			_collisionFlags = _characterController.Move(_motion);

			_lastHitPoint = _hitPoint;
			_lastGroundNormal = _groundNormal;
		}

		protected virtual void AddGravity(float deltaTime)
		{
			if (GravityActive)
			{
				if (Grounded)
				{
					_newVelocity.y = Mathf.Min(0, _newVelocity.y) - _gravity * deltaTime;
				}
				else
				{
					_newVelocity.y = Velocity.y - _gravity * deltaTime;
					_newVelocity.y = Mathf.Max(_newVelocity.y, -_maximumFallSpeed);
				}
			}
		}

		public virtual bool IsGroundedTest()
		{
			return (_groundNormal.y > 0.01);
		}

		protected virtual void CheckIfGrounded()
		{
			JustGotGrounded = (!_groundedLastFrame && Grounded);
			_groundedLastFrame = Grounded;
		}

		protected virtual void OnControllerColliderHit(ControllerColliderHit hit)
		{
			if (hit.normal.y > 0 && hit.normal.y > _groundNormal.y && hit.moveDirection.y < 0)
			{
				if (
					(hit.point.y - _lastHitPoint.y < 0.1f)
					&& ((hit.point != _lastHitPoint)
					    || (_lastGroundNormal == Vector3.zero)))
				{
					_groundNormal = hit.normal;
				}
				else
				{
					_groundNormal = _lastGroundNormal;
				}

				_hitPoint = hit.point;
				_frameVelocity = Vector3.zero;
			}

			//HandlePush(hit, hit.point);
		}

		protected virtual void StickToTheGround()
		{
			if (Grounded && !IsGroundedTest())
			{
				_grounded = false;
				transform.position += _stickyOffset * Vector3.up;
			}
			else if (!Grounded && IsGroundedTest())
			{
				_grounded = true;
			}
		}

		public void Activate(bool active)
		{
			_controllerIsActive = active;
			if (!_controllerIsActive)
				CurrentMovementInput = Vector3.zero;
		}

		public IEnumerator ActivateAfterFrame(bool active)
		{
			yield return null;
			Activate(active);
		}
	}
}