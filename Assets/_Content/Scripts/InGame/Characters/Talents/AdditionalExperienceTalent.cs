using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Content.InGame.Characters.Talents
{
	[CreateAssetMenu(menuName = "Abilities/Experience")]
	public class AdditionalExperienceTalent: TalentBase
	{
		public List<float> _experienceFactorPerLevel;
		public override int LevelsCount => _experienceFactorPerLevel.Count;

		public float GetExperienceFactorPerLevel(int level)
		{
			if (level >= _experienceFactorPerLevel.Count)
				level = _experienceFactorPerLevel.Count;
			
			return _experienceFactorPerLevel.ElementAt(level - 1);
		}

		public override string GetLevelDescription(int level)
		{
			var factor = GetExperienceFactorPerLevel(level);
			if (level > 1)
			{
				var prevAddition = GetExperienceFactorPerLevel(level - 1);
				factor -= prevAddition;
			}
			else
			{
				factor -= 1f;
			}

			var percent = Mathf.RoundToInt(factor * 100f);
			return $"+{percent}% experience";
		}
	}
}