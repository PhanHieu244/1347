﻿using UnityEngine;

namespace Utility
{
	public class ToastUtility
	{
		public static void ShowToastMessage(string message)
		{
#if UNITY_ANDROID
			if (Application.platform == RuntimePlatform.Android)
			{
				AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

				if (unityActivity != null)
				{
					AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
					unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
					{
						AndroidJavaObject toastObject =
							toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
						toastObject.Call("show");
					}));
				}
			}
			else
			{
				Debug.Log($"ToastMessage: {message}");
			}
#else
			Debug.Log($"ToastMessage: {message}");
#endif
		}
	}
}