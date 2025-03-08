using _Content.InGame.Managers;
using Common.UI;
using Doozy.Engine.Touchy;
using UnityEngine;

namespace _Content.InGame.UI.Main
{
	public class MovementTutorialUi: UIViewWrapper
	{
		private void Update()
		{
			if (!_shown)
				return;

			if (TouchDetector.Instance.TouchInProgress)
			{
				var touch = TouchDetector.Instance.CurrentTouchInfo;
				if (touch.Touch.phase == TouchPhase.Began)
					GameManager.Instance.ReturnFromMovementTutorial();
			}
		}

		public void OnTap()
		{
			
				
		}
	}
}