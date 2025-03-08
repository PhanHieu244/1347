using System.Collections;
using _Content.Data;
using _Content.InGame.UI.Misc;
using Common.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace _Content.InGame.Managers
{
	public class Bootstrapper: MonoBehaviour
	{
		[SerializeField] private UIViewWrapper _loading;
		private void Awake()
		{
			Application.targetFrameRate = 60;
		}
		
		private void OnEnable()
		{
			SetRenderScale();
			PrivacyAndTermsAccepted(false);
			//AppLovinManager.Instance.InitializeAds(ShowPrivacyAndTerms, ContinueWithoutGdpr);
			if (_loading != null)
				_loading.ShowView(true);
		}

		private void SetRenderScale()
		{
			var pipelineAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
			if (pipelineAsset != null)
			{
				pipelineAsset.renderScale = 1f;
			}

			DynamicResolutionScaling.Instance.RecalcRenderScaleAfterTime(2f);
		}
		
		public void PrivacyAndTermsAccepted(bool buttonClicked)
		{
			if (buttonClicked && !PlayerData.Instance.PrivacyAndTermsAccepted)
			{
				PlayerData.Instance.PrivacyAndTermsAccepted = true;
				//MaxSdk.SetHasUserConsent(PlayerData.Instance.PrivacyAndTermsAccepted);
				PlayerData.Instance.Save();
			}

			/*if (_gdprView != null && _gdprView.IsActive())
				_gdprView.Hide();*/

			//FB.Mobile.SetAdvertiserTrackingEnabled(MaxSdk.HasUserConsent());
			//AppLovinManager.Instance.Initialize();

			//StartCoroutine(StartAppsflyerSdk());
			SetSettings();
			StartCoroutine(StartGame());
		}

		private IEnumerator StartGame()
		{
			yield return new WaitForSeconds(0.1f);
			GameManager.Instance.StartGame();
		}
		
		public static void ShowPrivacy()
		{
			Application.OpenURL("https://devgame.me/policy");
		}
		private void SetSettings()
		{
			SettingsUi.SetMusicToManagers(PlayerData.Instance.MusicOn);
			SettingsUi.SetSoundToManagers(PlayerData.Instance.SoundOn);
			SettingsUi.SetVibrationToManagers(PlayerData.Instance.VibrationOn, false);
		}
	}
}