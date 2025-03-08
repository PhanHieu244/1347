using Base;
using Cinemachine;
using UnityEngine;

namespace _Content.InGame.Managers
{
	public class CameraManager: Singleton<CameraManager>
	{
		[SerializeField] private CinemachineVirtualCamera _mainMenuCamera;
		[SerializeField] private CinemachineVirtualCamera _gameplayCamera;

		protected override void OnAwake()
		{
			base.OnAwake();
			_mainMenuCamera.Priority = 10;
			_gameplayCamera.Priority = 1;
		}

		public void ShowGameplayCamera()
		{
			_gameplayCamera.Priority = 10;
			_mainMenuCamera.Priority = 1;
		}

		public void ShowMainMenuCamera()
		{
			_gameplayCamera.Priority = 1;
			_mainMenuCamera.Priority = 10;
		}
	}
}