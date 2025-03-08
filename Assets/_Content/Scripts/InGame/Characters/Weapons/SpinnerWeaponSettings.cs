using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Content.InGame.Characters.Weapons
{
	[CreateAssetMenu(menuName = "Weapons/Spinner Weapon Settings")]
	public class SpinnerWeaponSettings : WeaponSettings
	{
		[Serializable]
		private class LevelInfo
		{
			public int Level;
			public int BladesCount = 1;
			public int BlenderCount = 1;
			public float LifeTime = 5f;
			public float Cooldown = 5f;
			public float RotationSpeed = -360f;
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

		public int GetBlenderCount(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.BlenderCount ?? 1;
		}

		public int GetBladesCount(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.BladesCount ?? 1;
		}

		public float GetRotationSpeed(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.RotationSpeed ?? 1f;
		}

		public float GetLifeTime(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.LifeTime ?? 1f;
		}

		public float GetCooldown(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.Cooldown ?? 1f;
		}
	}
}