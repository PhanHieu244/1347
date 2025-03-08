using UnityEngine;

namespace Extensions
{
	public static class BoundsExtensions
	{
		public static Vector3 GetRandomPointInBounds(this Bounds bounds) {
			return new Vector3(
				Random.Range(bounds.min.x, bounds.max.x),
				Random.Range(bounds.min.y, bounds.max.y),
				Random.Range(bounds.min.z, bounds.max.z)
			);
		}
		
		public static Vector3 GetRandomPointInCollider(this Collider collider)
		{
			var point = new Vector3(
				Random.Range(collider.bounds.min.x, collider.bounds.max.x),
				Random.Range(collider.bounds.min.y, collider.bounds.max.y),
				Random.Range(collider.bounds.min.z, collider.bounds.max.z)
			);


			return collider.ClosestPoint(point);
		}
	}
}