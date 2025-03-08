using System;
using _Content.Cameras;

namespace _Content.InGame.Characters.Abilities
{
	public class CharacterCameraFollowerHandler: CharacterAbility
	{
		public override void Setup()
		{
			base.Setup();
			var controllers = FindObjectsOfType<CinemachineCameraController>();
			foreach (var controller in controllers)
			{
				controller.SetTarget(_character);
				controller.StartFollowing();
			}
		}

		private void OnDestroy()
		{
			var controllers = FindObjectsOfType<CinemachineCameraController>();
			foreach (var controller in controllers)
			{
				controller.StopFollowing();
			}
		}
	}
}