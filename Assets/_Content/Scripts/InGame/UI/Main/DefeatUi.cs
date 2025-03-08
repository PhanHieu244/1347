using _Content.Data;
using _Content.InGame.Managers;
using Common.UI;
using TMPro;
using UnityEngine;

namespace _Content.InGame.UI.Main
{
	public class DefeatUi: UIViewWrapper
	{
		[SerializeField] private TextMeshProUGUI _enemiesCount;
		[SerializeField] private TextMeshProUGUI _characterLevel;
		[SerializeField] private GameObject _reviveButton;
		[SerializeField] private GameObject _restartButton;

		public override void ShowView(bool force = false)
		{
			base.ShowView(force);
			UpdateInformation();
		}

		private void UpdateInformation()
		{
			_characterLevel.text = PlayerData.Instance.CurrentCharacterLevel.ToString();
			_enemiesCount.text = EnemyManager.Instance.GetDefeatedEnemyCount().ToString();
			_reviveButton.SetActive(!GameplayManager.Instance.CharacterRevived);
		}

		public void OnRevive()
		{
			GameManager.Instance.RevivePlayer();
		}

		public void OnRestart()
		{
			GameManager.Instance.RestartAfterDefeat();
		}
	}
}