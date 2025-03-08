using _Content.Events;
using _Content.InGame.Characters.Abilities;
using _Content.InGame.Managers;
using NaughtyAttributes;
using UnityEngine;

namespace _Content.InGame.Characters.Weapons
{
	public class DroneWeapon : WeaponBase
	{
		[SerializeReference] [Expandable] protected DroneWeaponSettings _settings;
		[SerializeField] private Drone _dronePrefab;
		private Character _character;
		private CharacterWeaponsHandler _characterWeaponHandler;
		private float _currentCooldownTime;
		private float _timer;
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
			if (weaponLevel > 1)
				_timer = _currentCooldownTime;
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
					CreateDrone();
					_timer = GetCooldown();
				}
			}
		}

		private void CreateDrone()
		{
			var random = Random.insideUnitSphere;
			random.y = 0f;
			random = random.normalized * Random.Range(0.5f, 1f);
			var lifeTime = _settings.GetLifeTime(_weaponLevel);
			var damage = _settings.GetDamage(_weaponLevel);
			var position = _character.transform.position + random;
			var mine = Instantiate(_dronePrefab, position, Quaternion.identity);
			mine.SetProperties(lifeTime, damage);

			var enemy = EnemyManager.Instance.GetNearestEnemy(_character.transform.position, 0f);
			var dir = Random.insideUnitSphere;
			if (enemy != null)
			{
				dir = enemy.transform.position - _character.transform.position;
			}

			dir.y = 0f;

			var dest = position + dir.normalized * Random.Range(3f, 7f);
			mine.StartDrone();
		}
	}
}