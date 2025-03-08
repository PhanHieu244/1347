using _Content.InGame.Managers;

namespace _Content.InGame.GameDrop
{
	public class CollectAllExpDrop : ExpGem
	{
		protected override void OnCollected()
		{
			_onCollectFeedback?.PlayFeedbacks();
			var allGems = FindObjectsOfType<ExpGem>();
			foreach (var allGem in allGems)
			{
				allGem.Collect(GameplayManager.Instance.Character.CollectionPoint, true);
			}

			Destroy(gameObject);
		}
	}
}