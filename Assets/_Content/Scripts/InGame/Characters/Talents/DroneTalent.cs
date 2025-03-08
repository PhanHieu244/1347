using UnityEngine;

namespace _Content.InGame.Characters.Talents
{
	[CreateAssetMenu(menuName = "Abilities/Drone")]
	public class DroneTalent: WeaponTalentBase
	{
		public override int LevelsCount => Weapon?.MaxLevel ?? 1;
	}
}