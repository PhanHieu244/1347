using NaughtyAttributes;
using UnityEngine;

namespace _Content.InGame.Characters.Weapons
{
	public abstract class WeaponBase : MonoBehaviour
	{
		[SerializeField] private string _weaponName;
		[SerializeField] [ReadOnly] protected int _weaponLevel;
		public int WeaponLevel => _weaponLevel;

		public abstract int MaxLevel { get; } 
		public string WeaponName => _weaponName;
		private void Awake()
		{
			OnAwake();
		}

		protected virtual void OnAwake()
		{
		}

		protected abstract float GetCooldown();
		
		[Button()]
		public void IncreaseWeaponLevel()
		{
			UpdateWeaponLevel(_weaponLevel + 1);
		}

		public abstract string GetDescriptionForLevel(int level);
		public abstract void UpdateWeaponLevel(int weaponLevel);
		public abstract void UpdateWeapon(float deltaTime);
	}
}