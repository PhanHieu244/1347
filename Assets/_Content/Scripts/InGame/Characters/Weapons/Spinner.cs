using System;
using System.Collections.Generic;
using _Content.Events;
using _Content.InGame.Managers;
using _Content.InGame.TweenAnimation;
using DG.Tweening;
using UnityEngine;
using EventHandler = Opsive.Shared.Events.EventHandler;
using Random = UnityEngine.Random;

namespace _Content.InGame.Characters.Weapons
{
	public class Spinner : MonoBehaviour
	{
		[SerializeField] private DropAnimation _dropAnimation;
		[SerializeField] private Transform _bladeHolder;
		[SerializeField] private List<GameObject> _blades;
		private float _currentRotationSpeed;
		private float _currentRotation;
		private float _lifeTimer;
		private Transform _currentBlades;

		private void OnEnable()
		{
			EventHandler.RegisterEvent<int>(InGameEvents.SpinnerBladesCountChanged, OnSpinnerBladesCountChanged);
			EventHandler.RegisterEvent(InGameEvents.DeinitializeLevel, OnLevelDeinitialized);
		}


		private void OnDisable()
		{
			EventHandler.UnregisterEvent<int>(InGameEvents.SpinnerBladesCountChanged, OnSpinnerBladesCountChanged);
			EventHandler.UnregisterEvent(InGameEvents.DeinitializeLevel, OnLevelDeinitialized);
		}

		private void OnLevelDeinitialized()
		{
			Destroy(gameObject);
		}

		private void OnSpinnerBladesCountChanged(int bladesCount)
		{
			if (bladesCount > _blades.Count)
				bladesCount = _blades.Count;

			for (int i = 0; i < _blades.Count; i++)
			{
				var blade = _blades[i];
				if (i == bladesCount - 1)
					_currentBlades = blade.transform;
				blade.SetActive(i == bladesCount - 1);
			}
		}

		private void Update()
		{
			var deltaTime = Time.deltaTime * GameManager.Instance.TimeScale;
			if (_lifeTimer > 0f)
			{
				_lifeTimer -= deltaTime;
				if (_lifeTimer <= 0f)
					Destroy(gameObject);
			}

			var speed = _currentRotationSpeed * deltaTime;

			_currentRotation += speed;
			if (_currentRotation > 360f)
				_currentRotation -= 360f;
			if (_currentBlades != null)
				_currentBlades.localRotation = Quaternion.Euler(0f, _currentRotation, 0f);
		}

		public void SetProperties(float lifeTime, float rotationSpeed, int bladesCount)
		{
			if (bladesCount > _blades.Count)
				bladesCount = _blades.Count;

			for (int i = 0; i < _blades.Count; i++)
			{
				var blade = _blades[i];
				if (i == bladesCount - 1)
					_currentBlades = blade.transform;
				blade.SetActive(i == bladesCount - 1);
			}

			_lifeTimer = lifeTime;
			_currentRotationSpeed = rotationSpeed;
			_currentRotation = Random.Range(0, 180f);
		}

		public void Drop(Vector3 pos)
		{
			transform.DOKill();
			_dropAnimation.DoAnimation(transform, pos, 6f, () => { });
		}
	}
}