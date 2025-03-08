using MoreMountains.Tools;
using NaughtyAttributes;
using UnityEngine;

namespace _Content.InGame.Characters.Abilities
{
	public abstract class CharacterAbility : MonoBehaviour
	{
		[ReadOnly] public Character _character;
		protected TopDownController _controller;
		protected Animator _animator;
		public bool AbilityInitialized { get; private set; }
		public bool AbilityPermitted { get; set; } = true;

		protected virtual void Awake()
		{
			Initialization();
		}

		protected virtual void Initialization()
		{
			_character = GetComponentInParent<Character>();
			_controller = GetComponentInParent<TopDownController>();
			_animator = _character.Animator;
			AbilityInitialized = true;
		}

		public virtual void Setup()
		{
		}

		public virtual void ProcessAbility()
		{
		}

		public virtual void LateProcessAbility()
		{
		}

		public virtual void UpdateAnimator()
		{
		}

		protected virtual void RegisterAnimatorParameter(string parameterName,
			AnimatorControllerParameterType parameterType, out int parameter)
		{
			parameter = Animator.StringToHash(parameterName);

			if (_animator == null)
			{
				return;
			}

			if (_animator.MMHasParameterOfType(parameterName, parameterType))
			{
				if (_character != null)
				{
					_character.AnimatorParameters.Add(parameter);
				}
			}
		}
	}
}