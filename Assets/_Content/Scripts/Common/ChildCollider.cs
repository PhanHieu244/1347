using System;
using UnityEngine;

namespace Common
{
	public class ChildCollider : MonoBehaviour
	{
		public event Action<Collision> CollisionEnter;
		public event Action<Collision> CollisionStay;
		public event Action<Collision> CollisionExit;

		private void OnCollisionEnter(Collision collision)
		{
			CollisionEnter?.Invoke(collision);
		}

		private void OnCollisionStay(Collision collision)
		{
			CollisionStay?.Invoke(collision);
		}

		private void OnCollisionExit(Collision collision)
		{
			CollisionExit?.Invoke(collision);
		}
	}
}