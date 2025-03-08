using System;
using _Content.Events;
using _Content.InGame.Damage;
using _Content.InGame.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EventHandler = Opsive.Shared.Events.EventHandler;

namespace _Content.InGame.UI.Main
{
	public class CharacterHealthUi : MonoBehaviour
	{
		[SerializeField] private RectTransform _canvasRectTransform;
		[SerializeField] private Image _sliderImage;
		[SerializeField] private TextMeshProUGUI _text;
		[SerializeField] private CanvasGroup _canvasGroup;
		private Health _health;
		private float _destinationT;
		private float _currentT;
		private Tweener _changeValueTween;

		private void Awake()
		{
			_health = GetComponentInParent<Health>();
		}

		private void OnEnable()
		{
			EventHandler.RegisterEvent(InGameEvents.CharacterHealthChanged, CharacterHealthChanged);
			EventHandler.RegisterEvent(InGameEvents.GameStart, OnGameStartChanged);
			_health.OnHit += OnHit;
			UpdateValue(true);
		}



		private void OnDisable()
		{
			EventHandler.UnregisterEvent(InGameEvents.CharacterHealthChanged, CharacterHealthChanged);
			EventHandler.UnregisterEvent(InGameEvents.GameStart, OnGameStartChanged);
			_health.OnHit -= OnHit;
		}

		private void OnGameStartChanged()
		{
			UpdateValue(true);
		}
		
		private void OnDestroy()
		{
			if (_changeValueTween != null && _changeValueTween.IsActive())
				_changeValueTween.Kill();
		}

		private void CharacterHealthChanged()
		{
			UpdateValue();
		}

		private void OnHit(int arg1, int arg2)
		{
			UpdateValue();
		}

		private void UpdateValue(bool force = false)
		{
			_canvasGroup.alpha = GameplayManager.Instance.GameStarted ? 1f : 0f;
			
			_destinationT = _health.CurrentHealth / (float)_health.MaximumHealth;
			if (force)
			{
				_currentT = _destinationT;
				UpdateSlider(_currentT);
			}
			else
			{
				if (_changeValueTween != null && _changeValueTween.IsActive())
					_changeValueTween.Kill();

				var startValue = _currentT;
				_changeValueTween = DOVirtual.Float(startValue, _destinationT, .1f, (x) =>
				{
					_currentT = x;
					UpdateSlider(_currentT);
				});
			}

			_text.text = $"{_health.CurrentHealth}/{_health.MaximumHealth}";
		}

		private void UpdateSlider(float value)
		{
			var sizeDelta = _sliderImage.rectTransform.sizeDelta;
			sizeDelta.x = _canvasRectTransform.sizeDelta.x * value;
			_sliderImage.rectTransform.sizeDelta = sizeDelta;
			//_sliderImage.fillAmount = value;
		}
	}
}