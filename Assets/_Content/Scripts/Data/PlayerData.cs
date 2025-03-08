using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extensions;
using UnityEngine;

namespace _Content.Data
{
	public class PlayerData
	{
		[Serializable]
		public class LevelProgression
		{
			public int LevelNumber;
			public int Progression;
		}

		private static PlayerData _instance;

		public static PlayerData Instance
		{
			get
			{
				if (_instance == null)
				{
					Create();

					//if we create the PlayerData, mean it's the very first call, so we use that to init the database
					//this allow to always init the database at the earlier we can, i.e. the start screen if started normally on device
					//or the Loadout screen if testing in editor
				}

				return _instance;
			}
		}


		protected string SaveFile = "";
		static int _version = 10;
		public bool PrivacyAndTermsAccepted = false;
		public bool VibrationOn = true;
		public bool SoundOn = true;
		public bool MusicOn = true;
		public int LevelCount;
		public int LevelLoop;
		public int RateUsAttempts = 0;
		public DateTime RateUsLastTime = DateTime.UtcNow;
		public bool Rated = false;
		public int LevelNumber = 1;
		public int TutorialStage = 1;
		public DateTime LastSavedTime;
		public int InterstitialsShown = 0;
		public int CurrentSauce = 0;
		public int CurrentLevelIndex = 0;
		public List<LevelProgression> LevelProgressions = new List<LevelProgression>();
		public List<string> TalentsUnlocked = new List<string>();

		private float _timeToSave;
		private bool _savingInProgress;

		public int CurrentCharacterExperience = 0;
		public int CurrentCharacterLevel = 1;
		public DateTime StartTime;
		public DateTime LastTimeBeforeStart;
		public bool DisableAds = false;
		public float LastTimeBuiltTileForAd;
		public int LastGameSeconds;
		private float _lastSaveTime;

		public bool InterstitialWithoutOffer { get; set; } = true;
		public bool IAPBuyButtonWasPressed { get; set; }

		public int GetGameSeconds()
		{
			return LastGameSeconds + Mathf.FloorToInt(Time.unscaledTime);
		}

		public static void Create()
		{
			if (_instance == null)
			{
				_instance = new PlayerData();

				//if we create the PlayerData, mean it's the very first call, so we use that to init the database
				//this allow to always init the database at the earlier we can, i.e. the start screen if started normally on device
				//or the Loadout screen if testing in editor
			}

			_instance.SaveFile = Application.persistentDataPath + "/save.bin";
			if (File.Exists(_instance.SaveFile))
			{
				// If we have a save, we read it.

				_instance.Read();
			}
			else
			{
				// If not we create one with default data.
				NewSave();
			}
		}

		public long GetSaveFileLength()
		{
			if (File.Exists(SaveFile))
			{
				var fileInfo = new System.IO.FileInfo(SaveFile);
				return fileInfo.Length;
			}

			return 0;
		}

		public static void NewSave()
		{
			_instance.VibrationOn = true;
			_instance.SoundOn = true;
			_instance.MusicOn = true;
			_instance.PrivacyAndTermsAccepted = false;
			_instance.LevelCount = 0;
			_instance.LevelLoop = 0;
			_instance.Rated = false;
			_instance.RateUsAttempts = 0;
			_instance.RateUsLastTime = DateTime.UtcNow;
			_instance.LevelNumber = 1;
			_instance.TutorialStage = 1;
			_instance.LastSavedTime = DateTime.UtcNow;
			_instance.InterstitialsShown = 0;
			_instance.CurrentSauce = 0;
			_instance.CurrentLevelIndex = 0;
			_instance.LevelProgressions = new List<LevelProgression>();
			_instance.TalentsUnlocked = new List<string>();

			_instance.Save(true);
		}

		public PlayerData GetNewSave()
		{
			var playerData = new PlayerData();
			playerData.VibrationOn = true;
			playerData.SoundOn = true;
			playerData.MusicOn = true;
			playerData.PrivacyAndTermsAccepted = false;
			playerData.LevelCount = 0;
			playerData.LevelLoop = 0;
			playerData.Rated = false;
			playerData.RateUsAttempts = 0;
			playerData.RateUsLastTime = DateTime.UtcNow;
			playerData.LevelNumber = 1;
			playerData.TutorialStage = 1;
			playerData.LastSavedTime = DateTime.UtcNow;
			playerData.InterstitialsShown = 0;
			playerData.CurrentSauce = 0;
			playerData.CurrentLevelIndex = 0;
			playerData.LevelProgressions.Clear();
			playerData.TalentsUnlocked.Clear();

			return playerData;
		}

		public void Read()
		{
			BinaryReader r = null;
			try
			{
				r = new BinaryReader(new FileStream(SaveFile, FileMode.Open));
				var playerData = ProcessPlayerData(r.ReadAllBytes());
				r.Close();

				ApplyPlayerData(playerData);
			}
			catch (Exception e)
			{
				r.Close();
				NewSave();
			}
		}

		public void Save(bool force = false)
		{
			_timeToSave = Time.time + 0.3f;
			if (!_savingInProgress)
			{
				CoroutineHandler.StartStaticCoroutine(SaveCoroutine(force));
			}
		}

		private void SaveInternal()
		{
			BinaryWriter w = null;
			try
			{
				LastSavedTime = DateTime.UtcNow;
				_lastSaveTime = Time.time;
				w = new BinaryWriter(new FileStream(SaveFile, FileMode.OpenOrCreate, FileAccess.ReadWrite,
					FileShare.Delete));
				w.Write(GetBinaryData());
				w.Close();
			}
			catch (IOException e)
			{
				if (w != null)
					w.Close();
				if (File.Exists(SaveFile))
					File.Delete(SaveFile);
				Save();
			}
		}

