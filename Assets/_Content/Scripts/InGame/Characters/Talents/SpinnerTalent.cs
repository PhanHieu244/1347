using UnityEngine;

namespace _Content.InGame.Characters.Talents
{
	[CreateAssetMenu(menuName = "Abilities/Spinner")]
	public class SpinnerTalent: WeaponTalentBase
	{
		public override int LevelsCount => Weapon?.MaxLevel ?? 1;
	}
}