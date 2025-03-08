using System;
using System.Collections.Generic;
using System.Linq;
using _Content.Data;
using _Content.InGame.Characters;
using _Content.InGame.Characters.Abilities;
using _Content.InGame.Characters.Talents;
using Base;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Content.InGame.Managers
{
	public class CharacterSkillsManager : Singleton<CharacterSkillsManager>
	{
		[SerializeField] private int _skillsCountPerLevel = 2;
		[SerializeReference] private List<TalentBase> _skills;
		private Character _character;
		public int SkillsCountPerLevel => _skillsCountPerLevel;

		public Character GetCharacter()
		{
			if (_character == null)
			{
				var character = FindObjectOfType<Character>();
				if (character != null)
					_character = character;
			}

			return _character;
		}

		public bool IsTalentUnlocked(string talentName)
		{
			return PlayerData.Instance.IsTalentUnlocked(talentName) ||
			       _skills.FirstOrDefault(s => s.AbilityName == talentName && s.UnlockedByDefault);
		}

		public void AddSkillToCharacter(CharacterSkill skill)
		{
			var character = GetCharacter();
			var skillsHandler = character.FindAbility<CharacterSkillsHandler>();
			skillsHandler.UpgradeTalent(skill.Skill);

			/*AppMetricaEvents.SendUpgradeCharacterEvent(PlayerData.Instance.LevelNumber, PlayerData.Instance.LevelCount,
				PlayerData.Instance.CurrentCharacterLevel, skill.Skill.AbilityName, skill.Level);*/
		}

		public bool HasSkillToUpgrade()
		{
			var character = GetCharacter();
			var skillsHandler = character.FindAbility<CharacterSkillsHandler>();
			var weaponSkills = skillsHandler.CurrentSkills.Where(s => s.Skill is WeaponTalentBase).ToList();
			var availableToUpgradeWeaponSkills = weaponSkills.Count(s => s.Level < s.Skill.LevelsCount);
			var utilitySkills = skillsHandler.CurrentSkills.Where(s => s.Skill is not WeaponTalentBase).ToList();
			var availableToUpgradeUtilitySkills = utilitySkills.Count(s => s.Level < s.Skill.LevelsCount);
			if (weaponSkills.Count < skillsHandler.WeaponSkillsMaxCount || availableToUpgradeWeaponSkills > 0 ||
			    utilitySkills.Count < skillsHandler.UtilitySkillsMaxCount || availableToUpgradeUtilitySkills > 0)
				return true;

			return false;
		}

		public List<CharacterSkill> GetSkillForNewLevel(int level)
		{
			var character = GetCharacter();
			var skillsHandler = character.FindAbility<CharacterSkillsHandler>();
			var result = new List<CharacterSkill>();

			var currentWeaponSkills = skillsHandler.GetWeaponSkills();
			var currentUtilitySkills = skillsHandler.GetUtilitySkills();

			var availableToUpgradeWeaponSkills = currentWeaponSkills.Where(s => s.Level < s.Skill.LevelsCount).ToList();
			var availableToUpgradeUtilitySkills =
				currentUtilitySkills.Where(s => s.Level < s.Skill.LevelsCount).ToList();

			var newWeaponSkills = _skills.Where(s =>
				s is WeaponTalentBase && IsTalentUnlocked(s.AbilityName) &&
				currentWeaponSkills.All(cs => cs.Skill != s)).ToList();
			var newUtilitySkills = _skills.Where(s =>
					s is not WeaponTalentBase && IsTalentUnlocked(s.AbilityName) &&
					currentUtilitySkills.All(cs => cs.Skill != s))
				.ToList();

			if (level == 2)
			{
				var newWeaponSkill1 = newWeaponSkills.PopRandomElement();
				var newWeaponSkill2 = newWeaponSkills.PopRandomElement();
				var utilitySkill = newUtilitySkills.GetRandomElement();
				result.Add(new CharacterSkill(newWeaponSkill1, 1));
				result.Add(new CharacterSkill(newWeaponSkill2, 1));
				result.Add(new CharacterSkill(utilitySkill, 1));
				return result;
			}
			else if (level == 3)
			{
				result = GetRandomSkills(skillsHandler, currentWeaponSkills, currentUtilitySkills,
					availableToUpgradeWeaponSkills, availableToUpgradeUtilitySkills, newWeaponSkills, newUtilitySkills);

				if (result.Any(s =>
					    s.Skill.AbilityName.Equals("orbital_knives", StringComparison.InvariantCultureIgnoreCase)))
					return result;
				
				var knives = availableToUpgradeWeaponSkills.FirstOrDefault(s =>
					s.Skill.AbilityName.Equals("orbital_knives", StringComparison.InvariantCultureIgnoreCase));
				if (knives != null)
				{
					result.RemoveAt(0);
					result.Add(new CharacterSkill(knives.Skill, knives.Level + 1));
				}

				return result;
			}
			else if (level == 4)
			{
				if (availableToUpgradeUtilitySkills.Count == 0)
				{
					var utilitySkill1 = newUtilitySkills.PopRandomElement();
					var utilitySkill2 = newUtilitySkills.PopRandomElement();
					var upgradeWeaponSkill = availableToUpgradeWeaponSkills.PopRandomElement();
					result.Add(new CharacterSkill(utilitySkill1, 1));
					result.Add(new CharacterSkill(utilitySkill2, 1));
					result.Add(new CharacterSkill(upgradeWeaponSkill.Skill, upgradeWeaponSkill.Level + 1));
				}
				else
				{
					return GetRandomSkills(skillsHandler, currentWeaponSkills, currentUtilitySkills,
						availableToUpgradeWeaponSkills, availableToUpgradeUtilitySkills, newWeaponSkills,
						newUtilitySkills);
				}
			}
			else if (level == 5)
			{
				if (availableToUpgradeUtilitySkills.Count == 0)
				{
					var utilitySkill1 = newUtilitySkills.PopRandomElement();
					var utilitySkill2 = newUtilitySkills.PopRandomElement();
					var utilitySkill3 = newUtilitySkills.PopRandomElement();
					result.Add(new CharacterSkill(utilitySkill1, 1));
					result.Add(new CharacterSkill(utilitySkill2, 1));
					result.Add(new CharacterSkill(utilitySkill3, 1));
				}
				else
				{
					return GetRandomSkills(skillsHandler, currentWeaponSkills, currentUtilitySkills,
						availableToUpgradeWeaponSkills, availableToUpgradeUtilitySkills, newWeaponSkills,
						newUtilitySkills);
				}
			}

			return GetRandomSkills(skillsHandler, currentWeaponSkills, currentUtilitySkills,
				availableToUpgradeWeaponSkills, availableToUpgradeUtilitySkills, newWeaponSkills, newUtilitySkills);
		}

		private List<CharacterSkill> GetRandomSkills(CharacterSkillsHandler skillsHandler,
			List<CharacterSkill> currentWeaponSkills, List<CharacterSkill> currentUtilitySkills,
			List<CharacterSkill> availableToUpgradeWeaponSkills, List<CharacterSkill> availableToUpgradeUtilitySkills,
			List<TalentBase> newWeaponSkills, List<TalentBase> newUtilitySkills)
		{
			var result = new List<CharacterSkill>();
			var tries = 0;
			while (result.Count < 3 && tries < 300)
			{
				var random = Random.Range(0, 4);

				if (random == 0 && availableToUpgradeWeaponSkills.Count > 0)
				{
					var randomExistedWeaponSkill = availableToUpgradeWeaponSkills
						.Where(s => result.All(resultSkill => s.Skill != resultSkill.Skill)).ToList();

					if (randomExistedWeaponSkill.Count > 0)
					{
						var skill = randomExistedWeaponSkill.GetRandomElement();
						result.Add(new CharacterSkill(skill.Skill,
							skill.Level + 1));
					}
				}
				else if (random == 1 && availableToUpgradeUtilitySkills.Count > 0)
				{
					var randomExistedUtilitySkill = availableToUpgradeUtilitySkills
						.Where(s => result.All(resultSkill => s.Skill != resultSkill.Skill)).ToList();

					if (randomExistedUtilitySkill.Count > 0)
					{
						var skill = randomExistedUtilitySkill.GetRandomElement();
						result.Add(new CharacterSkill(skill.Skill,
							skill.Level + 1));
					}
				}
				else if (random == 2 && newWeaponSkills.Count > 0 &&
				         currentWeaponSkills.Count < skillsHandler.WeaponSkillsMaxCount)
				{
					var skills = newWeaponSkills
						.Where(s => result.All(resultSkill => s != resultSkill.Skill)).ToList();
					if (skills.Count > 0)
					{
						var skill = skills.GetRandomElement();
						result.Add(new CharacterSkill(skill, 1));
					}
				}
				else if (random == 3 && currentUtilitySkills.Count < skillsHandler.UtilitySkillsMaxCount)
				{
					var skills = newUtilitySkills
						.Where(s => result.All(resultSkill => s != resultSkill.Skill)).ToList();
					if (skills.Count > 0)
					{
						var skill = skills.GetRandomElement();
						result.Add(new CharacterSkill(skill, 1));
					}
				}

				tries++;
			}

			return result;
		}

		/*public List<CharacterSkill> GetAllAvailableSkills()
		{
			var character = GetCharacter();
			var skillsHandler = character.FindAbility<CharacterSkillsHandler>();
			var result = new List<CharacterSkill>();

			var weaponSkills = skillsHandler.GetWeaponSkills();
			var availableToUpgradeWeaponSkills = weaponSkills.Where(s => s.Level < s.Skill.LevelsCount).ToList();
			var utilitySkills = skillsHandler.GetUtilitySkills();
			var availableToUpgradeUtilitySkills = utilitySkills.Where(s => s.Level < s.Skill.LevelsCount).ToList();

			if (availableToUpgradeWeaponSkills.Count > 0)
			{
				var randomExistedWeaponSkill = availableToUpgradeWeaponSkills.GetRandomElement();
				result.Add(new CharacterSkill()
					{ Skill = randomExistedWeaponSkill.Skill, Level = randomExistedWeaponSkill.Level + 1 });
			}

			if (availableToUpgradeUtilitySkills.Count > 0)
			{
				var randomExistedUtilitySkill = availableToUpgradeUtilitySkills.GetRandomElement();
				result.Add(new CharacterSkill()
					{ Skill = randomExistedUtilitySkill.Skill, Level = randomExistedUtilitySkill.Level + 1 });
			}

			if (weaponSkills.Count < skillsHandler.WeaponSkillsMaxCount)
			{
				var weapon = GetRandomNewWeaponSkill(weaponSkills);
				if (weapon != null)
					result.Add(weapon);
			}

			if (utilitySkills.Count < skillsHandler.WeaponSkillsMaxCount)
			{
				var utility = GetRandomNewUtilitySkill(weaponSkills);
				if (utility != null)
					result.Add(utility);
			}

			var tries = 0;
			while (result.Count < 3 && tries < 300)
			{
				var random = Random.Range(0, 4);

				if (random == 0 && availableToUpgradeUtilitySkills.Count > 0)
				{
					var randomExistedWeaponSkill = availableToUpgradeWeaponSkills
						.Where(s => result.All(resultSkill => s.Skill != resultSkill.Skill)).ToList()
						.GetRandomElement();
					result.Add(new CharacterSkill()
						{ Skill = randomExistedWeaponSkill.Skill, Level = randomExistedWeaponSkill.Level + 1 });
				}
				else if (random == 1 && availableToUpgradeUtilitySkills.Count > 0)
				{
					var randomExistedUtilitySkill = availableToUpgradeUtilitySkills
						.Where(s => result.All(resultSkill => s.Skill != resultSkill.Skill)).ToList()
						.GetRandomElement();
					result.Add(new CharacterSkill()
						{ Skill = randomExistedUtilitySkill.Skill, Level = randomExistedUtilitySkill.Level + 1 });
				}
				else if (random == 2 && weaponSkills.Count < skillsHandler.WeaponSkillsMaxCount)
				{
					var weapon = GetRandomNewWeaponSkill(weaponSkills.Union(result).ToList());
					if (weapon != null)
						result.Add(weapon);
				}
				else if (random == 3 && utilitySkills.Count < skillsHandler.WeaponSkillsMaxCount)
				{
					var utility = GetRandomNewUtilitySkill(weaponSkills.Union(result).ToList());
					if (utility != null)
						result.Add(utility);
				}

				tries++;
			}

			return result;
		}

		private CharacterSkill GetRandomNewWeaponSkill(List<CharacterSkill> existingSkills)
		{
			var skills = _skills.Where(s => IsTalentUnlocked(s.AbilityName)).ToList();
			var skill = skills.Where(w => w is WeaponTalentBase && existingSkills.All(s => s.Skill != w)).ToList()
				.GetRandomElement();
			if (skill != null)
				return new CharacterSkill() { Skill = skill, Level = 1 };

			return null;
		}

		private CharacterSkill GetRandomNewUtilitySkill(List<CharacterSkill> existingSkills)
		{
			var skills = _skills.Where(s => IsTalentUnlocked(s.AbilityName)).ToList();
			var skill = skills.Where(w => w is not WeaponTalentBase && existingSkills.All(s => s.Skill != w)).ToList()
				.GetRandomElement();

			if (skill != null)
				return new CharacterSkill() { Skill = skill, Level = 1 };

			return null;
		}*/

		public TalentBase GetSkill(string abilityName)
		{
			return _skills.FirstOrDefault(s => s.AbilityName == abilityName);
		}
	}
}