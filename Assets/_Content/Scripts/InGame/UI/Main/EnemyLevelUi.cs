using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Content.InGame.UI.Main
{
	public class EnemyLevelUi: MonoBehaviour
	{
		[SerializeField] private RectTransform _rectTransform;
		[SerializeField] private Image _backgroundColor;
		[SerializeField] private TextMeshProUGUI _text;
		public void Initialize(Color uiColor, Color textColor, string difficultyName, float xPosition,
			float width)
		{
			_rectTransform.anchoredPosition = new Vector2(xPosition, _rectTransform.anchoredPosition.y);
			_rectTransform.sizeDelta = new Vector2(width, _rectTransform.sizeDelta.y);
			_backgroundColor.color = uiColor;
			_text.color = textColor;
			_text.text = difficultyName;
		}
	}
}