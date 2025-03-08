using _Content.Data;
using _Content.Events;
using _Content.InGame.Managers;
using Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EventHandler = Opsive.Shared.Events.EventHandler;

namespace _Content.InGame.UI.Main
{
	public class WorldProgress : UIViewWrapper
	{
		[SerializeField] private TextMeshProUGUI _chapterCaptionText;
		[SerializeField] private TextMeshProUGUI _levelNumber;
		[SerializeField] private TextMeshProUGUI _progressText;
		[SerializeField] private Slider _progressSlider;
		[SerializeField] private RectTransform _progressSliderRectTransform;
		[SerializeField] private RectTransform _rewardTransform;
		[SerializeField] private Image _nextChapterIcon;
		private int _lastProgress;

		private void OnEnable()
		{
			EventHandler.RegisterEvent(InGameEvents.LevelProgressionChanged, OnLevelProgressionChanged);
		}

		private void OnDisable()
		{
			EventHandler.RegisterEvent(InGameEvents.LevelProgressionChanged, OnLevelProgressionChanged);
		}

		private void OnLevelProgressionChanged()
		{
			UpdateProgressSlider(false);
		}

		public override void ShowView(bool force = false)
		{
			base.ShowView(force);
			UpdateInformation();
		}

		private void UpdateInformation()
		{
			var neededProgressToNewLevel = GameManager.Instance.GetProgressToUnlockNewLevel();
			_levelNumber.text = (GameManager.Instance.GetCurrentSceneIndex() + 1).ToString();
			_chapterCaptionText.text = GameManager.Instance.GetChapterCaption();
			_nextChapterIcon.sprite = GameManager.Instance.GetChapterIcon();
			var unlocks = GameManager.Instance.GetLevelUnlocks();
			if (unlocks.Count > 0 && !PlayerData.Instance.IsTalentUnlocked(unlocks[0].AbilityName))
			{
				_rewardTransform.gameObject.SetActive(true);
				var progressToUnlock = unlocks[0].ProgressToUnlock;
				var t = progressToUnlock / (float)neededProgressToNewLevel;
				var width = _progressSliderRectTransform.rect.width;
				var posX = Mathf.Lerp(-width / 2f, width / 2f, t);
				var pos = _rewardTransform.anchoredPosition;
				pos.x = posX;
				_rewardTransform.anchoredPosition = pos;
			}
			else
			{
				_rewardTransform.gameObject.SetActive(false);
			}
			UpdateProgressSlider(true);
		}

		private void UpdateProgressSlider(bool force)
		{
			var neededProgressToNewLevel = GameManager.Instance.GetProgressToUnlockNewLevel();
			var currentProgress = GameManager.Instance.GetCurrentLevelProgress();
			if (force)
			{
				_lastProgress = currentProgress;
				_progressText.text = $"{_lastProgress}/{neededProgressToNewLevel}";
				_progressSlider.value = _lastProgress / (float)neededProgressToNewLevel;
			}
			else
			{
				DOVirtual.Int(_lastProgress, currentProgress, 0.5f, (x) =>
				{
					_lastProgress = x;
					_progressText.text = $"{_lastProgress}/{neededProgressToNewLevel}";
					_progressSlider.value = _lastProgress / (float)neededProgressToNewLevel;
				});
			}
		}
	}
}