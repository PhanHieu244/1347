using System.Collections.Generic;
using _Content.Events;
using _Content.InGame.Characters.Abilities;
using _Content.InGame.Enemies;
using _Content.InGame.Managers;
using NaughtyAttributes;
using Opsive.Shared.Events;
using UnityEngine;

namespace _Content.InGame.Characters.Weapons
{
	public class SpinnerWeapon : WeaponBase
	{
		[SerializeReference] [Expandable] protected SpinnerWeaponSettings _settings;
		[SerializeField] private Spinner _minePrefab;
		private Character _character;
		private float _timer;
		private float _currentCooldownTime;
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
			EventHandler.ExecuteEvent(InGameEvents.SpinnerBladesCountChanged, _settings.GetBladesCount(_weaponLevel));
			_timer = 0f;
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
			if (_timer >= 0f)
			{
				_timer -= deltaTime;
				if (_timer <= 0)
				{
					DropTheMine();
					_timer = GetCooldown();
				}
			}
		}

		private void DropTheMine()
		{
			var count = _settings.GetBlenderCount(_weaponLevel);
			var enemies = new List<SimpleEnemyController>();
			for (int i = 0; i < count; i++)
			{
				var random = Random.insideUnitSphere;
				random.y = 0f;
				random = random.normalized * Random.Range(0.5f, 1f);
				var lifeTime = _settings.GetLifeTime(_weaponLevel);
				var rotationSpeed = _settings.GetRotationSpeed(_weaponLevel);
				var bladesCount = _settings.GetBladesCount(_weaponLevel);
				var position = _character.transform.position + random;
				var mine = Instantiate(_minePrefab, position, Quaternion.identity);
				mine.SetProperties(lifeTime, rotationSpeed, bladesCount);

				var enemy = EnemyManager.Instance.GetNearestEnemy(_character.transform.position, 0f, enemies);
				var dir = Random.insideUnitSphere;
				if (enemy != null)
				{
					enemies.Add(enemy);
					dir = enemy.transform.position - _character.transform.position;
				}

				dir.y = 0f;

				var dest = position + dir.normalized * Random.Range(3f, 7f);
				mine.Drop(dest);
			}
		}
	}
}