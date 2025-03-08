using System;
using System.Collections.Generic;
using System.Linq;
using _Content.Events;
using _Content.InGame.Characters;
using _Content.InGame.Enemies;
using _Content.InGame.GameDrop;
using _Content.InGame.Levels;
using Base;
using NaughtyAttributes;
using UnityEngine;
using EventHandler = Opsive.Shared.Events.EventHandler;
using Random = UnityEngine.Random;

namespace _Content.InGame.Managers
{
	public class EnemyManager : Singleton<EnemyManager>
	{
		private const float _timeSinceStartGameForEnemySpawn = 0.5f;

		[SerializeField] private bool _arenaRestrictionEnabled = false;
		[SerializeField] private float _distanceToPlayer;
		[SerializeField] private List<EnemyWave> _enemyWaves;
		[SerializeField] private bool _enableSpawn = true;
		[SerializeField] private LayerMask _groundMask;
		[SerializeField] private LayerMask _maskForSpawnCheck;
		[SerializeField] private List<SimpleEnemyController> _enemies;
		[SerializeField] private Transform _hitPSParent;
		[SerializeField] private Transform _deathPSParent;
		[SerializeField] private bool _enableExpDropping;
		[SerializeField] private List<ExpGem> _expGemPrefabs;
		[SerializeField] private ExpGem _expGemPrefab;
		[SerializeField] private HealDrop _healDropPrefab;
		[SerializeField] private CollectAllExpDrop _collectAllExpDropPrefab;
		[SerializeField] private float _collectAllGemsDropTime = 60f;

		private Dictionary<string, ParticleSystem> _hitParticles;
		private Dictionary<string, ParticleSystem> _deathParticles;

		private Transform _playersTransform;
		private List<SimpleEnemyController> _currentEnemies = new List<SimpleEnemyController>();
		private Character _character;
		private Camera _camera;
		private List<EnemyWave> _currentWaves;
		private EnemyWave _currentWave;
		private List<EnemyWave.ConstantEnemiesOnLevel> _enemiesOnLevelList;
		private List<EnemyWave.ConstantEnemiesOnLevel> _currentEnemiesBurst;
		private EnemyWave.ConstantEnemiesOnLevel _currenEnemyOnLevel;
		private float _timeFromPreviousWave;
		private bool _waveIsActive;
		private HealDrop _currentHealDrop;
		private float _lastCollectAllGemsDropTime;
		private bool _initialized;
		private int _defeatedEnemiesCount;
		private float _startTimer;

		public bool ArenaRestrictionEnabled => _arenaRestrictionEnabled;
		public float DistanceToPlayer => _distanceToPlayer;

		protected override void OnAwake()
		{
			base.OnAwake();
			_currentEnemies = new List<SimpleEnemyController>();
			_hitParticles = new Dictionary<string, ParticleSystem>();
			_deathParticles = new Dictionary<string, ParticleSystem>();
			foreach (SimpleEnemyController enemyPrefab in _enemies)
			{
				var hitps = Instantiate(enemyPrefab.HitParticleSystem, _hitPSParent);
				var deathPs = Instantiate(enemyPrefab.DeathParticleSystem, _deathPSParent);
				_hitParticles.Add(enemyPrefab.EnemyId, hitps);
				_deathParticles.Add(enemyPrefab.EnemyId, deathPs);
			}
		}

		public void Initialize(LevelInfo prevLevelInfo)
		{
			_currentEnemies.Clear();
			_currentWaves = prevLevelInfo.EnemyWave.ToList();
			_playersTransform = null;
			_character = null;
			GetPlayersTransform();
			
			SetCurrentEnemyWave(_currentWaves[0]);
			UpdateEnemyLevel();
			_camera = Camera.main;
			_defeatedEnemiesCount = 0;
			_startTimer = _timeSinceStartGameForEnemySpawn;
			_timeFromPreviousWave = 0f;
			_initialized = true;
		}

