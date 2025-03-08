using UnityEngine;

namespace _Content.InGame.Enemies
{
	public class ChilliPepperAnimatorProxy: MonoBehaviour
	{
		private ChilliPepperEnemyController _rangeEnemy;

		private void Awake()
		{
			_rangeEnemy = GetComponentInParent<ChilliPepperEnemyController>();
		}
		
		public void OnAttack()
		{
			_rangeEnemy.ShootProjectiles();
		}
	}
}