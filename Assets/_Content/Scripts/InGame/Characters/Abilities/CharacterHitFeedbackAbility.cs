using System;
using _Content.InGame.Damage;
using _Content.InGame.Managers;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace _Content.InGame.Characters.Abilities
{
	public class CharacterHitFeedbackAbility: CharacterAbility
	{
		[SerializeField] private float _range;
		[SerializeField] private float _distance;
		
		private Health _health;

		protected override void Initialization()
		{
			base.Initialization();
			_health = GetComponentInParent<Health>();
		}

		private void OnEnable()
		{
			_health.OnHit += OnHit;
		}

		private void OnDisable()
		{
			_health.OnHit -= OnHit;
		}

		private void OnHit(int arg1, int arg2)
		{
			if (_health.CurrentHealth <= 0)
				return;

			EnemyManager.Instance.OnPlayerDamage(transform.position, _range, _distance);
		}
	}
}