		public void Deinitialize()
		{
			_currentWaves = null;
			_currentWave = null;
			_enemiesOnLevelList = null;
			_currentEnemiesBurst = null;
			_currenEnemyOnLevel = null;
			_timeFromPreviousWave = 0f;
			_waveIsActive = false;
			_lastCollectAllGemsDropTime = 0f;
			_currentHealDrop = null;
			_playersTransform = null;
			_character = null;

			_currentEnemies.ForEach(e =>
			{
				if (e != null)
					Destroy(e.gameObject);
			});

			_currentEnemies.Clear();

			var enemies = FindObjectsOfType<SimpleEnemyController>();
			foreach (var e in enemies)
			{
				Destroy(e.gameObject);
			}

			_initialized = false;
		}

		private void SetCurrentEnemyWave(EnemyWave currentWave)
		{
			_currentWave = currentWave;
			_enemiesOnLevelList = _currentWave.EnemiesOnLevel.ToList();
			_currentEnemiesBurst = _currentWave.EnemiesBurst.ToList();
			UpdateEnemyLevel();
			_timeFromPreviousWave = GameplayManager.Instance.GameTimer;
			_waveIsActive = true;

			if (_currentWave.IsBossWave)
				UIManager.Instance.NotificationUi.ShowView("BOSS");
			EventHandler.ExecuteEvent(InGameEvents.WaveStateChanged);
		}

		private void TryToSetNextWave()
		{
			if (GameplayManager.Instance.GameTimer - _timeFromPreviousWave > _currentWaves[0].TimeFromPrevWaveToStart)
			{
				SetCurrentEnemyWave(_currentWaves[0]);
			}
		}

		private void Update()
		{
			if (!_initialized)
				return;

			ClearCurrentEnemies();
			if (!GameplayManager.Instance.GameStarted)
				return;
			if (_startTimer > 0f)
			{
				_startTimer -= Time.deltaTime;
				return;
			}

			if (_currentWave == null)
			{
				TryToSetNextWave();
				return;
			}

			if (!_waveIsActive && _currentWave != null)
			{
				if (CanCompleteWave())
				{
					CompleteWave();
				}
			}

			if (_waveIsActive && GameplayManager.Instance.GameTimer - _timeFromPreviousWave > _currentWave.Duration)
			{
				_waveIsActive = false;
			}


			UpdateEnemyLevel();

			if (_currentEnemies.Count > 0)
			{
				MoveEnemiesOutOfCameraBounds();
			}

			if (!_enableSpawn)
				return;

			if (_currentEnemiesBurst.Count > 0)
			{
				var nextBurst = _currentEnemiesBurst[0];
				var timer = GameplayManager.Instance.GameTimer - _timeFromPreviousWave;
				if (nextBurst.Time < timer)
				{
					BurstWaveInstantiation(nextBurst);
					_currentEnemiesBurst.RemoveAt(0);
				}
			}

			if (_waveIsActive)
			{
				var neededEnemiesCount = GetEnemiesCount();
				var currentEnemiesCount = _currentEnemies.Count;
				var enemiesToCreate = Mathf.Max(neededEnemiesCount - currentEnemiesCount, 0);
				for (int i = 0; i < enemiesToCreate; i++)
				{
					var prefab = GetEnemyPrefab();
					var position = GetEnemyPosition();
					var enemy = Instantiate(prefab, position, Quaternion.identity);
					enemy.SetAdditionalSettings(0, 0);
					_currentEnemies.Add(enemy);
				}
			}
		}

		private bool CanCompleteWave()
		{
			if (_currentEnemies.Count > 0)
				return false;

			return true;
		}

		private void CompleteWave()
		{
			_currentWave = null;
			_currentWaves.RemoveAt(0);
			_timeFromPreviousWave = GameplayManager.Instance.GameTimer;
			EventHandler.ExecuteEvent(InGameEvents.WaveStateChanged);
			if (_currentWaves.Count == 0)
			{
				GameplayManager.Instance.CompleteLevel();
			}
		}


		[Button()]
		public void BurstWaveInstantiationText()
		{
			var wave = new EnemyWave.ConstantEnemiesOnLevel()
			{
				Time = 0,
				EnemiesInLevels = new List<EnemiesInLevel>()
				{
					new EnemiesInLevel() { EnemyName = "pea", Percent = 80 },
					new EnemiesInLevel() { EnemyName = "tomato", Percent = 20 }
				},
				MaxEnemiesCountOnLevel = 50
			};
			BurstWaveInstantiation(wave);
		}

