using System.Collections.Generic;
using System.Linq;
using _Content.Data;
using _Content.InGame.Characters.Talents;
using _Content.InGame.Managers;
using Common.UI;
using TMPro;
using UnityEngine;

namespace _Content.InGame.UI.Main
{
	public class NewLevelAbilitiesUi: UIViewWrapper
	{
		[SerializeField] private TextMeshProUGUI _levelNumberText;
		[SerializeField] private List<ChoosingAbilityUi> _abilities;

		protected override void OnAwake()
		{
			base.OnAwake();
			_abilities.ForEach(a => a.Initialize(this));
		}

		public override void ShowView(bool force = false)
		{
			var level = PlayerData.Instance.CurrentCharacterLevel - GameManager.Instance.NewLevelTimes + 1;
			UpdateAbilities(level);
			_levelNumberText.text = $"{level}";
			base.ShowView(force);
		}

		private void UpdateAbilities(int level)
		{
			var allSkills = CharacterSkillsManager.Instance.GetSkillForNewLevel(level);
			if (allSkills.Count == 0)
			{
				GameManager.Instance.HideNewLevelAbilitiesUi();
				return;
			}
			
			allSkills.Shuffle();
			var count = Mathf.Min(allSkills.Count, CharacterSkillsManager.Instance.SkillsCountPerLevel);
			var skillsToShow = allSkills.Take(count).ToList();
			for (int i = 0; i < _abilities.Count; i++)
			{
				var abilityUi = _abilities[i];
				if (i < count)
				{
					var skillToShow = skillsToShow[i];
					abilityUi.gameObject.SetActive(true);
					abilityUi.UpdateAbility(skillToShow);
				}
				else
				{
					abilityUi.gameObject.SetActive(false);
				}
			}
		}
		public void OnAbilitySelected(CharacterSkill skill)
		{
			CharacterSkillsManager.Instance.AddSkillToCharacter(skill);
			GameManager.Instance.HideNewLevelAbilitiesUi();
		}
	}
}