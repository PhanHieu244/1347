using System.Collections.Generic;
using System.Linq;
using MoreMountains.Tools;
using NaughtyAttributes;
using UnityEngine;

namespace _Content.InGame.Enemies
{
	public class EnemyRendererController : MonoBehaviour
	{
		[SerializeField] private bool _useUnscaledTime;
		[SerializeField] private int _targetMaterialID;
		
		[Space] [Header("Flickering")] [SerializeField]
		private string _blinkPropertyName;

		[SerializeField] private float _flickeringDuration = 0.1f;
		[SerializeField] private float _flickeringAmplitude = 1f;
		[SerializeField] private float _flickeringRemapZero = 0f;
		[SerializeField] private float _flickeringRemapOne = 1f;
		[SerializeField] private AnimationCurve _flickeringCurve;

		[SerializeField] private bool _findAllRenderersInChildren;

		[SerializeField] [HideIf("_findAllRenderersInChildren")]
		private Renderer _targetRenderer;

		private List<Renderer> _targetRenderers = new List<Renderer>();
		private bool _flickering;
		private float _flickeringStartTime;
		private float _remappedTimeSinceFlickeringStartTime;
		private float _flickeringInitialValue;
		private float _currentFlickeringValue;
		private float _currentFlickeringValueNormalized;
		private Dictionary<Renderer,MaterialPropertyBlock> _propertyBlocks;

		protected virtual void Awake()
		{
			Initialization();
		}

		private void Initialization()
		{
			if (_findAllRenderersInChildren)
				_targetRenderers.AddRange(GetComponentsInChildren<Renderer>());
			else
				_targetRenderers.Add(_targetRenderer);

			_propertyBlocks = new Dictionary<Renderer, MaterialPropertyBlock>();
			foreach (var targetRenderer in _targetRenderers)
			{
				var propertyBlock = new MaterialPropertyBlock();
				targetRenderer.GetPropertyBlock(propertyBlock, _targetMaterialID);
				_propertyBlocks.Add(targetRenderer, propertyBlock);
			}
		}

		protected virtual void OnEnable()
		{
			
		}

		protected virtual void OnDisable()
		{

		}

		protected virtual void Update()
		{
			UpdateValue();
		}

		public void Blink()
		{
			_flickeringStartTime = GetTime();
			_flickering = true;
			_flickeringInitialValue = GetBlinkInitialValue();
		}

		private float GetBlinkInitialValue()
		{
			if (_targetRenderers.Count == 0)
				return 0f;

			return _targetRenderers.FirstOrDefault()?.sharedMaterial.GetFloat(_blinkPropertyName) ?? 0f;
		}


		protected virtual void UpdateValue()
		{
			if (_flickering)
			{
				_remappedTimeSinceFlickeringStartTime = MMMaths.Remap(GetTime() - _flickeringStartTime, 0f, _flickeringDuration, 0f, 1f);
				_currentFlickeringValueNormalized = _flickeringCurve.Evaluate(_remappedTimeSinceFlickeringStartTime);
				_currentFlickeringValue = MMMaths.Remap(_currentFlickeringValueNormalized, 0f, 1f, _flickeringRemapZero, _flickeringRemapOne);
				_currentFlickeringValue *= _flickeringAmplitude;

				if (_flickering && (GetTime() - _flickeringStartTime > _flickeringDuration))
				{
					_flickering = false;
					_currentFlickeringValue = 0f;
				}

				SetBlinkValue(_currentFlickeringValue);
			}
		}


		protected float GetTime()
		{
			return _useUnscaledTime ? Time.unscaledTime : Time.time;
		}


		private void SetBlinkValue(float newValue)
		{
			if (_targetRenderers.Count == 0)
			{
				return;
			}
			
			foreach (var targetRenderer in _targetRenderers)
			{
				if (_propertyBlocks.TryGetValue(targetRenderer, out var propertyBlock))
				{
					targetRenderer.GetPropertyBlock(propertyBlock, _targetMaterialID);
					propertyBlock.SetFloat(_blinkPropertyName, newValue);
					targetRenderer.SetPropertyBlock(propertyBlock, _targetMaterialID);
				}
			}
		}
	}
}