		private void BurstWaveInstantiation(EnemyWave.ConstantEnemiesOnLevel enemyWave)
		{
			var allEnemyCount = enemyWave.MaxEnemiesCountOnLevel;
			foreach (var enemiesInLevel in enemyWave.EnemiesInLevels)
			{
				var enemyPrefab = _enemies.FirstOrDefault(e => e.EnemyId == enemiesInLevel.EnemyName);
				if (enemyPrefab == null)
					enemyPrefab = _enemies.FirstOrDefault();

				var count = Mathf.CeilToInt(allEnemyCount * enemiesInLevel.Percent / 100f);

				for (int j = 0; j < count; j++)
				{
					var position = GetEnemyPosition(false);
					var enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
					enemy.SetAdditionalSettings(0, 0);
					_currentEnemies.Add(enemy);
				}
			}
		}

		private void MoveEnemiesOutOfCameraBounds()
		{
			var outOfCameraEnemies = _currentEnemies.Where(e =>
			{
				var vp = _camera.WorldToViewportPoint(e.transform.position);
				if (vp.x < -0.3f || vp.y < -0.5f || vp.x > 1.3f || vp.y > 1.05f)
				{
					return true;
				}

				return false;
			}).ToList();

			if (outOfCameraEnemies.Count > 0)
			{
				foreach (var enemy in outOfCameraEnemies)
				{
					enemy.transform.position = GetEnemyPosition();
				}
			}
		}

		private Vector3 GetEnemyPosition(bool playerDependent = true)
		{
			GetPlayersTransform();
			var moveVector = _character.Controller.Velocity;
			var angles = new Vector2(-180f, 180f);
			if (playerDependent && _currentEnemies.Count > GetEnemiesCount() / 2 && moveVector != Vector3.zero)
			{
				var angle = -Vector3.SignedAngle(Vector3.forward, moveVector, Vector3.up);
				angles = new Vector2(angle - 45f, angle + 45f);
			}

			return GetOutOfCameraPosition(angles, playerDependent);
		}

		public SimpleEnemyController GetEnemyPrefabByName(string enemyId)
		{
			var enemyPrefab = _enemies.FirstOrDefault(e => e.EnemyId == enemyId);
			if (enemyPrefab != null)
				return enemyPrefab;

			return null;
		}

		public SimpleEnemyController GetRandomEnemyPrefab()
		{
			return _enemies.GetRandomElement();
		}

		private SimpleEnemyController GetEnemyPrefab()
		{
			var enemies = _currentEnemies
				.GroupBy(e => e.EnemyId)
				.Select(g => (Name: g.Key, Percent: (g.Count() / (float)GetEnemiesCount()) * 100))
				.ToList();
			foreach (var enemiesInLevel in _currenEnemyOnLevel.EnemiesInLevels)
			{
				var currentEnemy = enemies.FirstOrDefault(e => e.Name == enemiesInLevel.EnemyName);
				if (currentEnemy.Name == null || currentEnemy.Percent < enemiesInLevel.Percent)
				{
					var enemyPrefab = _enemies.FirstOrDefault(e => e.EnemyId == enemiesInLevel.EnemyName);
					if (enemyPrefab != null)
						return enemyPrefab;
				}
			}

			return _enemies.GetRandomElement();
		}

		private int GetEnemiesCount()
		{
			return _currenEnemyOnLevel?.MaxEnemiesCountOnLevel ?? 0;
		}

		private void UpdateEnemyLevel()
		{
			for (int i = 0; i < _enemiesOnLevelList.Count; i++)
			{
				var level = _enemiesOnLevelList[i];
				var timer = GameplayManager.Instance.GameTimer - _timeFromPreviousWave;
				if (i == _enemiesOnLevelList.Count - 1 || level.Time <= timer &&
				    _enemiesOnLevelList[i + 1].Time > timer)
				{
					_currenEnemyOnLevel = level;
					break;
				}
			}
		}

