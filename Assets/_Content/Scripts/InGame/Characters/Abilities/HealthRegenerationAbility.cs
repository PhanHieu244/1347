using _Content.Events;
using _Content.InGame.Damage;
using _Content.InGame.Managers;
using NaughtyAttributes;
using Opsive.Shared.Events;
using UnityEngine;

namespace _Content.InGame.Characters.Abilities
{
	public class HealthRegenerationAbility: CharacterAbility
	{
		[SerializeField] [ReadOnly] private float _heathRegeneration;
		private Health _health;
		[SerializeField] [ReadOnly]private float _healthToRegenerate;

		public override void Setup()
		{
			base.Setup();
			_health = _character.Health;
			_heathRegeneration = 0f;
			_healthToRegenerate = 0f;
		}

		public override void ProcessAbility()
		{
			base.ProcessAbility();
			var deltaTime = Time.deltaTime * GameManager.Instance.TimeScale;
			if (_health.CurrentHealth < _health.MaximumHealth && _heathRegeneration > 0f)
			{
				var heathRegenerationDelta = _heathRegeneration * _health.MaximumHealth * deltaTime / 60f;
				_healthToRegenerate += heathRegenerationDelta;
				if (_healthToRegenerate >= 1f)
				{
					_health.CurrentHealth += 1;
					EventHandler.ExecuteEvent(InGameEvents.CharacterHealthChanged);
					_healthToRegenerate -= 1f;
				}
			}
		}

		public void SetRegeneration(float healthRegeneration)
		{
			_heathRegeneration = healthRegeneration;
		}
	}
}