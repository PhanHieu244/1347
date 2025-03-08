using System.Collections.Generic;
using System.Linq;
using _Content.Events;
using _Content.InGame.Managers;
using Extensions;
using UnityEngine;
using EventHandler = Opsive.Shared.Events.EventHandler;

namespace _Content.InGame.Enemies
{
	public class EnemySpawnArea : MonoBehaviour
	{
		[SerializeField] private Collider _boundsCollider;
		[SerializeField] private int _enemiesCount = 10;
		[SerializeField] private List<EnemiesInLevel> _enemiesInLevel;
		private float _checkTimer = 0f;

		private List<SimpleEnemyController> _currentEnemies = new List<SimpleEnemyController>();

		private void OnEnable()
		{
			EventHandler.RegisterEvent(InGameEvents.LevelInitialized, OnLevelInitialized);
			SpawnEnemies();
		}

		private void OnDisable()
		{
			EventHandler.UnregisterEvent(InGameEvents.LevelInitialized, OnLevelInitialized);
		}

		private void OnLevelInitialized()
		{
			SpawnEnemies();
		}

		private void Update()
		{
			if (_checkTimer <= 0f)
			{
				SpawnEnemies();
				_checkTimer = 2f;
			}
			else
			{
				_checkTimer -= Time.deltaTime * GameManager.Instance.TimeScale;
			}
		}

		private void SpawnEnemies()
		{
			var neededEnemiesCount = GetEnemiesCount();
			var currentEnemiesCount = _currentEnemies.Count;
			var enemiesToCreate = Mathf.Max(neededEnemiesCount - currentEnemiesCount, 0);
			for (int i = 0; i < enemiesToCreate; i++)
			{
				var prefab = GetEnemyPrefab();
				var position = GetEnemyPosition();
				var enemy = Instantiate(prefab, position, Quaternion.identity);
				/*var health = _currentLevel?.AdditionalHealth ?? 0;
				var damage = _currentLevel?.AdditionalDamage ?? 0;*/
				enemy.SetAdditionalSettings(0, 0);
				_currentEnemies.Add(enemy);
			}
		}

		private Vector3 GetEnemyPosition()
		{
			var tries = 0;
			var pos = Vector3.zero;
			while (tries < 100)
			{
				var point = _boundsCollider.GetRandomPointInCollider();
				var ray = new Ray(point + Vector3.up * 5f, Vector3.down);
				if (Physics.SphereCast(ray, .5f, out var hit, 3000f, LayerMaskManager.Instance.EnemyCheckMask))
				{
					pos = hit.point;
					pos.y = 0f;
					if (LayerMaskManager.Instance.GroundMask.Contains(hit.collider.gameObject.layer))
					{
						break;
					}
				}

				tries++;
			}

			return pos;
		}

		private SimpleEnemyController GetEnemyPrefab()
		{
			var enemies = _currentEnemies
				.GroupBy(e => e.EnemyId)
				.Select(g => (Name: g.Key, Percent: (g.Count() / (float)GetEnemiesCount()) * 100))
				.ToList();
			foreach (var enemiesInLevel in _enemiesInLevel)
			{
				var currentEnemy = enemies.FirstOrDefault(e => e.Name == enemiesInLevel.EnemyName);
				if (currentEnemy.Name == null || currentEnemy.Percent < enemiesInLevel.Percent)
				{
					var enemyPrefab = EnemyManager.Instance.GetEnemyPrefabByName(enemiesInLevel.EnemyName);
					if (enemyPrefab != null)
						return enemyPrefab;
				}
			}

			return EnemyManager.Instance.GetRandomEnemyPrefab();
		}

		private int GetEnemiesCount()
		{
			return _enemiesCount;
		}
	}
}