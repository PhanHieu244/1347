using UnityEngine;

namespace _Content.InGame.Characters.Talents
{
	[CreateAssetMenu(menuName = "Abilities/Orbital Knives")]
	public class OrbitalKnives : WeaponTalentBase
	{
		public override int LevelsCount => Weapon?.MaxLevel ?? 1;
	}
}