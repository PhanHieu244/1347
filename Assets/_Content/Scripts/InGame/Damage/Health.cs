using System;
using System.Collections;
using System.Linq;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace _Content.InGame.Damage
{
	public class Health : MonoBehaviour
	{
		private static readonly int DamageAnimatorParameter = Animator.StringToHash("Damage");
		private static readonly int DeathAnimatorParameter = Animator.StringToHash("Death");
		private static readonly int DeathBoolAnimatorParameter = Animator.StringToHash("Death_bool");

		[SerializeField] private Animator _animatorGo;

		[Tooltip("the model to disable (if set so)")]
		public GameObject Model;

		[Header("Status")] [Tooltip("the current health of the character")]
		public int CurrentHealth;

		[MMReadOnly] [Tooltip("If this is true, this object can't take damage")]
		public bool Invulnerable = false;

		[Header("Health")] [Tooltip("the initial amount of health of the object")]
		public int InitialHealth = 10;

		[Tooltip("the maximum amount of health of the object")]
		public int MaximumHealth = 10;

		[Tooltip("the feedback to play when getting damage")]
		public MMF_Player DamageMMFeedbacks;

		[Header("DeathAnimatorParameter")] [Tooltip("whether or not this object should get destroyed on death")]
		public bool DestroyOnDeath = true;

		[Tooltip("the time (in seconds) before the character is destroyed or disabled")]
		public float DelayBeforeDestruction = 0f;

		[Tooltip("if this is true, the model will be disabled instantly on death (if a model has been set)")]
		public bool DisableModelOnDeath = true;

		[Tooltip("if this is true, collisions will be turned off when the character dies")]
		public bool DisableCollisionsOnDeath = true;

		[Tooltip("whether or not this object should change layer on death")]
		public bool ChangeLayerOnDeath = false;

		/// the layer we should move this character to on death
		[Tooltip("the layer we should move this character to on death")]
		public MMLayer LayerOnDeath;

		/// the feedback to play when dying
		[Tooltip("the feedback to play when dying")]
		public MMF_Player DeathMMFeedbacks;

		public Vector3 LastDamageDirection { get; private set; }
		public Vector3 LastShootPoint { get; private set; }
		public int LastDamage { get; private set; }

		public float LastDamageTime { get; private set; }

		public Action<int, int> OnHit;
		public Action<Transform, int> OnDeath;
		public Action OnRevive;

		private Animator _animator;
		private int _initialLayer;
		private bool _initialized;
		private Collider _collider3D;

		private void Awake()
		{
			Initialization();
		}

		protected virtual void Initialization()
		{
			if (Model != null)
			{
				Model.SetActive(true);
			}

			_collider3D = GetComponent<Collider>();
			if (_collider3D == null && Model != null)
			{
				_collider3D = Model.GetComponent<Collider>();
			}

			if (_animatorGo != null)
				_animator = _animatorGo;
			else
				_animator = GetComponent<Animator>();

			if (_animator != null)
			{
				_animator.logWarnings = false;
			}

			_initialLayer = gameObject.layer;

			DamageMMFeedbacks?.Initialization(this.gameObject);
			DeathMMFeedbacks?.Initialization(this.gameObject);

			_initialized = true;
			CurrentHealth = InitialHealth;
			DamageEnabled();
		}

		protected virtual void OnEnable()
		{
			CurrentHealth = InitialHealth;
			if (Model != null)
			{
				Model.SetActive(true);
			}

			DamageEnabled();
		}

		public virtual void Damage(int damage, GameObject instigator, float flickerDuration,
			float invincibilityDuration,
			Vector3 damageDirection, GameObject owner, int whoShootPlayerId)
		{
			if (Invulnerable)
			{
				return;
			}

			if ((CurrentHealth <= 0) && (InitialHealth != 0))
			{
				return;
			}

			float previousHealth = CurrentHealth;
			CurrentHealth -= damage;

			LastDamage = damage;
			LastDamageDirection = damageDirection;
			LastShootPoint = owner != null ? owner.transform.position : transform.position;
			LastDamageTime = Time.time;
			OnHit?.Invoke(damage, whoShootPlayerId);

			if (CurrentHealth < 0)
			{
				CurrentHealth = 0;
			}

			if (invincibilityDuration > 0)
			{
				DamageDisabled();
				StartCoroutine(DamageEnabled(invincibilityDuration));
			}

			if (_animator != null)
			{
				_animator.SetTrigger(DamageAnimatorParameter);
			}

			var damageFeedback = DamageMMFeedbacks;
			if (damageFeedback != null)
			{
				var floatingTextFeedback = damageFeedback.Feedbacks.FirstOrDefault(f => f is MMFeedbackFloatingText);

				/* Remove for ScoreFloatingText
				 
				if (floatingTextFeedback != null)
				{
					((MMFeedbackFloatingText)floatingTextFeedback).Value = damage.ToString();
					((MMFeedbackFloatingText)floatingTextFeedback).Channel = 0;
				}*/

				var instantiateFeedbacks = damageFeedback.Feedbacks.Where(f => f is MMFeedbackInstantiateObject);
				foreach (var feedback in instantiateFeedbacks)
				{
					var instantiateFeedback = (MMFeedbackInstantiateObject)feedback;
					instantiateFeedback.transform.rotation = Quaternion.LookRotation(damageDirection);
				}
			}

			damageFeedback?.PlayFeedbacks(this.transform.position);

			if (CurrentHealth <= 0)
			{
				CurrentHealth = 0;
				Kill(whoShootPlayerId, instigator, owner);
			}
		}

		/// <summary>
		/// Kills the character, vibrates the device, instantiates death effects, handles points, etc
		/// </summary>
		/// <param name="instigator"></param>
		/// <param name="owner"></param>
		public virtual void Kill(int whoShootPlayerId, GameObject instigator = null, GameObject owner = null)
		{
			CurrentHealth = 0;
			DamageDisabled();
			DeathMMFeedbacks?.PlayFeedbacks(this.transform.position);

			if (_animator != null)
			{
				_animator.SetTrigger(DeathAnimatorParameter);
				_animator.SetBool(DeathBoolAnimatorParameter, true);
			}

			if (DisableCollisionsOnDeath)
			{
				if (_collider3D != null)
				{
					_collider3D.enabled = false;
				}
			}

			if (ChangeLayerOnDeath)
			{
				gameObject.layer = LayerOnDeath.LayerIndex;
				this.transform.ChangeLayersRecursively(LayerOnDeath.LayerIndex);
			}

			OnDeath?.Invoke(owner?.transform ?? transform, whoShootPlayerId);

			if (DisableModelOnDeath && (Model != null))
			{
				Model.SetActive(false);
			}

			if (DelayBeforeDestruction > 0f)
			{
				Invoke("DestroyObject", DelayBeforeDestruction);
			}
			else
			{
				DestroyObject();
			}
		}

		public virtual void Revive()
		{
			if (!_initialized)
			{
				return;
			}

			if (_collider3D != null)
			{
				_collider3D.enabled = true;
			}

			if (ChangeLayerOnDeath)
			{
				gameObject.layer = _initialLayer;
				this.transform.ChangeLayersRecursively(_initialLayer);
			}

			Initialization();
			SetHealth(MaximumHealth);
			DamageEnabled();
			OnRevive?.Invoke();
		}

		protected virtual void DestroyObject()
		{
			if (DestroyOnDeath)
			{
				Destroy(gameObject, 0.01f);
			}
		}

		/// <summary>
		/// Called when the character gets health (from a stimpack for example)
		/// </summary>
		/// <param name="health">The health the character gets.</param>
		/// <param name="instigator">The thing that gives the character health.</param>
		public virtual void GetHealth(int health, GameObject instigator)
		{
			CurrentHealth = Mathf.Min(CurrentHealth + health, MaximumHealth);
		}

		/// <summary>
		/// Resets the character's health to its max value
		/// </summary>
		public virtual void ResetHealthToMaxHealth()
		{
			CurrentHealth = MaximumHealth;
		}

		/// <summary>
		/// Sets the current health to the specified new value, and updates the health bar
		/// </summary>
		/// <param name="newValue"></param>
		public virtual void SetHealth(int newValue)
		{
			CurrentHealth = newValue;
		}

		/// <summary>
		/// Prevents the character from taking any damage
		/// </summary>
		public virtual void DamageDisabled()
		{
			Invulnerable = true;
		}

		/// <summary>
		/// Allows the character to take damage
		/// </summary>
		public virtual void DamageEnabled()
		{
			Invulnerable = false;
		}

		/// <summary>
		/// makes the character able to take damage again after the specified delay
		/// </summary>
		/// <returns>The layer collision.</returns>
		public virtual IEnumerator DamageEnabled(float delay)
		{
			yield return new WaitForSeconds(delay);
			Invulnerable = false;
		}

		/// <summary>
		/// On Disable, we prevent any delayed destruction from running
		/// </summary>
		protected virtual void OnDisable()
		{
			CancelInvoke();
		}
	}
}