using _Content.InGame.Characters;
using Cinemachine;
using UnityEngine;

namespace _Content.Cameras
{
	[RequireComponent(typeof(CinemachineVirtualCamera))]
	public class CinemachineCameraController : MonoBehaviour
	{
		[SerializeField] private bool _setLookAt;
		public bool FollowsPlayer { get; set; }
		public Character TargetCharacter { get; set; }
		protected CinemachineVirtualCamera _virtualCamera;

		protected virtual void Awake()
		{
			_virtualCamera = GetComponent<CinemachineVirtualCamera>();
		}

		public virtual void SetTarget(Character character)
		{
			TargetCharacter = character;
		}

		public virtual void StartFollowing()
		{
			if (FollowsPlayer)
			{
				return;
			}

			FollowsPlayer = true;
			_virtualCamera.Follow = TargetCharacter.CameraTarget.transform;
			if (_setLookAt)
				_virtualCamera.LookAt = TargetCharacter.Model.transform;
			_virtualCamera.enabled = true;
		}


		public virtual void StopFollowing()
		{
			if (!FollowsPlayer)
			{
				return;
			}

			FollowsPlayer = false;
			_virtualCamera.Follow = null;
			_virtualCamera.enabled = false;
		}
	}
}