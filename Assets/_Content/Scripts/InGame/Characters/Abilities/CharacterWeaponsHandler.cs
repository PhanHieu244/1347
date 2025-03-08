using System;
using System.Collections.Generic;
using System.Linq;
using _Content.Events;
using _Content.InGame.Characters.Weapons;
using _Content.InGame.Managers;
using UnityEngine;
using EventHandler = Opsive.Shared.Events.EventHandler;

namespace _Content.InGame.Characters.Abilities
{
	public class CharacterWeaponsHandler : CharacterAbility
	{
		[SerializeField] private Transform _weaponParent;
		private List<WeaponBase> _currentWeapons;
		private float _cooldownRatio = 1f;

		public float CooldownRatio => _cooldownRatio;
		public List<WeaponBase> CurrentWeapons => _currentWeapons;

		protected override void Initialization()
		{
			base.Initialization();
			_currentWeapons = new List<WeaponBase>();
		}

		private void OnEnable()
		{
			EventHandler.RegisterEvent(InGameEvents.GameStart, OnGameStartChanged);
		}

		private void OnDisable()
		{
			EventHandler.UnregisterEvent(InGameEvents.GameStart, OnGameStartChanged);
		}

		private void OnGameStartChanged()
		{
			if (GameplayManager.Instance.GameStarted)
			{
				foreach (var weapon in _currentWeapons)
				{
					weapon.UpdateWeaponLevel(1);
				}
			}
		}

		public override void Setup()
		{
			base.Setup();

			_cooldownRatio = 1f;
			_currentWeapons.Clear();
			_currentWeapons.AddRange(_character.GetComponentsInChildren<WeaponBase>());
			/*foreach (var weapon in _currentWeapons)
			{
				weapon.UpdateWeaponLevel(1);
			}*/
		}

		public void AddWeapon(WeaponBase weaponPrefab)
		{
			var newWeapon = Instantiate(weaponPrefab, _weaponParent);
			_currentWeapons.Add(newWeapon);
			if (GameplayManager.Instance.GameStarted)
				newWeapon.UpdateWeaponLevel(1);
		}

		public override void ProcessAbility()
		{
			base.ProcessAbility();
			if (GameplayManager.Instance.GameStarted)
			{
				var deltaTime = Time.deltaTime * GameManager.Instance.TimeScale;
				_currentWeapons.ForEach(w => w.UpdateWeapon(deltaTime));
			}
		}

		public void UpdateWeaponLevel(string weaponName, int level)
		{
			var weapon = _currentWeapons.FirstOrDefault(w => w.WeaponName == weaponName);
			if (weapon != null)
				weapon.UpdateWeaponLevel(level);
		}

		public void SetCooldownRatio(float ratio)
		{
			_cooldownRatio = ratio;
		}
	}
}