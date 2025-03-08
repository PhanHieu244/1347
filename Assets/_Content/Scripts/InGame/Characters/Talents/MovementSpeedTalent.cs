using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Content.InGame.Characters.Talents
{
	[CreateAssetMenu(menuName = "Abilities/Movement")]
	public class MovementSpeedTalent: TalentBase
	{
		public List<float> _speedRatioPerLevel;
		public override int LevelsCount => _speedRatioPerLevel.Count;

		public float GetMovementSpeedRatio(int level)
		{
			return _speedRatioPerLevel.ElementAt(level - 1);
		}
		
		public override string GetLevelDescription(int level)
		{
			var speedRatio = GetMovementSpeedRatio(level);
			if (level > 1)
			{
				var prevAddition = GetMovementSpeedRatio(level - 1);
				speedRatio -= prevAddition;
			}
			else
			{
				speedRatio -= 1f;
			}

			var percent = Mathf.RoundToInt(speedRatio * 100f);
			return $"+{percent}% speed";
		}
	}
}