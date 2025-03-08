using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace _Content.InGame.Levels
{
	[CreateAssetMenu(menuName = "Levels List")]
	public class LevelsList: ScriptableObject
	{
		[SerializeField] [Expandable] private List<LevelInfo> _levels;

		public List<LevelInfo> Levels => _levels;
	}
}