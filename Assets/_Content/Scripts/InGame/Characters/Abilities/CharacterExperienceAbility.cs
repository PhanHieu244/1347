using _Content.InGame.GameDrop;
using _Content.InGame.Managers;
using UnityEngine;

namespace _Content.InGame.Characters.Abilities
{
	public class CharacterExperienceAbility: CharacterAbility
	{
		[SerializeField] private float _checkTime = 0.1f;
		[SerializeField] private float _expCollectRadius = 10f;
		[SerializeField] private LayerMask _expLayerMask;
		private Collider[] _checkResult = new Collider[30];
		private float _checkTimer;
		public float ExperienceRatio { get; set; } = 1f;

		public override void ProcessAbility()
		{
			base.ProcessAbility();
			var deltaTime = Time.deltaTime * GameManager.Instance.TimeScale;
			if (_checkTimer <= 0f)
			{
				CheckExperience();
				_checkTimer = _checkTime;
			}
			else
			{
				_checkTimer -= deltaTime;
			}
		}

		private void CheckExperience()
		{
			var count = Physics.OverlapSphereNonAlloc(transform.position, _expCollectRadius, _checkResult,
				_expLayerMask);
			if (count > 0)
			{
				for (int i = 0; i < count; i++)
				{
					var col = _checkResult[i];
					var expGem = col.GetComponent<ExpGem>();
					if (expGem != null)
					{
						if (!expGem.Collected)
							expGem.Collect(_character.CollectionPoint);
					}
				}
			}
		}

		public int ProcessExperience(int currentExp)
		{
			return Mathf.CeilToInt(currentExp * ExperienceRatio);
		}
	}
}