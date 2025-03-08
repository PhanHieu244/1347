using System.Collections.Generic;
using System.Linq;
using _Content.InGame.Characters.Talents;
using UnityEngine;

namespace _Content.InGame.Characters.Abilities
{
	[CreateAssetMenu(menuName = "Abilities/Cooldown Reduction")]
	public class WeaponCooldownReductionTalent: TalentBase
	{
		public List<float> _cooldownRatioPerLevel;
		public override int LevelsCount => _cooldownRatioPerLevel.Count;
		public override string GetLevelDescription(int level)
		{
			var ratio = GetCooldownRatio(level);
			if (level > 1)
			{
				var prevAddition = GetCooldownRatio(level - 1);
				ratio = prevAddition - ratio;
			}
			else
			{
				ratio = 1f - ratio;
			}

			var percent = Mathf.RoundToInt(ratio * 100f);
			return $"-{percent}% cooldown";
		}

		public float GetCooldownRatio(int level)
		{
			if (level >= _cooldownRatioPerLevel.Count)
				level = _cooldownRatioPerLevel.Count;
			
			return _cooldownRatioPerLevel.ElementAt(level - 1);
		}
	}
}