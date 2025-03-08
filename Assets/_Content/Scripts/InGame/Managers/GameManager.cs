using System.Collections.Generic;
using System.Linq;
using _Content.Appmetricas;
using _Content.Data;
using _Content.Events;
using _Content.InGame.Levels;
using Base;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Opsive.Shared.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Content.InGame.Managers
{
	public class GameManager : Singleton<GameManager>
	{
		[SerializeField] private LevelsList _levelsList;
		[SerializeField] private MMF_Player _musicFeedback;
		[SerializeField] private MMF_Player _lvlUpFeedback;
		[SerializeField] private MMF_Player _defeatFeedback;
		[SerializeField] private MMF_Player _winFeedback;
		[SerializeField] private MMF_Player _unlockSkillFeedback;
		[SerializeField] private float _stopGameDuration = 0.15f;
		[SerializeField] private AnimationCurve _stopGameCurve;
		[SerializeField] private float _onPlayerDamageTimescaleTime = 0.3f;
		[SerializeField] private AnimationCurve _onPlayerDamageCurve;
		private Tweener _timeScaleTween;
		private bool _levelInitialized;
		private LevelInfo _prevLevelInfo;
		private bool _unloadOperationCompleted;
		private bool _loadOperationCompleted;
		private int _newLevelTimes;
		public float TimeScale { get; private set; } = 1f;

		public int NewLevelTimes => _newLevelTimes;

		public MMF_Player UnlockSkillFeedback => _unlockSkillFeedback;

		public void OnDamageTimeScale()
		{
			var existingTimeScale = TimeScale;
			if (_timeScaleTween != null && _timeScaleTween.IsActive())
				_timeScaleTween.Kill();
			TimeScale = 0f;
			_timeScaleTween = DOVirtual.Float(existingTimeScale, 1f, _onPlayerDamageTimescaleTime, (x) =>
				{
					TimeScale = x;
					EventHandler.ExecuteEvent(InGameEvents.OnTimeScaleChanged, TimeScale);
				})
				.SetEase(_onPlayerDamageCurve);
		}

		private void StopGame()
		{
			var existingTimeScale = TimeScale;
			if (_timeScaleTween != null && _timeScaleTween.IsActive())
				_timeScaleTween.Kill();

			_timeScaleTween = DOVirtual.Float(existingTimeScale, 0f, _stopGameDuration, (x) =>
				{
					TimeScale = x;
					EventHandler.ExecuteEvent(InGameEvents.OnTimeScaleChanged, TimeScale);
				})
				.SetEase(_stopGameCurve);
		}

		private void ResumeGame()
		{
			if (_timeScaleTween != null && _timeScaleTween.IsActive())
				_timeScaleTween.Kill();

			var existingTimeScale = TimeScale;
			_timeScaleTween = DOVirtual.Float(existingTimeScale, 1f, 0.1f, (x) =>
			{
				TimeScale = x;
				EventHandler.ExecuteEvent(InGameEvents.OnTimeScaleChanged, TimeScale);
			});
			EventHandler.ExecuteEvent(InGameEvents.OnTimeScaleChanged, TimeScale);
		}

		public void ShowNewLevelAbilitiesUi(int newLevels)
		{
			_newLevelTimes += newLevels;
			if (!UIManager.Instance.NewLevelAbilitiesUi.IsShown)
			{
				StopGame();
				UIManager.Instance.Joystick.HideView();
				UIManager.Instance.NewLevelAbilitiesUi.ShowView();
				_lvlUpFeedback?.PlayFeedbacks();
				_newLevelTimes--;
			}
		}

		public void HideNewLevelAbilitiesUi()
		{
			UIManager.Instance.NewLevelAbilitiesUi.HideView();
			if (_newLevelTimes > 0)
			{
				UIManager.Instance.NewLevelAbilitiesUi.ShowView();
				_lvlUpFeedback?.PlayFeedbacks();
				_newLevelTimes--;
			}
			else
			{
				UIManager.Instance.Joystick.ShowView();
				UIManager.Instance.MovementTutorialUi.ShowView();
			}
		}


		public void ShowDefeatUi()
		{
			StopGame();
			_defeatFeedback?.PlayFeedbacks();
			UIManager.Instance.Joystick.HideView();
			UIManager.Instance.DefeatUi.ShowView();
		}

		public void RevivePlayer()
		{
			ResumeGame();
			UIManager.Instance.Joystick.ShowView();
			UIManager.Instance.DefeatUi.HideView();
			GameplayManager.Instance.ReviveCharacter();
			EnemyManager.Instance.OnPlayerDamage(GameplayManager.Instance.Character.transform.position, 10, 10);
		}

		public void RestartAfterDefeat()
		{
			PlayerData.Instance.LevelNumber++;
			PlayerData.Instance.Save();
			AppMetricaEvents.SendLevelEndEvent(PlayerData.Instance.LevelNumber, PlayerData.Instance.LevelCount,
				_prevLevelInfo?.LevelCaption, 1, LevelResult.Lose, Mathf.RoundToInt(GameplayManager.Instance.GameTimer),
				EnemyManager.Instance.GetLevelProgressForAnalytics());
			ResumeGame();
			UIManager.Instance.Joystick.HideView();
			UIManager.Instance.DefeatUi.HideView();
			UIManager.Instance.RewardUi.HideView();
			UIManager.Instance.UnlockSkillUi.HideView();
			UIManager.Instance.LoadingUi.ShowView();
			LoadCurrentLevelScene();
		}

		public void ReturnFromMovementTutorial()
		{
			ResumeGame();
			UIManager.Instance.MovementTutorialUi.HideView();
		}

		public void ShowRewardUi()
		{
			PlayerData.Instance.LevelNumber++;
			PlayerData.Instance.Save();
			AppMetricaEvents.SendLevelEndEvent(PlayerData.Instance.LevelNumber, PlayerData.Instance.LevelCount,
				_prevLevelInfo?.LevelCaption, 1, LevelResult.Lose, Mathf.RoundToInt(GameplayManager.Instance.GameTimer),
				EnemyManager.Instance.GetLevelProgressForAnalytics());
			
			StopGame();
			_winFeedback?.PlayFeedbacks();
			UIManager.Instance.RewardUi.ShowView();
			UIManager.Instance.WorldProgress.ShowView();
			UIManager.Instance.SettingsButton.HideView();
			UIManager.Instance.CharacterProgressUi.HideView();
			UIManager.Instance.CurrentSkillsUi.HideView();
			UIManager.Instance.LevelProgressUi.HideView();
			UIManager.Instance.SettingsUi.HideView();
			UIManager.Instance.Joystick.HideView();
		}

		public void HideRewardUi()
		{
			ResumeGame();
			UIManager.Instance.RewardUi.HideView();
			UIManager.Instance.UnlockSkillUi.HideView();
			UIManager.Instance.LoadingUi.ShowView();
			GameManager.Instance.LoadCurrentLevelScene();
		}

		public void ShowSettings()
		{
			StopGame();
			UIManager.Instance.Joystick.HideView();
			UIManager.Instance.SettingsButton.HideView();
			UIManager.Instance.MainMenu.HideView();
			UIManager.Instance.SettingsUi.ShowView();
		}

		public void HideSettings()
		{
			ResumeGame();
			UIManager.Instance.Joystick.ShowView();
			UIManager.Instance.SettingsButton.ShowView();
			UIManager.Instance.SettingsUi.HideView();
			if (!GameplayManager.Instance.GameStarted)
				UIManager.Instance.MainMenu.ShowView();
		}

		public void StartGame()
		{
			LoadCurrentLevelScene();
			UIManager.Instance.CheatsUi.ShowView();
			_musicFeedback?.Initialization(gameObject);
			_musicFeedback?.PlayFeedbacks();
			_lvlUpFeedback?.Initialization(gameObject);
			_defeatFeedback?.Initialization(gameObject);
			_winFeedback?.Initialization(gameObject);
			_unlockSkillFeedback?.Initialization(gameObject);
		}

		public void StartMap()
		{
			UIManager.Instance.MainMenu.HideView();
			UIManager.Instance.WorldProgress.HideView();
			UIManager.Instance.SettingsButton.ShowView();
			UIManager.Instance.CharacterProgressUi.ShowView();
			UIManager.Instance.CurrentSkillsUi.ShowView();
			UIManager.Instance.LevelProgressUi.ShowView();
			UIManager.Instance.SettingsUi.HideView();
			UIManager.Instance.Joystick.ShowView();

			CameraManager.Instance.ShowGameplayCamera();
			PlayerData.Instance.LevelCount++;
			PlayerData.Instance.Save();
			AppMetricaEvents.SendLevelStartEvent(PlayerData.Instance.LevelNumber, PlayerData.Instance.LevelCount,
				_prevLevelInfo?.LevelCaption, 1);
		}

		public void LoadCurrentLevelScene()
		{
			var levelIndex = GetCurrentSceneIndex();
			LoadLevelScene(levelIndex);
		}

		public int GetCurrentSceneIndex()
		{
			if (PlayerData.Instance.CurrentLevelIndex >= _levelsList.Levels.Count)
				return _levelsList.Levels.Count - 1;

			return PlayerData.Instance.CurrentLevelIndex;
		}

		private void LoadLevelScene(int levelIndex)
		{
			var levelInfo = _levelsList.Levels.ElementAt(levelIndex);

			if (levelInfo == null)
			{
				levelInfo = _levelsList.Levels.FirstOrDefault();
			}

			if (_prevLevelInfo != null)
			{
				DeinitializeLevel();
			}

			_unloadOperationCompleted = false;
			_loadOperationCompleted = false;
			if (_prevLevelInfo != null)
			{
				var sceneName = _prevLevelInfo.SceneName;
				_prevLevelInfo = levelInfo;
				var unloadAsyncOperation = SceneManager.UnloadSceneAsync(sceneName);
				unloadAsyncOperation.completed += OnUnloadOperationCompleted;
			}
			else
			{
				_prevLevelInfo = levelInfo;
				_unloadOperationCompleted = true;
			}

			var loadAsyncOperation = SceneManager.LoadSceneAsync(levelInfo.SceneName, LoadSceneMode.Additive);
			loadAsyncOperation.allowSceneActivation = true;
			loadAsyncOperation.completed += OnLoadOperationComplete;
		}

		private void DeinitializeLevel()
		{
			EventHandler.ExecuteEvent(InGameEvents.DeinitializeLevel);
			UIManager.Instance.SettingsButton.HideView();
			UIManager.Instance.CharacterProgressUi.HideView();
			UIManager.Instance.CurrentSkillsUi.HideView();
			UIManager.Instance.LevelProgressUi.HideView();
			UIManager.Instance.SettingsUi.HideView();
			UIManager.Instance.Joystick.HideView();
			GameplayManager.Instance.Deinitialize();
			_levelInitialized = false;
			//FreezingManager.Instance.Deinitialize();
		}

		private void OnLoadOperationComplete(AsyncOperation obj)
		{
			_loadOperationCompleted = true;
			if (_unloadOperationCompleted)
				SetCurrentLevelSceneActive();
		}

		private void OnUnloadOperationCompleted(AsyncOperation obj)
		{
			_unloadOperationCompleted = true;
			if (_loadOperationCompleted)
				SetCurrentLevelSceneActive();
		}

		private void SetCurrentLevelSceneActive()
		{
			GameplayManager.Instance.InitializeLevel(_prevLevelInfo, OnLevelInitialized);
		}

		public void OnLevelInitialized()
		{
			UIManager.Instance.LoadingUi.HideView();
			UIManager.Instance.MainMenu.ShowView();
			UIManager.Instance.SettingsButton.ShowView();
			UIManager.Instance.WorldProgress.ShowView();

			PlayerData.Instance.LevelCount++;
			PlayerData.Instance.Save();

			CameraManager.Instance.ShowMainMenuCamera();
			/*AppmetricaEvents.SendLevelStartEvent(PlayerData.Instance.LevelNumber, _prevLevelInfo?.LevelName ?? "",
				PlayerData.Instance.LevelCount, PlayerData.Instance.LevelLoop);*/
			_levelInitialized = true;
		}

		public int GetCurrentLevelProgress()
		{
			return PlayerData.Instance.GetLevelProgression(GetCurrentSceneIndex()).Progression;
		}

		public string GetChapterCaption()
		{
			return _prevLevelInfo?.LevelCaption ?? string.Empty;
		}

		public Sprite GetChapterIcon()
		{
			return _prevLevelInfo?.LevelIcon;
		}

		public List<UnlockAbility> GetLevelUnlocks()
		{
			return _prevLevelInfo.UnlockAbilities;
		}

		public void ClearData()
		{
			PlayerData.NewSave();
			LoadCurrentLevelScene();
		}

		public void ChangeNextLevel()
		{
			PlayerData.Instance.CurrentLevelIndex++;
			if (PlayerData.Instance.CurrentLevelIndex >= _levelsList.Levels.Count)
			{
				PlayerData.Instance.CurrentLevelIndex = _levelsList.Levels.Count - 1;
			}
		}

		public int GetProgressToUnlockNewLevel()
		{
			return _prevLevelInfo?.ProgressMaxAmount ?? 100;
		}

		public int GetProgressPerWin()
		{
			return _prevLevelInfo?.ProgressPerWin ?? 35;
		}

		public void WinLevel()
		{
			GameplayManager.Instance.WinLevelCheat();
		}
	}
}