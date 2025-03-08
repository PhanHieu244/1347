using System;
using System.Collections.Generic;
using System.Linq;
using _Content.Events;
using _Content.InGame.Characters.Abilities;
using _Content.InGame.Managers;
using Common.UI;
using UnityEngine;
using EventHandler = Opsive.Shared.Events.EventHandler;

namespace _Content.InGame.UI.Main
{
	public class CurrentSkillsUi: UIViewWrapper
	{
		[SerializeField] private List<SkillSmallDisplayUi> _weaponsList;
		[SerializeField] private List<SkillSmallDisplayUi> _utilityList;

		private void OnEnable()
		{
			EventHandler.RegisterEvent(InGameEvents.CharacterSkillsChanged, OnCharacterSkillsChanged);
		}

		private void OnDisable()
		{
			EventHandler.UnregisterEvent(InGameEvents.CharacterSkillsChanged, OnCharacterSkillsChanged);
		}

		private void OnCharacterSkillsChanged()
		{
			UpdateInformation();
		}

		public override void ShowView(bool force = false)
		{
			base.ShowView(force);
			UpdateInformation();
		}

		private void UpdateInformation()
		{
			var skillHandler = GameplayManager.Instance.Character.FindAbility<CharacterSkillsHandler>();
			var weaponSkills = skillHandler.GetWeaponSkills();
			var utilitySkills = skillHandler.GetUtilitySkills();
			
			for (int i = 0; i < _weaponsList.Count; i++)
			{
				if (i < weaponSkills.Count)
				{
					var charSkill = weaponSkills[i];
					_weaponsList[i].Show(charSkill.Skill.AbilityIcon, charSkill.Level);
				}
				else
				{
					_weaponsList[i].Hide();
				}
			}
			
			for (int i = 0; i < _utilityList.Count; i++)
			{
				if (i < utilitySkills.Count)
				{
					var charSkill = utilitySkills[i];
					_utilityList[i].Show(charSkill.Skill.AbilityIcon, charSkill.Level);
				}
				else
				{
					_utilityList[i].Hide();
				}
			}
		}
	}
}