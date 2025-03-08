using _Content.InGame.Managers;
using Common.UI;
using TMPro;
using UnityEngine;

namespace _Content.InGame.UI.Main
{
	public class MainMenu: UIViewWrapper
	{
		[SerializeField] private TextMeshProUGUI _levelName;
		[SerializeField] private GameObject _leftButton;
		[SerializeField] private GameObject _rightButton;

		public override void ShowView(bool force = false)
		{
			base.ShowView(force);
			UpdateInfo();
		}

		private void UpdateInfo()
		{
			_levelName.text = GameManager.Instance.GetChapterCaption();
			_leftButton.SetActive(false);
			_rightButton.SetActive(false);
		}

		public void StartGame()
		{
			GameplayManager.Instance.StartGame();
		}
	}
}