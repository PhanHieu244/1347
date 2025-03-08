using UnityEngine;

namespace _Content.InGame.Characters.Talents
{
	[CreateAssetMenu(menuName = "Abilities/Slicer")]
	public class SlicerTalent: WeaponTalentBase
	{
		public override int LevelsCount => Weapon?.MaxLevel ?? 1;
	}
}