using UnityEngine;

namespace _Content.InGame.Enemies
{
	public class RangeEnemyAnimatorProxy: MonoBehaviour
	{
		private RangeEnemyController _rangeEnemy;

		private void Awake()
		{
			_rangeEnemy = GetComponentInParent<RangeEnemyController>();
		}
		
		public void OnAttack()
		{
			_rangeEnemy.ShootProjectile();
		}
	}
}