using _Content.InGame.Enemies;
using _Content.InGame.Managers;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace _Content.CustomFeedbacks
{
	[FeedbackPath("Particles/Enemy Hit")]
	public class PlayEnemyHitParticle: MMF_Feedback
	{
		[MMFInspectorGroup("Settings", true, 28)]
		[SerializeField] private string _enemyId;

		public override void Initialization(MMF_Player owner)
		{
			base.Initialization(owner);
			var controller = owner.GetComponentInParent<SimpleEnemyController>();
			_enemyId = controller.EnemyId;
		}

		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			var ps = EnemyManager.Instance.GetHitParticles(_enemyId);
			if (ps != null)
			{
				ps.transform.position = Owner.transform.position;
				ps.Play();
			}
		}
	}
}