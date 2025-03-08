using System.Collections.Generic;
using _Content.InGame.Characters.Talents;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Content.InGame.UI.Main
{
	public class ChoosingAbilityUi: MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _nameText;
		[SerializeField] private Image _icon;
		[SerializeField] private TextMeshProUGUI _levelDescription;
		[SerializeField] private List<GameObject> _stars;
		private NewLevelAbilitiesUi _newLevelAbilitiesUi;
		private CharacterSkill _skill;

		public void Initialize(NewLevelAbilitiesUi newLevelAbilitiesUi)
		{
			_newLevelAbilitiesUi = newLevelAbilitiesUi;
		}

		public void OnButtonClick()
		{
			if (_skill != null)
				_newLevelAbilitiesUi.OnAbilitySelected(_skill);
		}

		public void UpdateAbility(CharacterSkill skillToShow)
		{
			_skill = skillToShow;
			_nameText.text = _skill.Skill.AbilityCaption;
			_icon.sprite = _skill.Skill.AbilityIcon;
			_levelDescription.text = $"{_skill.Skill.GetLevelDescription(_skill.Level)}";
			for (int i = 0; i < _stars.Count; i++)
			{
				_stars[i].SetActive(i < skillToShow.Level);
			}
		}
	}
}