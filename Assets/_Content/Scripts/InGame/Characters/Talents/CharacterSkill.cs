using System;

namespace _Content.InGame.Characters.Talents
{
	[Serializable]
	public class CharacterSkill
	{
		public TalentBase Skill;
		public int Level;

		public CharacterSkill(TalentBase skill, int level)
		{
			Skill = skill;
			Level = level;
		}
	}
}