using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.CustomPlugins;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace _Content.InGame.Characters.Abilities
{
	public class CharacterOrientation : CharacterAbility
	{
		[SerializeField] private float _timeToRotate;
		private Vector3 _neededDirection;
		private bool _neededDirectionChanged;
		private Vector3 _currentDirection;
		private Quaternion _currentRotation;
		private Quaternion _neededRotation;
		private TweenerCore<Quaternion,Quaternion,NoOptions> _rotationChangeTween;

		private void OnDestroy()
		{
			if (_rotationChangeTween != null && _rotationChangeTween.IsActive())
				_rotationChangeTween.Kill();
		}

		protected override void Initialization()
		{
			base.Initialization();

			_currentDirection = Vector3.back;
			_neededDirection = Vector3.back;
			_currentRotation = _neededRotation = Quaternion.Euler(0f, 180f, 0f);
			_character.Model.rotation = _currentRotation;
			
			_rotationChangeTween = DOTween.To(PureQuaternionPlugin.Plug(), () => _currentRotation, (x) => _currentRotation = x,_neededRotation, _timeToRotate)
				//.SetSpeedBased()
				.SetEase(Ease.Linear)
				.SetAutoKill(false);
		}

		public override void Setup()
		{
			base.Setup();
		}

		public override void ProcessAbility()
		{
			base.ProcessAbility();
			CalculateNeededRotation();
			HandleRotation();
		}

		private void HandleRotation()
		{
			_character.Model.rotation = _currentRotation;
		}

		public void Rotate(Vector3 direction)
		{
			_neededDirection = direction;
			if (!_neededDirectionChanged)
				_neededDirectionChanged = true;
		}

		public void Rotate(Quaternion quaternion)
		{
			Rotate(quaternion * Vector3.forward);
		}

		private void CalculateNeededRotation()
		{
			var newDir = _controller.CurrentMovementInput.normalized;
			if (newDir.sqrMagnitude < 0.001f)
				return;

			if (Vector3.Angle(_neededDirection, newDir) > 0.1f)
			{
				_neededDirection = newDir;
				_neededRotation = Quaternion.LookRotation(_neededDirection);
				_rotationChangeTween.ChangeEndValue(_neededRotation, true);
				_rotationChangeTween.Restart();
			}
		}
	}
}