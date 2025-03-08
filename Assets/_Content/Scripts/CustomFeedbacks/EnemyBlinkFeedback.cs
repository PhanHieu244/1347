using _Content.InGame.Enemies;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace _Content.CustomFeedbacks
{
	[AddComponentMenu("")]
	[FeedbackPath("Renderer/EnemyBlinkFeedback")]
	public class EnemyBlinkFeedback: MMF_Feedback
	{
		public static bool FeedbackTypeAuthorized = true;
		[SerializeField] private EnemyRendererController _targetRendererController;
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1f)
		{
			if (!Active || !FeedbackTypeAuthorized || (_targetRendererController == null))
			{
				return;
			}
			
			_targetRendererController.Blink();
		}
	}
}