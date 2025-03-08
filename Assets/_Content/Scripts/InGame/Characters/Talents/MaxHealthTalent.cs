using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Content.InGame.Characters.Talents
{
	[CreateAssetMenu(menuName = "Abilities/Max Health")]
	public class MaxHealthTalent: TalentBase
	{
		public List<int> _maxHealthPerLevel;
		public override int LevelsCount => _maxHealthPerLevel.Count;

		public int GetMaxHealthAddition(int existedTalentLevel)
		{
			return _maxHealthPerLevel.ElementAt(existedTalentLevel - 1);
		}
		public override string GetLevelDescription(int level)
		{
			return $"+10% max health";
		}
	}
}