using _Content.InGame.Characters.Abilities;
using _Content.InGame.Managers;
using NaughtyAttributes;
using UnityEngine;

namespace _Content.InGame.Characters.Weapons
{
	public class KatanaWeapon : WeaponBase
	{
		[SerializeField] private float _rotationTime;
		[SerializeField] private AnimationCurve _rotationCurve;
		[SerializeField] private float _characterOffset = 1f;
		[SerializeReference] [Expandable] protected KatanaWeaponSettings _settings;
		[SerializeField] private Katana _katanaPrefab;
		private Character _character;
		private float _currentCooldownTime;
		private float _timer;
		private CharacterWeaponsHandler _characterWeaponHandler;
		private Transform _holder;
		public override int MaxLevel => _settings.GetMaxLevel();

		protected override void OnAwake()
		{
			_character = GetComponentInParent<Character>();
			_characterWeaponHandler = _character.FindAbility<CharacterWeaponsHandler>();
			_character = GetComponentInParent<Character>();
			if (_character != null)
			{
				if (_holder == null)
				{
					_holder = new GameObject("KatanaHolder").transform;
					_holder.SetParent(_character.transform);
					_holder.localPosition = Vector3.up * 1f;
				}
			}
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
					ShootKatana();
					_timer = GetCooldown();
				}
			}
		}

		private void ShootKatana()
		{
			var enemy = EnemyManager.Instance.GetNearestEnemy(_character.transform.position, 0f);
			var pos = _character.transform.position + Vector3.left;

			if (enemy != null)
				pos = enemy.transform.position;

			var direction = pos - _character.transform.position;
			direction.y = 0f;
			direction.Normalize();

			var time = _settings.GetLoopTime(_weaponLevel);
			var timeOneSlice = _settings.GetTimeForOneSlice(_weaponLevel);
			var damage = _settings.GetDamage(_weaponLevel);
			pos = _character.transform.position + direction * _characterOffset;
			if (_settings.OnlyUp)
				pos = _character.transform.position + Vector3.forward * _characterOffset;
			var angleRange = _settings.GetAttackAngle(_weaponLevel);
			var slicesCount = _settings.GetSlicesCount(_weaponLevel);
			var katana = Instantiate(_katanaPrefab, pos, Quaternion.identity);
			katana.transform.SetParent(_holder);
			katana.Shoot(direction, angleRange, damage, timeOneSlice, _rotationCurve, time, _characterOffset, enemy, _settings.OnlyUp, slicesCount);
		}
	}
}