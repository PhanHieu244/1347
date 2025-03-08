using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace _Content.InGame.Characters.Talents
{
	[CreateAssetMenu(menuName = "Abilities/Health regeneration")]
	public class HealthRegenerationTalent: TalentBase
	{
		[Label("Regeneration Per Minute")]
		public List<float> _healthRegenerationPerLevel;
		public override int LevelsCount => _healthRegenerationPerLevel.Count;

		public float GetHealthRegeneration(int level)
		{
			if (level >= _healthRegenerationPerLevel.Count)
				level = _healthRegenerationPerLevel.Count;
			
			return _healthRegenerationPerLevel.ElementAt(level - 1);
		}
		
		public override string GetLevelDescription(int level)
		{
			var ratio = GetHealthRegeneration(level);
			var percent = Mathf.FloorToInt(ratio * 100f) / 60f;
			return $"+{percent:F1}% health / sec";
		}
	}
}