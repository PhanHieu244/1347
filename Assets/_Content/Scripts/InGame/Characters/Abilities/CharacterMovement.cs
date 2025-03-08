using _Content.InGame.Managers;
using UnityEngine;

namespace _Content.InGame.Characters.Abilities
{
	public class CharacterMovement : CharacterAbility
	{
		[SerializeField] private bool _analogInput;
		[SerializeField] private float _idleThreshold = 0f;
		[SerializeField] private bool _interpolateSpeed;
		[SerializeField] private float _walkSpeed;
		[SerializeField] private float _acceleration;
		[SerializeField] private float _deceleration;
		private Vector2 _lastInput;
		private Vector2 _lerpedInput;
		private float _movementSpeed;
		private float _currentAcceleration;
		private bool _inputForbid;

		public Vector2 LastInput => _lastInput;
		public float MovementSpeed { get; private set; }
		public float MovementSpeedMultiplier { get; set; }

		public override void Setup()
		{
			base.Setup();
			MovementSpeed = _walkSpeed;
			MovementSpeedMultiplier = 1f;
		}

		public override void ProcessAbility()
		{
			base.ProcessAbility();
			SetMovement();
		}

		protected virtual void SetMovement()
		{
			var deltaTime = Time.deltaTime * GameManager.Instance.TimeScale;
			var movementVector = Vector3.zero;
			var currentInput = Vector2.zero;

			currentInput.x = _lastInput.x;
			currentInput.y = _lastInput.y;

			var normalizedInput = currentInput.normalized;
			if ((_acceleration == 0) || (_deceleration == 0))
			{
				_lerpedInput = currentInput;
			}
			else
			{
				if (normalizedInput.magnitude == 0)
				{
					_currentAcceleration = Mathf.Lerp(_currentAcceleration, 0f, _deceleration * deltaTime);
					_lerpedInput = Vector2.Lerp(_lerpedInput, _lerpedInput * _currentAcceleration,
						deltaTime * _deceleration);
				}
				else
				{
					_currentAcceleration = Mathf.Lerp(_currentAcceleration, 1f, _acceleration * deltaTime);
					_lerpedInput = _analogInput
						? Vector2.ClampMagnitude(currentInput, _currentAcceleration)
						: Vector2.ClampMagnitude(normalizedInput, _currentAcceleration);
				}
			}

			movementVector.x = _lerpedInput.x;
			movementVector.y = 0f;
			movementVector.z = _lerpedInput.y;

			if (_interpolateSpeed)
			{
				_movementSpeed = Mathf.Lerp(_movementSpeed, MovementSpeed * MovementSpeedMultiplier,
					_acceleration * deltaTime);
			}
			else
			{
				_movementSpeed = MovementSpeed * MovementSpeedMultiplier;
			}

			movementVector *= _movementSpeed;


			if (movementVector.magnitude > MovementSpeed)
			{
				movementVector = Vector3.ClampMagnitude(movementVector, MovementSpeed);
			}

			/*if ((_currentInput.magnitude <= _idleThreshold) && (_controller.CurrentMovement.magnitude < _idleThreshold))
			{
				movementVector = Vector3.zero;
			}*/

			

			_controller.SetMovementInput(movementVector);
		}

		public virtual void SetInput(Vector2 movementVector)
		{
			if (_inputForbid)
				_lastInput = Vector2.zero;
			else
				_lastInput = movementVector;
		}

		public void ForbidMovement(bool forbid)
		{
			_inputForbid = forbid;
		}
	}
}