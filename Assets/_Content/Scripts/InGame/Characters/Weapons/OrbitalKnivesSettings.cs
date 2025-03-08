using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Content.InGame.Characters.Weapons
{
	[CreateAssetMenu(menuName = "Weapons/Orbital Knives Settings")]
	public class OrbitalKnivesSettings: WeaponSettings
	{
		[Serializable]
		private class LevelInfo
		{
			public int Level;
			public int KnivesCount;
			public float RotationSpeed = -70f;
			public float Cooldown = 1f;
			public float LifeTime = 1f;
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
		
		public int GetKnivesCount(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.KnivesCount ?? 1;
		}

		public float GetKnivesRotationSpeed(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.RotationSpeed ?? 1f;
		}

		public float GetCooldown(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.Cooldown ?? 1f;
		}
		public float GetLifeTime(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.LifeTime ?? 1f;
		}
	}
}