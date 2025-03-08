using System;
using System.Collections.Generic;
using System.Linq;
using _Content.InGame.Characters.Talents;
using _Content.InGame.Enemies;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace _Content.InGame.Levels
{
	[CreateAssetMenu(menuName = "Level Info")]
	public class LevelInfo: ScriptableObject
	{
		[SerializeField] private string _levelName;
		[SerializeField] private int _levelNumber;
		[SerializeField] private string _levelCaption;
		[SerializeField] private Sprite _levelIcon;
		[SerializeField] private string _sceneName;
		[SerializeField] private int _progressPerWin;
		[SerializeField] private int _progressMaxAmount;
		[SerializeField] private List<EnemyWave> _enemyWave;
		[SerializeField] private List<UnlockAbility> _unlockAbilities;

		public List<UnlockAbility> UnlockAbilities => _unlockAbilities;
		public Sprite LevelIcon => _levelIcon;
		public string LevelCaption => _levelCaption;
		public string LevelName => _levelName;
		public int LevelNumber => _levelNumber;
		public string SceneName => _sceneName;
		public List<EnemyWave> EnemyWave => _enemyWave;
		public int ProgressMaxAmount => _progressMaxAmount;
		public int ProgressPerWin => _progressPerWin;
	}

	[Serializable]
	public class UnlockAbility
	{
		[Dropdown("GetEnemyNames")]
		public string AbilityName;
		public int ProgressToUnlock;
		
		public DropdownList<string> GetEnemyNames()
		{
#if UNITY_EDITOR
			var list = new DropdownList<string>();
			string[] guids = AssetDatabase.FindAssets($"t:TalentBase", new[] { "Assets/_Content/Skills" });
			foreach (string guid in guids)
			{
				string myObjectPath = AssetDatabase.GUIDToAssetPath(guid);
				var myObj = AssetDatabase.LoadAssetAtPath<TalentBase>(myObjectPath);
				if (myObj != null)
				{
					list.Add(myObj.AbilityName, myObj.AbilityName);
				}
			}

			if (!list.Any())
			{
				list.Add("Empty", null);
			}
			return list;
#else
			return new DropdownList<string>();
#endif
		}
	}
}