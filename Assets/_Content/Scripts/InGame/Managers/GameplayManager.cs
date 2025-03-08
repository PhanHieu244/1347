using System;
using System.Collections.Generic;
using System.Linq;
using _Content.Data;
using _Content.Events;
using _Content.InGame.Characters;
using _Content.InGame.Characters.Abilities;
using _Content.InGame.GameDrop;
using _Content.InGame.Levels;
using Base;
using MoreMountains.Feedbacks;
using NaughtyAttributes;
using UnityEngine;
using EventHandler = Opsive.Shared.Events.EventHandler;

namespace _Content.InGame.Managers
{
	public class GameplayManager : Singleton<GameplayManager>
	{
		[SerializeField] [Tag] private string _spawnPointTag;
		[SerializeField] private float _arenaSize = 50f;
		[SerializeField] private Transform _arenaCenter;
		[SerializeField] private List<int> _expToNextLevels;
		[SerializeField] private Character _characterPrefab;
		[SerializeField] private MMF_Player _onLevelCompleteFeedback;

		public bool CharacterRevived => _characterRevived;
		public float ArenaSize => _arenaSize;

		public Vector3 ArenaCenter
		{
			get
			{
				if (_arenaCenter != null)
					return _arenaCenter.position;
				return Vector3.zero;
			}
		}

		private bool _gameStarted = false;
		private float _gameTimer = 0f;
		private Character _character;
		private CharacterExperienceAbility _expAbility;
		private bool _characterRevived;
		private int _spawnPointIndex;
		public bool GameStarted => _gameStarted;

		public float GameTimer => _gameTimer;
		public Character Character => _character;

		protected override void OnAwake()
		{
			base.OnAwake();
			_onLevelCompleteFeedback?.Initialization(gameObject);
		}

		public void InitializeLevel(LevelInfo prevLevelInfo, Action onLevelInitialized)
		{
			var spawnPoints = GameObject.FindGameObjectsWithTag(_spawnPointTag);
			var charStartPosition = Vector3.zero;
			if (spawnPoints != null && spawnPoints.Length > 0)
			{
				if (_spawnPointIndex >= spawnPoints.Length)
					_spawnPointIndex = 0;

				charStartPosition = spawnPoints[_spawnPointIndex].transform.position;
				_spawnPointIndex++;
				/*if (spawnPoints.Length == 1)
					charStartPosition = spawnPoints[0].transform.position;
				else
					charStartPosition = spawnPoints.GetRandomElement().transform.position;*/
			}

			_character = Instantiate(_characterPrefab, charStartPosition, Quaternion.identity);

			if (_character != null)
				_expAbility = _character.FindAbility<CharacterExperienceAbility>();

			var arenaCenterGo = GameObject.FindGameObjectWithTag("ArenaCenter");
			if (arenaCenterGo != null)
			{
				_arenaCenter = arenaCenterGo.transform;
			}

			_gameStarted = false;
			PlayerData.Instance.CurrentCharacterLevel = 1;
			PlayerData.Instance.CurrentCharacterExperience = 0;
			EventHandler.ExecuteEvent(InGameEvents.CharacterExperienceChanged,
				PlayerData.Instance.CurrentCharacterExperience);
			EventHandler.ExecuteEvent(InGameEvents.CharacterLevelChanged,
				PlayerData.Instance.CurrentCharacterLevel);

			EnemyManager.Instance.Initialize(prevLevelInfo);
			onLevelInitialized();
		}

		public void Deinitialize()
		{
			_gameStarted = false;
			var drop = FindObjectsOfType<ExpGem>();
			foreach (var expGem in drop)
			{
				Destroy(expGem.gameObject);
			}

			Destroy(_character.gameObject);
			EnemyManager.Instance.Deinitialize();
		}

		private void Update()
		{
			if (GameStarted)
			{
				_gameTimer += Time.deltaTime * GameManager.Instance.TimeScale;
			}
		}

		public void AddExperience(int exp)
		{
			exp = _expAbility.ProcessExperience(exp);
			var expToNewLevel = GetExpToNextLevel();
			var newExp = PlayerData.Instance.CurrentCharacterExperience + exp;
			if (newExp >= expToNewLevel)
			{
				var newLevels = newExp / expToNewLevel;
				newExp = newExp % expToNewLevel;
				PlayerData.Instance.SetExperience(newExp);
				PlayerData.Instance.SetCharacterLevel(PlayerData.Instance.CurrentCharacterLevel + newLevels);
				EventHandler.ExecuteEvent(InGameEvents.CharacterExperienceChanged,
					PlayerData.Instance.CurrentCharacterExperience);
				EventHandler.ExecuteEvent(InGameEvents.CharacterLevelChanged,
					PlayerData.Instance.CurrentCharacterLevel);
				if (CharacterSkillsManager.Instance.HasSkillToUpgrade())
					GameManager.Instance.ShowNewLevelAbilitiesUi(newLevels);
			}
			else
			{
				PlayerData.Instance.SetExperience(newExp);
				EventHandler.ExecuteEvent(InGameEvents.CharacterExperienceChanged,
					PlayerData.Instance.CurrentCharacterExperience);
			}
		}

		public int GetExpToNextLevel()
		{
			var index = PlayerData.Instance.CurrentCharacterLevel - 1;
			if (index >= _expToNextLevels.Count)
				index = _expToNextLevels.Count - 1;

			return _expToNextLevels.ElementAt(index);
		}

		public void OnSauceCollect()
		{
			PlayerData.Instance.AddSauce(1);
			EventHandler.ExecuteEvent(InGameEvents.SauceAmountChanged);
		}

		public void StartGame()
		{
			GameManager.Instance.StartMap();
			_gameTimer = 0f;
			_characterRevived = false;
			_gameStarted = true;
			EventHandler.ExecuteEvent(InGameEvents.GameStart);
		}

		public int GetCurrentLevelReward()
		{
			return GameManager.Instance.GetProgressPerWin();
		}

		public void CompleteLevel()
		{
			_gameStarted = false;
			_onLevelCompleteFeedback?.PlayFeedbacks();
			
			GameManager.Instance.ShowRewardUi();
		}

		public void ReviveCharacter()
		{
			_characterRevived = true;
			Character?.Revive();
		}

		public void WinLevelCheat()
		{
			if (!GameStarted)
				return;
			
			CompleteLevel();
		}
	}
}