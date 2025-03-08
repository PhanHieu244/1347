using System;
using DG.Tweening;
using UnityEngine;

namespace _Content.InGame.TweenAnimation
{
	[CreateAssetMenu(menuName = "Animations/CollectWithY")]
	public class CollectAnimationWithY : CollectAnimation
	{
		public override void DoAnimation(Transform tr, Transform parent, Action onComplete, bool slow = false)
		{
			var timeY = slow ? _timeY * 2f : _timeY;
			var timeXZ = slow ? _timeXZ * 2f : _timeXZ;
			//tr.SetParent(parent);
			tr.DOMoveY(parent.position.y, timeY)
				.SetEase(_yCurve);
			/*tr.DOLocalMoveX(0f, timeXZ)
				.SetEase(_xzCurve);
			tr.DOLocalMoveZ(0f, timeXZ)
				.SetEase(_xzCurve)
				.OnComplete(() =>
				{
					if (onComplete != null)
						onComplete();
				});*/
		}
	}
}