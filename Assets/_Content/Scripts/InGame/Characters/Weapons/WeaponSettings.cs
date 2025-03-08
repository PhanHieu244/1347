using UnityEngine;

namespace _Content.InGame.Characters.Weapons
{
	
	public abstract class WeaponSettings: ScriptableObject
	{
		public abstract int GetMaxLevel();
		public abstract string GetLevelDescription(int level);
	}
}