using _Content.InGame.Characters.Weapons;
using UnityEngine;

namespace _Content.InGame.Characters.Talents
{
	public abstract class WeaponTalentBase: TalentBase
	{
		[SerializeField] private WeaponBase _weapon;
		public WeaponBase Weapon => _weapon;
		
				
		public override string GetLevelDescription(int level)
		{
			return _weapon.GetDescriptionForLevel(level);
		}
	}
}