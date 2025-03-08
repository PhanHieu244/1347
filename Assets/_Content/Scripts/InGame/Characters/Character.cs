using System;
using System.Collections.Generic;
using _Content.Events;
using _Content.InGame.Characters.Abilities;
using _Content.InGame.Damage;
using _Content.InGame.Managers;
using MoreMountains.Tools;
using UnityEngine;
using EventHandler = Opsive.Shared.Events.EventHandler;
using Random = UnityEngine.Random;

namespace _Content.InGame.Characters
{
	[RequireComponent(typeof(TopDownController))]
	public class Character : MonoBehaviour
	{
		public bool PerformAnimatorSanityChecks;
		[SerializeField] private int _initialHealth = 2;
		[SerializeField] private Transform _model;
		[SerializeField] private Animator _characterAnimator;
		[SerializeField] private GameObject _cameraTarget;
		[SerializeField] private GameObject _abilitiesHolder;
		[SerializeField] private Transform _collectionPoint;
		private bool _abilitiesCachedOnce;
		private CharacterAbility[] _characterAbilities;
		private bool _animatorInitialized;
		private HashSet<int> _animatorParameters;
		private Animator _animator;
		private float _animatorRandomNumber;
		private int _randomAnimationParameter;
		private int _currentSpeedAnimationParameter;
		private int _timeScaleAnimationParameter;
		private TopDownController _controller;
		private Health _health;

		private const string _timeScaleAnimationParameterName = "TimeScale";
		private const string _currentSpeedAnimationParameterName = "CurrentSpeed";
		private const string _randomAnimationParameterName = "RandomFloat";

		public HashSet<int> AnimatorParameters => _animatorParameters;
		public Animator Animator => _animator;
		public Transform Model => _model;
		public TopDownController Controller => _controller;
		public GameObject CameraTarget => _cameraTarget;
		public Health Health => _health;

		public int InitialHealth => _initialHealth;
		public Transform CollectionPoint => _collectionPoint;

		protected virtual void Awake()
		{
			Initialization();
		}

		private void OnEnable()
		{
			_health.OnDeath += OnDeath;
		}

		private void OnDisable()
		{
			_health.OnDeath -= OnDeath;
		}

		private void OnDeath(Transform arg1, int arg2)
		{
			GameManager.Instance.ShowDefeatUi();
		}

		protected virtual void Start()
		{
			Setup();
		}

		protected virtual void Update()
		{
			EveryFrame();
		}

		protected virtual void EveryFrame()
		{
			ProcessAbilities();
			LateProcessAbilities();
			UpdateAnimators();
		}

		protected virtual void Initialization()
		{
			_health = GetComponent<Health>();
			// we get the current input manager
			_controller = GetComponent<TopDownController>();
			//SetInputManager();
			AssignAnimator();

			// instantiate camera target
			if (_cameraTarget == null)
			{
				_cameraTarget = new GameObject();
			}

			CacheAbilitiesAtInit();
			CameraTarget.transform.SetParent(this.transform);
			CameraTarget.transform.localPosition = Vector3.zero;
			CameraTarget.name = "CameraTarget";
			_health.MaximumHealth = _initialHealth;
			_health.CurrentHealth = _initialHealth;
			EventHandler.ExecuteEvent(InGameEvents.CharacterHealthChanged);
		}

		public void Teleport(Vector3 position)
		{
			transform.position = position;
		}

		public virtual void AssignAnimator()
		{
			if (_animatorInitialized)
			{
				return;
			}

			_animatorParameters = new HashSet<int>();

			if (_characterAnimator != null)
			{
				_animator = _characterAnimator;
			}
			else
			{
				_animator = this.gameObject.GetComponent<Animator>();
			}

			if (_animator != null)
			{
				InitializeAnimatorParameters();
			}

			_animatorInitialized = true;
		}

		private void InitializeAnimatorParameters()
		{
			if (_animator == null)
			{
				return;
			}

			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _currentSpeedAnimationParameterName,
				out _currentSpeedAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _randomAnimationParameterName,
				out _randomAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _timeScaleAnimationParameterName,
				out _timeScaleAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
		}

		protected virtual void Setup()
		{
			foreach (var characterAbility in _characterAbilities)
			{
				characterAbility.Setup();
			}
		}

		protected virtual void CacheAbilitiesAtInit()
		{
			if (_abilitiesCachedOnce)
			{
				return;
			}

			CacheAbilities();
		}

		private void CacheAbilities()
		{
			if (_abilitiesHolder != null)
				_characterAbilities = _abilitiesHolder.GetComponents<CharacterAbility>();
			else
				_characterAbilities = this.gameObject.GetComponents<CharacterAbility>();

			_abilitiesCachedOnce = true;
		}


		protected virtual void UpdateAnimators()
		{
			var PerformAnimatorSanityChecks = false;
			UpdateAnimationRandomNumber();

			if (_animator != null)
			{
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _timeScaleAnimationParameter,
					GameManager.Instance.TimeScale, _animatorParameters, PerformAnimatorSanityChecks);
				/*MMAnimatorExtensions.UpdateAnimatorBool(_animator, _groundedAnimationParameter, _controller.Grounded,
					_animatorParameters, false);*/
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _currentSpeedAnimationParameter,
					_controller.CurrentMovementInput.magnitude, _animatorParameters, PerformAnimatorSanityChecks);
				/*MMAnimatorExtensions.UpdateAnimatorBool(_animator, _idleAnimationParameter,
					(MovementState.CurrentState == CharacterStates.MovementStates.Idle), _animatorParameters,
					PerformAnimatorSanityChecks);*/
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _randomAnimationParameter, _animatorRandomNumber,
					_animatorParameters, PerformAnimatorSanityChecks);

				foreach (CharacterAbility ability in _characterAbilities)
				{
					if (ability.enabled && ability.AbilityInitialized)
					{
						ability.UpdateAnimator();
					}
				}
			}
		}

		private void UpdateAnimationRandomNumber()
		{
			_animatorRandomNumber = Random.Range(0f, 1f);
		}

		protected virtual void ProcessAbilities()
		{
			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability.enabled && ability.AbilityInitialized && ability.AbilityPermitted)
				{
					ability.ProcessAbility();
				}
			}
		}

		/// <summary>
		/// Calls all registered abilities' Late Process methods
		/// </summary>
		protected virtual void LateProcessAbilities()
		{
			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability.enabled && ability.AbilityInitialized)
				{
					ability.LateProcessAbility();
				}
			}
		}

		public T FindAbility<T>() where T : CharacterAbility
		{
			CacheAbilitiesAtInit();

			Type searchedAbilityType = typeof(T);

			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability is T characterAbility)
				{
					return characterAbility;
				}
			}

			return null;
		}

		public void Reset()
		{
		}

		public void HealFromDrop()
		{
			var health = Mathf.Min(Mathf.CeilToInt(_health.MaximumHealth / 2f),
				_health.MaximumHealth - _health.CurrentHealth);
			_health.CurrentHealth += health;
			EventHandler.ExecuteEvent(InGameEvents.CharacterHealthChanged);
		}

		public void Revive()
		{
			_health.Revive();
		}
	}
}