using System;
using DG.Tweening;
using UnityEngine;

namespace _Content.InGame.TweenAnimation
{
	[CreateAssetMenu(menuName = "Animations/Collect")]
	public class CollectAnimation : ScriptableObject
	{
		[SerializeField] public AnimationCurve _xzCurve;
		[SerializeField] public AnimationCurve _yCurve;
		[SerializeField] public float _timeXZ;
		[SerializeField] public float _timeY;

		public virtual void DoAnimation(Transform tr, Transform parent, Action onComplete, bool slow = false)
		{
			var timeY = slow ? _timeY * 2f : _timeY;
			var timeXZ = slow ? _timeXZ * 2f : _timeXZ;
			tr.DOMoveY(parent.position.y, timeY)
				.SetEase(_yCurve)
				.OnComplete(() =>
				{
					tr.SetParent(parent);
					tr.DOLocalMoveX(0f, timeXZ)
						.SetEase(_xzCurve);
					tr.DOLocalMoveZ(0f, timeXZ)
						.SetEase(_xzCurve)
						.OnComplete(() =>
						{
							if (onComplete != null)
								onComplete();
						});
				});
		}
	}
}