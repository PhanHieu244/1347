using _Content.InGame.Characters.Talents;
using _Content.InGame.Managers;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Content.InGame.UI.Main
{
	public class UnlockSkillUi: UIViewWrapper
	{
		[SerializeField] private TextMeshProUGUI _captionText;
		[SerializeField] private Image _skillIcon;
		[SerializeField] private TextMeshProUGUI _descriptionText;
		private TalentBase _currentTalent;

		public void ShowView(TalentBase talentBase, bool force = false)
		{
			_currentTalent = talentBase;
			GameManager.Instance.UnlockSkillFeedback?.PlayFeedbacks();
			ShowView(force);
		}
		public override void ShowView(bool force = false)
		{
			base.ShowView(force);
			UpdateInformation();
		}

		private void UpdateInformation()
		{
			_captionText.text = _currentTalent.AbilityCaption;
			_skillIcon.sprite = _currentTalent.AbilityIcon;
			_descriptionText.text = _currentTalent.AbilityDescription;
		}

		public void OnNextButtonClick()
		{
			GameManager.Instance.HideRewardUi();
		}
	}
}