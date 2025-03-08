using _Content.Data;
using _Content.Events;
using _Content.InGame.Managers;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EventHandler = Opsive.Shared.Events.EventHandler;

namespace _Content.InGame.UI.Main
{
	public class CharacterProgressUi : UIViewWrapper
	{
		[SerializeField] private Image _expImageSlider;
		[SerializeField] private TextMeshProUGUI _levelText;

		private void OnEnable()
		{
			EventHandler.RegisterEvent<int>(InGameEvents.CharacterExperienceChanged, OnExperienceChanged);
			EventHandler.RegisterEvent<int>(InGameEvents.CharacterLevelChanged, OnCharacterLevelChanged);
			UpdateSlider();
		}

		private void OnDisable()
		{
			EventHandler.UnregisterEvent<int>(InGameEvents.CharacterExperienceChanged, OnExperienceChanged);
			EventHandler.UnregisterEvent<int>(InGameEvents.CharacterLevelChanged, OnCharacterLevelChanged);
		}

		private void OnExperienceChanged(int exp)
		{
			UpdateSlider();
		}


		private void OnCharacterLevelChanged(int level)
		{
			UpdateSlider();
		}

		private void UpdateSlider()
		{
			_levelText.text = $"{PlayerData.Instance.CurrentCharacterLevel}";
			var value = PlayerData.Instance.CurrentCharacterExperience / (float)GameplayManager.Instance.GetExpToNextLevel();
			_expImageSlider.fillAmount = value;
		}
	}
}