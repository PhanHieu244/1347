using _Content.InGame.Managers;

namespace _Content.InGame.GameDrop
{
	public class HealDrop: ExpGem
	{
		protected override void OnCollected()
		{
			_onCollectFeedback?.PlayFeedbacks();
			EnemyManager.Instance.OnHealingDropCollected();
			Destroy(gameObject);
		}
	}
}