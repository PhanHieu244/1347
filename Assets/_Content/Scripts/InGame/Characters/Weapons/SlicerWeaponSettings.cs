using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Content.InGame.Characters.Weapons
{
	[CreateAssetMenu(menuName = "Weapons/Slicer Weapon Settings")]
	public class SlicerWeaponSettings : WeaponSettings
	{
		[Serializable]
		private class LevelInfo
		{
			public int Level;
			public int SlicersCount;
			public int Damage = 1;
			public float Cooldown = 5f;
			public string DescriptionInLevelUp;
		}

		[SerializeField] private List<LevelInfo> _levels;

		public override string GetLevelDescription(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.DescriptionInLevelUp ?? string.Empty;
		}
		
		public override int GetMaxLevel()
		{
			return _levels.Max(l => l.Level);
		}

		public float GetCooldown(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.Cooldown ?? 1f;
		}

		public int GetSlicersCount(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.SlicersCount ?? 1;
		}

		public int GetDamage(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.Damage ?? 1;
		}
	}
}