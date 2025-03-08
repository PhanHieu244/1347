using _Content.InGame.Managers;

namespace _Content.InGame.GameDrop
{
	public class SauceDrop: ExpGem
	{
		protected override void OnCollected()
		{
			_onCollectFeedback?.PlayFeedbacks();
			GameplayManager.Instance.OnSauceCollect();
			Destroy(gameObject);
		}
	}
}