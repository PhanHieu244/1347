using _Content.InGame.Managers;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace _Content.CustomFeedbacks
{
	[FeedbackPath("Particles/Enemy Death")]
	public class PlayEnemyDeathParticle: MMF_Feedback
	{
		[MMFInspectorGroup("Settings", true, 28)]
		[SerializeField] private string _enemyId;
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			var ps = EnemyManager.Instance.GetDeathParticle(_enemyId);
			if (ps != null)
			{
				ps.transform.position = Owner.transform.position;;
				ps.Play();
			}
		}
	}
}