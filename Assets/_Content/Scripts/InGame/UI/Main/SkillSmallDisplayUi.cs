using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Content.InGame.UI.Main
{
	public class SkillSmallDisplayUi: MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _lvlText;
		[SerializeField] private Image _icon;

		public void Show(Sprite icon, int lvl)
		{
			_icon.sprite = icon;
			_lvlText.text = $"{lvl}";
			_lvlText.gameObject.SetActive(true);
			_icon.gameObject.SetActive(true);
		}

		public void Hide()
		{
			_icon.sprite = null;
			_lvlText.text = "";
			_lvlText.gameObject.SetActive(false);
			_icon.gameObject.SetActive(false);
		}
	}
}