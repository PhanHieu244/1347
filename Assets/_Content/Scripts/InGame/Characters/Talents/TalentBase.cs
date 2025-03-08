using UnityEngine;

namespace _Content.InGame.Characters.Talents
{
	public abstract class TalentBase: ScriptableObject
	{
		[SerializeField] protected string _abilityName;
		[SerializeField] protected string _abilityCaption;
		[SerializeField] protected string _abilityDescription;
		[SerializeField] protected bool _unlockedByDefault;
		[SerializeField] protected Sprite _abilityIcon;
		
		public abstract int LevelsCount { get; }

		public string AbilityDescription => _abilityDescription;
		public string AbilityCaption => _abilityCaption;
		public string AbilityName => _abilityName;
		public Sprite AbilityIcon => _abilityIcon;
		public bool UnlockedByDefault => _unlockedByDefault;

		public abstract string GetLevelDescription(int level);
	}
}