		private Vector2 GetOutOfCameraViewPortPositionFromDirection(Vector2 direction, float ratio = 1f)
		{
			var center = new Vector2(0.5f, 0.5f);
			var magn = 0.5f;
			var tries = 0;
			var pos = center + direction * magn;
			while (tries < 10000)
			{
				pos = center + direction * magn;
				if (pos.x <= -0.1f * ratio || pos.x >= 1.1f * ratio || pos.y <= -0.3f * ratio || pos.y >= 1.02f * ratio)
					break;
				magn += 0.01f;
				tries++;
			}

			return pos;
		}

		private Vector3 GetOutOfCameraPosition(Vector2 randomAngles, bool playerDependent)
		{
			var tries = 0;
			var pos = Vector3.zero;
			var distanceRatio = 1f;
			while (tries < 100)
			{
				if (tries > 50 && playerDependent)
					randomAngles = new Vector2(-180f, 180f);
				var randomAngle = Random.Range(randomAngles.x, randomAngles.y);

				Vector2 dir = Quaternion.Euler(0f, 0f, randomAngle) * Vector3.up;
				var viewPortPos = GetOutOfCameraViewPortPositionFromDirection(dir, distanceRatio);
				var ray = _camera.ViewportPointToRay(viewPortPos);
				if (Physics.SphereCast(ray, .5f, out var hit, 3000f, _maskForSpawnCheck))
				{
					pos = hit.point;
					pos.y = 0f;
					if (_groundMask.Contains(hit.collider.gameObject.layer) && CanSpawnWithinDistance(pos))
					{
						break;
					}
				}

				tries++;
				if (tries % 5 == 0)
					distanceRatio = 1.1f;
			}

			return pos;
		}

		private bool CanSpawnWithinDistance(Vector3 position)
		{
			if (!_arenaRestrictionEnabled)
				return true;

			var distance = Vector3.Distance(GameplayManager.Instance.ArenaCenter, position);
			if (distance <= GameplayManager.Instance.ArenaSize)
				return true;

			return false;
		}

		private void ClearCurrentEnemies()
		{
			_currentEnemies.RemoveAll(e => e == null);
		}

		public Transform GetPlayersTransform()
		{
			if (_playersTransform == null)
			{
				_character = FindObjectOfType<Character>();
				if (_character != null)
					_playersTransform = _character.transform;
			}

			return _playersTransform;
		}

		public void OnEnemyDeath(SimpleEnemyController enemy)
		{
			if (_currentEnemies.Contains(enemy))
				_currentEnemies.Remove(enemy);

			var exp = enemy.ExperienceForKill;
			var expCount = enemy.GemsToDrop.Count;
			if (_enableExpDropping)
			{
				var restExpo = exp;
				var expPerCount = Mathf.CeilToInt(exp / (float)expCount);
				var expToGem = expPerCount;
				for (int i = 0; i < expCount; i++)
				{
					if (expToGem > 0 && restExpo < expToGem)
					{
						expToGem = restExpo;
					}

					var prefab = GetExpGemPrefab(enemy.GemsToDrop[i]);
					DropExperience(prefab, enemy.transform.position, expToGem);
					restExpo -= expPerCount;
					if (restExpo == 0)
						expToGem = 0;
				}
			}
			else
			{
				OnExpCollected(exp);
			}

			if (NeedDropHeal())
				DropHeal(enemy.transform.position);


			if (GameplayManager.Instance.GameTimer - _lastCollectAllGemsDropTime > _collectAllGemsDropTime)
			{
				DropCollectAllGems(enemy.transform.position);
			}

			Destroy(enemy.gameObject);
			_defeatedEnemiesCount++;
		}

		private void DropCollectAllGems(Vector3 pos)
		{
			var drop = Instantiate(_collectAllExpDropPrefab, pos, Quaternion.Euler(0f, Random.Range(-180f, 180f), 0f));
			var random = Random.insideUnitSphere;
			random.y = 0f;
			random.Normalize();
			drop.Drop(pos + random * Random.Range(8f, 15f), 1);
			_lastCollectAllGemsDropTime = GameplayManager.Instance.GameTimer;
		}

		private void DropHeal(Vector3 pos)
		{
			_currentHealDrop = Instantiate(_healDropPrefab, pos, Quaternion.Euler(0f, Random.Range(-180f, 180f), 0f));
			var random = Random.insideUnitSphere;
			random.y = 0f;
			random.Normalize();
			_currentHealDrop.Drop(pos + random * Random.Range(5f, 10f), 1);
		}

