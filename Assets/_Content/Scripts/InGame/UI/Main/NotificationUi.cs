using _Content.InGame.Managers;
using Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Content.InGame.UI.Main
{
	public class NotificationUi: UIViewWrapper
	{
		[SerializeField] private TextMeshProUGUI _waveStartedText;
		[SerializeField] private RectTransform _waveStartedTransform;
		[SerializeField] private CanvasGroup _waveStartedCg;
		[SerializeField] private float _startYPosition;
		[SerializeField] private float _endYPosition;

		public void ShowView(string str, bool force = false)
		{
			_waveStartedText.text = str;
			ShowView(force);
		}
		public override void ShowView(bool force = false)
		{
			base.ShowView(force);
			StartAnimation();
		}

		private void StartAnimation()
		{
			var pos = _waveStartedTransform.anchoredPosition;
			pos.y = _startYPosition;
			_waveStartedTransform.anchoredPosition = pos;
			
			_waveStartedCg.alpha = 0f;
			_waveStartedCg.DOFade(1f, 0.5f);
			_waveStartedTransform.DOAnchorPosY(_endYPosition, 3f)
				.OnComplete(() => _waveStartedCg.DOFade(0f, 0.5f).OnComplete(() => HideView()));
		}
	}
}