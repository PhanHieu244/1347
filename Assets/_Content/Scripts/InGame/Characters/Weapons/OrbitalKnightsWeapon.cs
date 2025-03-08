using System.Collections.Generic;
using _Content.InGame.Managers;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace _Content.InGame.Characters.Weapons
{
	public class OrbitalKnightsWeapon : WeaponBase
	{
		[SerializeField] private bool _showKnivesAlways = true;
		[SerializeField] private float _knightsOffset = 1f;
		[SerializeField] private Transform _knightsHolder;
		[SerializeField] private string _knightsHolderTag;
		[SerializeField] private GameObject _knightPrefab;
		[SerializeReference] [Expandable] protected OrbitalKnivesSettings _settings;
		private List<GameObject> _currentKnights;
		[SerializeField] [ReadOnly] private float _currentRotation;
		private Character _character;
		private float _currentRotationSpeed;
		private float _currentCooldown;
		private float _currentLifeTime;
		private float _cooldownTimer;
		private float _lifeTimeTimer;
		private bool _knivesShown;
		public WeaponSettings Settings => _settings;
		public override int MaxLevel => Settings?.GetMaxLevel() ?? 1;

		protected override void OnAwake()
		{
			_character = GetComponentInParent<Character>();
			if (_character != null)
			{
				if (_knightsHolder == null)
				{
					_knightsHolder = _character.transform.FindDeepChildWithTag(_knightsHolderTag);
					if (_knightsHolder == null)
					{
						_knightsHolder = new GameObject("KnightsHolder").transform;
						_knightsHolder.SetParent(_character.Model);
						_knightsHolder.localPosition = Vector3.up * 1f;
					}
				}
			}

			_currentKnights = new List<GameObject>();
		}

		public override string GetDescriptionForLevel(int level)
		{
			return _settings.GetLevelDescription(level);
		}

		protected override float GetCooldown()
		{
			return 1f;
		}

		public override void UpdateWeaponLevel(int weaponLevel)
		{
			_weaponLevel = weaponLevel;
			foreach (var knight in _currentKnights)
			{
				Destroy(knight.gameObject);
			}

			_currentKnights.Clear();

			var knightsCount = _settings.GetKnivesCount(_weaponLevel);
			for (int i = 0; i < knightsCount; i++)
			{
				var rotation = Quaternion.Euler(0f, (360f / knightsCount * i), 0f);
				var knight = Instantiate(_knightPrefab, _knightsHolder);
				var localPosition = rotation * Vector3.forward * _knightsOffset;
				knight.transform.localPosition = localPosition;
				knight.transform.localRotation = rotation;
				_currentKnights.Add(knight);
			}

			_currentCooldown = _settings.GetCooldown(_weaponLevel);
			_currentLifeTime = _settings.GetLifeTime(_weaponLevel);
			_currentRotationSpeed = _settings.GetKnivesRotationSpeed(_weaponLevel);
			_currentRotation = 0f;
			_cooldownTimer = _currentCooldown;
			_lifeTimeTimer = 0f;
			_knivesShown = false;
		}

		public override void UpdateWeapon(float deltaTime)
		{
			if (!GameplayManager.Instance.GameStarted)
				return;
			var speed = _currentRotationSpeed * deltaTime;

			_currentRotation += speed;
			if (_currentRotation > 360f)
				_currentRotation -= 360f;
			_knightsHolder.localRotation = Quaternion.Euler(0f, _currentRotation, 0f);

			if (_cooldownTimer > 0f)
			{
				_cooldownTimer -= deltaTime;
			}

			if (_lifeTimeTimer > 0f)
			{
				_lifeTimeTimer -= deltaTime;
			}

			if (!_showKnivesAlways)
			{
				if (_knivesShown && _lifeTimeTimer <= 0f && _cooldownTimer <= 0f)
				{
					HideKnives();
				}
				else if (!_knivesShown && _cooldownTimer <= 0F)
				{
					ShowKnives();
				}
			}
		}

		private void HideKnives()
		{
			foreach (var knife in _currentKnights)
			{
				knife.transform.DOScale(Vector3.zero, 0.5f)
					.OnComplete(() => knife.SetActive(false));
			}

			_knivesShown = false;
			_cooldownTimer = _currentCooldown;
		}

		private void ShowKnives()
		{
			foreach (var knife in _currentKnights)
			{
				knife.SetActive(true);
				knife.transform.DOScale(Vector3.one, 0.5f);
			}

			_knivesShown = true;
			_lifeTimeTimer = _currentLifeTime;
		}
	}
}