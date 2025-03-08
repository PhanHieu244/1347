using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Content.InGame.Characters.Weapons
{
	[CreateAssetMenu(menuName = "Weapons/Pizza Knives Weapon Settings")]
	public class PizzaKnivesWeaponSettings: WeaponSettings
	{
		[Serializable]
		private class LevelInfo
		{
			public int Level;
			public int KnivesCount;
			public int Damage = 1;
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

		public int GetKnivesCount(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.KnivesCount ?? 1;
		}

		public int GetDamage(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.Damage ?? 1;
		}

		public float GetCooldown(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.Cooldown ?? 1f;
		}
	}
}