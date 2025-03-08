using UnityEngine;

namespace _Content.InGame.Characters.Talents
{
	[CreateAssetMenu(menuName = "Abilities/Boomerang")]
	public class BoomerangTalent: WeaponTalentBase
	{
		public override int LevelsCount => Weapon?.MaxLevel ?? 1;

	}
}