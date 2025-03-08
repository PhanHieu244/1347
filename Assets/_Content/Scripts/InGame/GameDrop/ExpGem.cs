using System;
using _Content.Events;
using _Content.InGame.Managers;
using _Content.InGame.TweenAnimation;
using DG.Tweening;
using UnityEngine;
using MoreMountains.Feedbacks;
using EventHandler = Opsive.Shared.Events.EventHandler;

namespace _Content.InGame.GameDrop
{
	public class ExpGem : MonoBehaviour
	{
		[SerializeField] private DropAnimation _dropAnimation;
		[SerializeField] private CollectAnimation _collectAnimation;
		[SerializeField] protected MMF_Player _onCollectFeedback;

		private bool _collected;
		private int _currentExp;
		private Transform _collectionParent;
		private float _collectionTimer;
		private Vector3 _startCollectionPosition;

		public bool Collected => _collected;

		private void Awake()
		{
			_onCollectFeedback?.Initialization(gameObject);
		}

		private void OnEnable()
		{
			EventHandler.RegisterEvent<float>(InGameEvents.OnTimeScaleChanged, OnTimeScaleChanged);
		}

		private void OnDisable()
		{
			EventHandler.UnregisterEvent<float>(InGameEvents.OnTimeScaleChanged, OnTimeScaleChanged);
		}

		private void OnDestroy()
		{
			transform.DOKill();
		}

		private void Update()
		{
			if (_collected)
			{
				_collectionTimer += Time.deltaTime * GameManager.Instance.TimeScale;
				var t = _collectionTimer / _collectAnimation._timeXZ;
				var realT = _collectAnimation._xzCurve.Evaluate(t);
				var neededPos = Vector3.Lerp(_startCollectionPosition, _collectionParent.position, realT);
				var pos = transform.position;
				pos.x = neededPos.x;
				pos.z = neededPos.z;
				transform.position = pos;
				if (t > 1f)
					OnCollected();
			}
		}

		private void OnTimeScaleChanged(float timeScale)
		{
			/*var ts = timeScale < 0.99f ? 0f : 1f;
			var activeTweens = DOTween.TweensByTarget(transform);
			if (activeTweens != null && activeTweens.Count > 0)
				activeTweens.ForEach(t => t.timeScale = ts);*/
		}

		public void Drop(Vector3 destination, int exp)
		{
			transform.DOKill();
			_dropAnimation.DoAnimation(transform, destination, 6f, () => { });
			_currentExp = exp;
		}

		public void Collect(Transform parent, bool slow = false)
		{
			transform.DOKill();
			_collectAnimation.DoAnimation(transform, parent, OnCollected, slow);
			_startCollectionPosition = transform.position;
			_collectionParent = parent;
			_collectionTimer = 0f;
			_collected = true;
		}

		protected virtual void OnCollected()
		{
			_onCollectFeedback?.PlayFeedbacks();
			EnemyManager.Instance.OnExpCollected(_currentExp);
			Destroy(gameObject);
		}
	}
}