		private IEnumerator SaveCoroutine(bool force)
		{
			_savingInProgress = true;
			if (!force)
			{
				while (_timeToSave > Time.time && Time.time - _lastSaveTime < 2f)
					yield return null;
			}

			SaveInternal();
			_savingInProgress = false;
		}

		private void ApplyPlayerData(PlayerData playerData)
		{
			VibrationOn = playerData.VibrationOn;
			SoundOn = playerData.SoundOn;
			MusicOn = playerData.MusicOn;
			PrivacyAndTermsAccepted = playerData.PrivacyAndTermsAccepted;
			LevelCount = playerData.LevelCount;
			LevelLoop = playerData.LevelLoop;
			Rated = playerData.Rated;
			RateUsAttempts = playerData.RateUsAttempts;
			RateUsLastTime = playerData.RateUsLastTime;
			LevelNumber = playerData.LevelNumber;
			TutorialStage = playerData.TutorialStage;
			LastSavedTime = playerData.LastSavedTime;
			InterstitialsShown = playerData.InterstitialsShown;
			CurrentSauce = playerData.CurrentSauce;
			CurrentLevelIndex = playerData.CurrentLevelIndex;
			LevelProgressions.Clear();
			LevelProgressions.AddRange(playerData.LevelProgressions);
			TalentsUnlocked.Clear();
			TalentsUnlocked.AddRange(playerData.TalentsUnlocked);
		}

		public PlayerData ProcessPlayerData(byte[] data)
		{
			using (var ms = new MemoryStream(data))
			using (var r = new BinaryReader(ms))
			{
				int ver = r.ReadInt32();
				if (ver > _version)
				{
					throw new Exception();
				}

				var pd = GetNewSave();
				pd.VibrationOn = r.ReadBoolean();
				pd.SoundOn = r.ReadBoolean();
				pd.MusicOn = r.ReadBoolean();
				pd.PrivacyAndTermsAccepted = r.ReadBoolean();
				pd.LevelCount = r.ReadInt32();
				pd.LevelLoop = r.ReadInt32();
				pd.Rated = r.ReadBoolean();
				pd.RateUsAttempts = r.ReadInt32();
				var longData = r.ReadInt64();
				var date = DateTime.FromBinary(longData);
				pd.RateUsLastTime = date;
				pd.LevelNumber = r.ReadInt32();
				pd.TutorialStage = r.ReadInt32();

				longData = r.ReadInt64();
				date = DateTime.FromBinary(longData);
				pd.LastSavedTime = date;
				pd.InterstitialsShown = r.ReadInt32();
				pd.CurrentSauce = r.ReadInt32();
				pd.CurrentLevelIndex = r.ReadInt32();
				var progCount = r.ReadInt32();

				for (int i = 0; i < progCount; i++)
				{
					var number = r.ReadInt32();
					var progression = r.ReadInt32();

					pd.LevelProgressions.Add(new LevelProgression()
						{ LevelNumber = number, Progression = progression });
				}

				var talentsCount = r.ReadInt32();
				for (int i = 0; i < talentsCount; i++)
				{
					var talentName = r.ReadString();
					pd.TalentsUnlocked.Add(talentName);
				}

				return pd;
			}
		}

		public byte[] GetBinaryData()
		{
			using (var ms = new MemoryStream())
			using (var w = new BinaryWriter(ms))
			{
				w.Write(_version);

				w.Write(VibrationOn);
				w.Write(SoundOn);
				w.Write(MusicOn);
				w.Write(PrivacyAndTermsAccepted);
				w.Write(LevelCount);
				w.Write(LevelLoop);
				w.Write(Rated);
				w.Write(RateUsAttempts);
				w.Write(RateUsLastTime.ToBinary());
				w.Write(LevelNumber);
				w.Write(TutorialStage);

				w.Write(LastSavedTime.ToBinary());
				w.Write(InterstitialsShown);
				w.Write(CurrentSauce);
				w.Write(CurrentLevelIndex);
				w.Write(LevelProgressions.Count);

				for (int i = 0; i < LevelProgressions.Count; i++)
				{
					w.Write(LevelProgressions[i].LevelNumber);
					w.Write(LevelProgressions[i].Progression);
				}

				w.Write(TalentsUnlocked.Count);
				for (int i = 0; i < TalentsUnlocked.Count; i++)
				{
					w.Write(TalentsUnlocked[i]);
				}

				return ms.ToArray();
			}
		}

		public void SetExperience(int exp)
		{
			CurrentCharacterExperience = exp;
			Save();
		}

		public void SetCharacterLevel(int level)
		{
			CurrentCharacterLevel = level;
			Save();
		}

		public void AddSauce(int amount)
		{
			CurrentSauce = amount;
			Save();
		}

		public LevelProgression GetLevelProgression(int levelNumber)
		{
			var prog = LevelProgressions.FirstOrDefault(p => p.LevelNumber == levelNumber);
			if (prog == null)
			{
				prog = new LevelProgression() { LevelNumber = levelNumber, Progression = 0 };
				LevelProgressions.Add(prog);
			}

			return prog;
		}

		public bool IsTalentUnlocked(string talentName)
		{
			return TalentsUnlocked.Contains(talentName);
		}

		public void AddLevelProgression(int levelNumber, int reward)
		{
			var prog = GetLevelProgression(levelNumber);
			prog.Progression += reward;
			Save();
		}
	}
}