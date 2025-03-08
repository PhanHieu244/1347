using Base;
using UnityEngine;

namespace _Content.InGame.Managers
{
	public class LayerMaskManager: Singleton<LayerMaskManager>
	{
		[SerializeField] private LayerMask _enemyCheckMask;
		[SerializeField] private LayerMask _groundMask;

		public LayerMask GroundMask => _groundMask;
		public LayerMask EnemyCheckMask => _enemyCheckMask;
	}
}