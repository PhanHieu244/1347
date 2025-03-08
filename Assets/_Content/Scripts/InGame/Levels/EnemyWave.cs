using System;
using System.Collections.Generic;
using _Content.InGame.Managers;
using NaughtyAttributes;
using UnityEngine;

namespace _Content.InGame.Levels
{
	[CreateAssetMenu(menuName = "Wave")]
	public class EnemyWave : ScriptableObject
	{
		[Serializable]
		public class ConstantEnemiesOnLevel
		{
			public float Time;
			public int MaxEnemiesCountOnLevel = 10;
			public List<EnemiesInLevel> EnemiesInLevels;
		}

		public bool IsBossWave;
		public string WaveCaption;
		public float TimeFromPrevWaveToStart;
		public float Duration;
		public List<ConstantEnemiesOnLevel> EnemiesOnLevel;
		public List<ConstantEnemiesOnLevel> EnemiesBurst;
	}
}