using System.Linq;
using _Content.Data;
using _Content.Events;
using _Content.InGame.Managers;
using _Content.InGame.UI.Misc;
using Common.UI;
using DG.Tweening;
using Doozy.Engine.UI;
using Opsive.Shared.Events;
using TMPro;
using UnityEngine;

namespace _Content.InGame.UI.Main
{
	public class RewardUi : UIViewWrapper
	{
		[SerializeField] private TextMeshProUGUI _enemyCountText;
		[SerializeField] private TextMeshProUGUI _rewardText;
		[SerializeField] private TextMeshProUGUI _characterLevelText;

		[SerializeField] private UIButton _rewardButton;

		public override void ShowView(bool force = false)
		{
			base.ShowView(force);
			UpdateInfo();
			_rewardButton.EnableButton();
		}

		private void UpdateInfo()
		{
			_characterLevelText.text = PlayerData.Instance.CurrentCharacterLevel.ToString();
			_rewardText.text = GameplayManager.Instance.GetCurrentLevelReward().ToString();
			_enemyCountText.text = EnemyManager.Instance.GetDefeatedEnemyCount().ToString();
		}

		public void OnClaimReward()
		{
			_rewardButton.DisableButton();
			var levelIndex = GameManager.Instance.GetCurrentSceneIndex();
			var currentProgression = PlayerData.Instance.GetLevelProgression(levelIndex);
			var reward = GameplayManager.Instance.GetCurrentLevelReward();
			var neededProgression = GameManager.Instance.GetProgressToUnlockNewLevel();
			reward = Mathf.Min(reward, neededProgression - currentProgression.Progression);

			PlayerData.Instance.AddLevelProgression(levelIndex, reward);
			var needToUnlockNextLevel =
				PlayerData.Instance.GetLevelProgression(levelIndex).Progression == neededProgression;

			if (needToUnlockNextLevel)
			{
				UnlockNextLevel();
				return;
			}

			var unlocks = GameManager.Instance.GetLevelUnlocks();
			var neededUnlock = unlocks.FirstOrDefault(u =>
				u.ProgressToUnlock < PlayerData.Instance.GetLevelProgression(levelIndex)
					.Progression
				&& !CharacterSkillsManager.Instance.IsTalentUnlocked(u.AbilityName));

			var dest = UIManager.Instance.CoinParticleDestination;
			UiParticleSystem.Instance.StartCoinParticles(_rewardText.rectTransform.position, dest.position);

			if (neededUnlock != null)
			{
				PlayerData.Instance.TalentsUnlocked.Add(neededUnlock.AbilityName);
				DOVirtual.DelayedCall(1.5f, () => { EventHandler.ExecuteEvent(InGameEvents.LevelProgressionChanged); });
				var skill = CharacterSkillsManager.Instance.GetSkill(neededUnlock.AbilityName);
				DOVirtual.DelayedCall(2f, () =>
				{
					HideView();
					UIManager.Instance.UnlockSkillUi.ShowView(skill);
				});
			}
			else
			{
				DOVirtual.DelayedCall(1.5f, () => { EventHandler.ExecuteEvent(InGameEvents.LevelProgressionChanged); });
				DOVirtual.DelayedCall(3f, () => { GameManager.Instance.HideRewardUi(); });
			}
		}

		private void UnlockNextLevel()
		{
			EventHandler.ExecuteEvent(InGameEvents.LevelProgressionChanged);
			GameManager.Instance.ChangeNextLevel();

			var dest = UIManager.Instance.CoinParticleDestination;
			UiParticleSystem.Instance.StartCoinParticles(_rewardText.rectTransform.position, dest.position);
			DOVirtual.DelayedCall(2f, () => { GameManager.Instance.HideRewardUi(); });
		}
	}
}