		public void OnHealingDropCollected()
		{
			if (GameplayManager.Instance.Character != null)
				GameplayManager.Instance.Character.HealFromDrop();
			_currentHealDrop = null;
		}

		private bool NeedDropHeal()
		{
			var character = GameplayManager.Instance.Character;
			if (character != null && character.Health.CurrentHealth < character.Health.MaximumHealth / 2 &&
			    _currentHealDrop == null)
			{
				return true;
			}

			return false;
		}

		private void DropExperience(Vector3 pos, int exp)
		{
			var prefab = GetExpGemPrefab(exp);
			DropExperience(prefab, pos, exp);
		}
		private void DropExperience(ExpGem prefab, Vector3 pos, int exp)
		{
			var expGem = Instantiate(prefab, pos, Quaternion.Euler(0f, Random.Range(-180f, 180f), 0f));
			var random = Random.insideUnitSphere;
			random.y = 0f;
			random.Normalize();
			expGem.Drop(pos + random * Random.Range(3f, 6f), exp);
		}


		private ExpGem GetExpGemPrefab(int exp)
		{
			if (_expGemPrefabs.Count == 0)
				return _expGemPrefab;

			if (exp > _expGemPrefabs.Count)
				exp = _expGemPrefabs.Count;

			return _expGemPrefabs[exp - 1];
		}


		public void OnExpCollected(int exp)
		{
			GameplayManager.Instance.AddExperience(exp);
		}

		public ParticleSystem GetHitParticles(string enemyId)
		{
			if (_hitParticles.TryGetValue(enemyId, out var ps))
				return ps;

			return null;
		}

		public ParticleSystem GetDeathParticle(string enemyId)
		{
			if (_deathParticles.TryGetValue(enemyId, out var ps))
				return ps;

			return null;
		}

		public void OnPlayerDamage(Vector3 playerPosition, float radius, float distance)
		{
			var enemies = _currentEnemies
				.Where(ce => ce != null && Vector3.Distance(ce.transform.position, playerPosition) < radius).ToList();

			foreach (var enemy in enemies)
			{
				enemy.ForceFromPoint(playerPosition, distance);
			}
		}

		public SimpleEnemyController GetNearestEnemy(Vector3 position, float excludedRange,
			List<SimpleEnemyController> excludedEnemies = null)
		{
			if (_playersTransform == null)
				return null;

			if (excludedEnemies == null)
				excludedEnemies = new List<SimpleEnemyController>();

			var enemy = _currentEnemies
				.OrderBy(ce =>
				{
					var distance = Vector3.Distance(ce.transform.position, position);
					if (distance <= excludedRange)
						distance = 2000;
					return distance;
				})
				.FirstOrDefault(e => !excludedEnemies.Contains(e));
			if (enemy != null)
			{
				return enemy;
			}

			return null;
		}

		public string GetCurrentWaveCaption()
		{
			if (_currentWave == null)
			{
				return "Resting";
			}

			return _currentWave.WaveCaption;
		}

		public float GetCurrentWaveTimerValue()
		{
			if (_currentWaves.Count == 0)
				return 1f;
			if (_currentWave == null)
			{
				if (_currentWaves[0].TimeFromPrevWaveToStart < 0.001f)
				{
					return 1f;
				}
				return (GameplayManager.Instance.GameTimer - _timeFromPreviousWave) /
				       _currentWaves[0].TimeFromPrevWaveToStart;
			}
			else
			{
				if (_currentWave.IsBossWave)
					return 1f;
				if (_currentWave.Duration < 0.001f)
				{
					return 1f;
				}
				return (GameplayManager.Instance.GameTimer - _timeFromPreviousWave) / _currentWave.Duration;
			}
		}

		public int GetDefeatedEnemyCount()
		{
			return _defeatedEnemiesCount;
		}

		public int GetLevelProgressForAnalytics()
		{
			if (_currentWave == null)
				return 100;
			if (_currentWave.IsBossWave)
				return 90;

			return Mathf.RoundToInt(Mathf.Lerp(0f, 80f,
				Mathf.Clamp01(GameplayManager.Instance.GameTimer / _currentWave.Duration)));
		}
	}
}