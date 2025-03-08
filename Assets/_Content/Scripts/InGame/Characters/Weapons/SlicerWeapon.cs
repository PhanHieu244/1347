using System.Collections.Generic;
using _Content.InGame.Characters.Abilities;
using _Content.InGame.Enemies;
using _Content.InGame.Managers;
using NaughtyAttributes;
using UnityEngine;

namespace _Content.InGame.Characters.Weapons
{
	public class SlicerWeapon : WeaponBase
	{
		[SerializeField] private float _yOffset = 0.5f;
		[SerializeReference] [Expandable] protected SlicerWeaponSettings _settings;
		[SerializeField] private Slicer _slicerPrefab;
		private Character _character;
		private float _currentCooldownTime;
		private float _timer;
		private CharacterWeaponsHandler _characterWeaponHandler;
		public override int MaxLevel => _settings.GetMaxLevel();

		protected override void OnAwake()
		{
			_character = GetComponentInParent<Character>();
			_characterWeaponHandler = _character.FindAbility<CharacterWeaponsHandler>();
		}

		public override string GetDescriptionForLevel(int level)
		{
			return _settings.GetLevelDescription(level);
		}

		public override void UpdateWeaponLevel(int weaponLevel)
		{
			_weaponLevel = weaponLevel;
			_currentCooldownTime = _settings.GetCooldown(_weaponLevel);
			_timer = .1f;
		}

		protected override float GetCooldown()
		{
			var ratio = _characterWeaponHandler.CooldownRatio;
			return _currentCooldownTime * ratio;
		}

		public override void UpdateWeapon(float deltaTime)
		{
			if (!GameplayManager.Instance.GameStarted)
				return;

			if (_timer > 0f)
			{
				_timer -= deltaTime;
				if (_timer <= 0)
				{
					ShootSlicer();
					_timer = GetCooldown();
					;
				}
			}
		}

		private void ShootSlicer()
		{
			var count = _settings.GetSlicersCount(_weaponLevel);
			var enemies = new List<SimpleEnemyController>();
			for (int i = 0; i < count; i++)
			{
				var enemy = EnemyManager.Instance.GetNearestEnemy(_character.transform.position, 0f, enemies);
				var pos = Vector3.zero;
				if (enemy != null)
				{
					pos = enemy.transform.position;
					enemies.Add(enemy);
				}

				var direction = pos - _character.transform.position;
				direction.y = 0f;
				direction.Normalize();

				var damage = _settings.GetDamage(_weaponLevel);
				pos = _character.transform.position + Vector3.up * _yOffset;
				var slicer = Instantiate(_slicerPrefab, pos, Quaternion.identity);
				slicer.Shoot(pos, direction, damage);
			}
		}
	}
}