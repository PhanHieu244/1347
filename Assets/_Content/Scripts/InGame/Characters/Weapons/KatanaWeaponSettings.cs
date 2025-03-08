using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Content.InGame.Characters.Weapons
{
	[CreateAssetMenu(menuName = "Weapons/Katana Weapon Settings")]
	public class KatanaWeaponSettings : WeaponSettings
	{
		[Serializable]
		private class LevelInfo
		{
			public int Level;
			public int SlicesCount = 2;
			public float Angle = 45f;
			public int Damage = 1;
			public float Time;
			public float TimeForOneSlice = 0.25f;
			public float Cooldown = 5f;
			public string DescriptionInLevelUp;
		}

		[SerializeField] private bool _onlyUp;
		[SerializeField] private List<LevelInfo> _levels;
		public bool OnlyUp => _onlyUp;
		public override string GetLevelDescription(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.DescriptionInLevelUp ?? string.Empty;
		}
		public override int GetMaxLevel()
		{
			return _levels.Max(l => l.Level);
		}

		public float GetLoopTime(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.Time ?? 1;
		}
		public float GetTimeForOneSlice(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.TimeForOneSlice ?? 1;
		}

		public float GetAttackAngle(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.Angle ?? 1;
		}

		public int GetDamage(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.Damage ?? 1;
		}

		public float GetCooldown(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.Cooldown ?? 1f;
		}

		public int GetSlicesCount(int level)
		{
			return _levels.FirstOrDefault(l => l.Level == level)?.SlicesCount ?? 1;
		}
	}
}