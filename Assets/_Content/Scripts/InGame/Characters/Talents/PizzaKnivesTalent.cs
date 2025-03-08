using UnityEngine;

namespace _Content.InGame.Characters.Talents
{
	[CreateAssetMenu(menuName = "Abilities/Pizza Knives")]
	public class PizzaKnivesTalent: WeaponTalentBase
	{
		public override int LevelsCount => Weapon?.MaxLevel ?? 1;
	}
}