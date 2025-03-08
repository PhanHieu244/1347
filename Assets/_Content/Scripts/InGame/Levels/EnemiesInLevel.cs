using System;
using System.Linq;
using _Content.InGame.Enemies;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace _Content.InGame.Managers
{
	[Serializable]
	public class EnemiesInLevel
	{
		[Dropdown("GetEnemyNames")]
		public string EnemyName;
		public int Percent;
		
		public DropdownList<string> GetEnemyNames()
		{
#if UNITY_EDITOR
			var list = new DropdownList<string>();
			string[] guids = AssetDatabase.FindAssets($"t:Prefab", new[] { "Assets/_Content/Prefabs/Enemies" });
			foreach (string guid in guids)
			{
				string myObjectPath = AssetDatabase.GUIDToAssetPath(guid);
				var myObj = AssetDatabase.LoadAssetAtPath<GameObject>(myObjectPath);
				if (myObj != null)
				{
					var enemyController = myObj.GetComponent<SimpleEnemyController>();
					if (enemyController != null)
					{
						list.Add(enemyController.EnemyId, enemyController.EnemyId);
					}
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