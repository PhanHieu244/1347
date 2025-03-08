using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Content.InGame.Characters.Weapons
{
	[CreateAssetMenu(menuName = "Weapons/Drone Weapon Settings")]
	public class DroneWeaponSettings: WeaponSettings
	{
		[Serializable]
		private class LevelInfo
		{
			public int Level;
			public int Damage;
			public float LifeTime = 5f;
			public float Cooldown = 5f;
			public string DescriptionInLevelUp;
		}

		[SerializeField] private List<LevelInfo> _levels;

		public override int GetMaxLevel()
		{
			return _levels.Max(l => l.Level);
		}

		public override string GetLevelDescription(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.DescriptionInLevelUp ?? string.Empty;
		}

		public int GetDamage(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.Damage ?? 1;
		}

		public float GetLifeTime(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.LifeTime ?? 1;
		}

		public float GetCooldown(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.Cooldown ?? 1f;
		}
	}
}