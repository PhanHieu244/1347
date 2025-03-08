using System;
using DG.Tweening;
using UnityEngine;

namespace _Content.InGame.TweenAnimation
{
	[CreateAssetMenu(menuName = "Animations/Drop")]
	public class DropAnimation: ScriptableObject
	{
		[SerializeField] private AnimationCurve _xzCurve;
		[SerializeField] private AnimationCurve _yCurve;
		[SerializeField] private float _time;

		public void DoAnimation(Transform tr, Vector3 destination, float yMax, Action onComplete)
		{
			tr.DOMoveX(destination.x, _time)
				.SetEase(_xzCurve);
			tr.DOMoveZ(destination.z, _time)
				.SetEase(_xzCurve);
			tr.DOMoveY(yMax, _time)
				.SetEase(_yCurve)
				.OnComplete(() =>
				{
					if (onComplete != null)
						onComplete();
				});
		}
	}
}