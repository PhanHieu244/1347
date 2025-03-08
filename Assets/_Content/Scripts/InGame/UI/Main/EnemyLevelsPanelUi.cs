using _Content.InGame.Managers;
using Common.UI;
using UnityEngine;

namespace _Content.InGame.UI.Main
{
	public class EnemyLevelsPanelUi : UIViewWrapper
	{
		[SerializeField] private EnemyLevelUi _enemyLevelUiPrefab;
		[SerializeField] private RectTransform _difficultyParents;
		[SerializeField] private float _sizePerMinute = 32;
		private void Update()
		{
			if (!_shown)
				return;
			
			var position = (GameplayManager.Instance.GameTimer / 60f) * _sizePerMinute;
			_difficultyParents.anchoredPosition = new Vector2(-position, 0f);
		}

		public override void ShowView(bool force = false)
		{
			base.ShowView(force);
			//InitializeDifficulties();
		}

		public override void HideView(bool force = false)
		{
			base.HideView(force);
			//DeinitializeDifficulties();
		}

		private void DeinitializeDifficulties()
		{
			/*for (int i = 0; i < _difficultyParents.childCount; i++)
			{
				var child = _difficultyParents.GetChild(i);
				Destroy(child);
			}*/
		}

		private void InitializeDifficulties()
		{
			/*var difficulties = EnemyManager.Instance.CurrentEnemyLevels.Levels;
			var lastPosition = 0f;
			for (int i = 0; i < difficulties.Count; i++)
			{
				var difficulty = difficulties[i];
				var size = 200f;
				if (i < difficulties.Count - 1)
				{
					var nextDifficultyTime = difficulties[i + 1].Time;
					var timeDif = nextDifficultyTime - difficulty.Time;
					size = _sizePerMinute * (timeDif / 60f);
				}

				var difficultyUi = Instantiate(_enemyLevelUiPrefab, _difficultyParents);
				difficultyUi.Initialize(difficulty.BackgroundColor, difficulty.TextColor, difficulty.Caption, lastPosition, size);
				lastPosition = lastPosition + size;
			}*/
		}


	}
}