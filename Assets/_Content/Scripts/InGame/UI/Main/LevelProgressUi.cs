using _Content.Events;
using _Content.InGame.Managers;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EventHandler = Opsive.Shared.Events.EventHandler;

namespace _Content.InGame.UI.Main
{
	public class LevelProgressUi : UIViewWrapper
	{
		[SerializeField] private TextMeshProUGUI _currentWaveCaption;
		[SerializeField] private Slider _waveSlider;

		private void OnEnable()
		{
			EventHandler.RegisterEvent(InGameEvents.WaveStateChanged, OnWaveStateChanged);
		}

		private void OnDisable()
		{
			EventHandler.UnregisterEvent(InGameEvents.WaveStateChanged, OnWaveStateChanged);
		}

		private void Update()
		{
			if (_shown)
			{
				UpdateWaveTimer();
			}
		}

		private void OnWaveStateChanged()
		{
			UpdateWaveInfo();
		}

		private void UpdateWaveInfo()
		{
			_currentWaveCaption.text = EnemyManager.Instance.GetCurrentWaveCaption();
			UpdateWaveTimer();
		}

		private void UpdateWaveTimer()
		{
			var timerValue = EnemyManager.Instance.GetCurrentWaveTimerValue();
			_waveSlider.value = Mathf.Clamp01(timerValue);
		}

		public override void ShowView(bool force = false)
		{
			base.ShowView(force);
			UpdateWaveInfo();
		}
	}
}