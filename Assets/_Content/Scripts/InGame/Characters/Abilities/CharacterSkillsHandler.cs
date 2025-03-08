using System.Collections.Generic;
using System.Linq;
using _Content.Events;
using _Content.InGame.Characters.Talents;
using _Content.InGame.Damage;
using _Content.InGame.Managers;
using NaughtyAttributes;
using Opsive.Shared.Events;
using UnityEngine;

namespace _Content.InGame.Characters.Abilities
{
	public class CharacterSkillsHandler : CharacterAbility
	{
		[SerializeField] private int _weaponSkillsMaxCount = 3;
		[SerializeField] private int _utilitySkillsMaxCount = 3;
		[SerializeField] private List<TalentBase> _initialTalents;
		[SerializeField] [ReadOnly] private List<CharacterSkill> _currentSkills;
		private CharacterWeaponsHandler _weaponHandler;
		private Health _health;
		private CharacterMovement _movementAbility;
		private HealthRegenerationAbility _healthRegenerationAbility;
		private CharacterExperienceAbility _characterExperienceAbility;

		public int WeaponSkillsMaxCount => _weaponSkillsMaxCount;
		public int UtilitySkillsMaxCount => _utilitySkillsMaxCount;
		public List<CharacterSkill> CurrentSkills => _currentSkills;

		protected override void Initialization()
		{
			base.Initialization();
			_currentSkills = new List<CharacterSkill>();
		}

		public override void Setup()
		{
			base.Setup();
			_weaponHandler = _character.FindAbility<CharacterWeaponsHandler>();
			_health = _character.GetComponent<Health>();
			_movementAbility = _character.FindAbility<CharacterMovement>();
			_healthRegenerationAbility = _character.FindAbility<HealthRegenerationAbility>();
			_characterExperienceAbility = _character.FindAbility<CharacterExperienceAbility>();
			foreach (var talent in _initialTalents)
			{
				AddTalent(talent);
			}
		}

		public List<CharacterSkill> GetWeaponSkills()
		{
			return _currentSkills.Where(s => s.Skill is WeaponTalentBase).ToList();
		}
		public List<CharacterSkill> GetUtilitySkills()
		{
			return _currentSkills.Where(s => s.Skill is not WeaponTalentBase).ToList();
		}

		public void UpgradeTalent(TalentBase talent)
		{
			var existedTalent = _currentSkills.FirstOrDefault(s => s.Skill.AbilityName == talent.AbilityName);
			if (existedTalent != null)
			{
				UpdateTalentLevel(existedTalent);
			}
			else
			{
				AddTalent(talent);
			}
			
			EventHandler.ExecuteEvent(InGameEvents.CharacterSkillsChanged);
		}

		private void AddTalent(TalentBase talent)
		{
			var existedTalent = new CharacterSkill(talent, 1);

			var weaponTalent = existedTalent.Skill as WeaponTalentBase;
			if (weaponTalent != null)
			{
				_weaponHandler.AddWeapon(weaponTalent.Weapon);
				_currentSkills.Add(existedTalent);
				return;
			}

			var maxHealthTalent = existedTalent.Skill as MaxHealthTalent;
			if (maxHealthTalent != null)
			{
				_health.MaximumHealth =
					_character.InitialHealth + maxHealthTalent.GetMaxHealthAddition(existedTalent.Level);
				EventHandler.ExecuteEvent(InGameEvents.CharacterHealthChanged);
				_currentSkills.Add(existedTalent);
				return;
			}

			var heal = existedTalent.Skill as HealCharacterTalent;
			if (heal != null)
			{
				_health.CurrentHealth = _health.MaximumHealth;
				EventHandler.ExecuteEvent(InGameEvents.CharacterHealthChanged);
				return;
			}

			var movement = existedTalent.Skill as MovementSpeedTalent;
			if (movement != null)
			{
				_movementAbility.MovementSpeedMultiplier = movement.GetMovementSpeedRatio(existedTalent.Level);
				_currentSkills.Add(existedTalent);
				return;
			}
			
			var healthRegeneration = existedTalent.Skill as HealthRegenerationTalent;
			if (healthRegeneration != null)
			{
				_healthRegenerationAbility.SetRegeneration(healthRegeneration.GetHealthRegeneration(existedTalent.Level));
				_currentSkills.Add(existedTalent);
				return;
			}
			
			var additionalExperience = existedTalent.Skill as AdditionalExperienceTalent;
			if (additionalExperience != null)
			{
				_characterExperienceAbility.ExperienceRatio = additionalExperience.GetExperienceFactorPerLevel(existedTalent.Level);
				_currentSkills.Add(existedTalent);
				return;
			}
			
			var weaponCooldownReductionTalent = existedTalent.Skill as WeaponCooldownReductionTalent;
			if (weaponCooldownReductionTalent != null)
			{
				_weaponHandler.SetCooldownRatio(weaponCooldownReductionTalent.GetCooldownRatio(existedTalent.Level));
				_currentSkills.Add(existedTalent);
				return;
			}
		}

		private void UpdateTalentLevel(CharacterSkill existedTalent)
		{
			existedTalent.Level++;
			var weaponTalent = existedTalent.Skill as WeaponTalentBase;
			if (weaponTalent != null)
			{
				_weaponHandler.UpdateWeaponLevel(weaponTalent.Weapon.WeaponName, existedTalent.Level);
				return;
			}

			var maxHealthTalent = existedTalent.Skill as MaxHealthTalent;
			if (maxHealthTalent != null)
			{
				_health.MaximumHealth =
					_character.InitialHealth + maxHealthTalent.GetMaxHealthAddition(existedTalent.Level);
				EventHandler.ExecuteEvent(InGameEvents.CharacterHealthChanged);
				return;
			}

			var movement = existedTalent.Skill as MovementSpeedTalent;
			if (movement != null)
			{
				_movementAbility.MovementSpeedMultiplier = movement.GetMovementSpeedRatio(existedTalent.Level);
				return;
			}
			var healthRegeneration = existedTalent.Skill as HealthRegenerationTalent;
			if (healthRegeneration != null)
			{
				_healthRegenerationAbility.SetRegeneration(healthRegeneration.GetHealthRegeneration(existedTalent.Level));
				return;
			}
			
			var additionalExperience = existedTalent.Skill as AdditionalExperienceTalent;
			if (additionalExperience != null)
			{
				_characterExperienceAbility.ExperienceRatio = additionalExperience.GetExperienceFactorPerLevel(existedTalent.Level);
				return;
			}
			
			var weaponCooldownReductionTalent = existedTalent.Skill as WeaponCooldownReductionTalent;
			if (weaponCooldownReductionTalent != null)
			{
				_weaponHandler.SetCooldownRatio(weaponCooldownReductionTalent.GetCooldownRatio(existedTalent.Level));
				return;
			}
		}
	}
}