using System.Collections;
using System.Collections.Generic;
using Base;
using Extensions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace _Content.InGame.Managers
{
	public class DynamicResolutionScaling: Singleton<DynamicResolutionScaling>
	{
		[SerializeField] private float _minScale1 = 0.6f;
		[SerializeField] private float _minScale2 = 0.6f;
		[SerializeField] private int _fpsToChangeScale = 25;
		public int Granularity = 15; // how many frames to wait until you re-calculate the FPS
		private List<double> _times;
		int _counter = 5;
		private double _averageFps;
		private float _currentScaleFactor = 1f;
		private float _lastTimeToCheck;
		private List<double> _fpsSinceLastRecalculate;
		private bool _firstTime;

		public void Start()
		{
			_fpsSinceLastRecalculate = new List<double>();
			_times = new List<double>();
			_currentScaleFactor = 1f;
			_firstTime = true;
			Invoke(nameof(RecalculateScaleFactorBasedOnCurrentFps), 2f);
		}

		private void CalcFPS()
		{
			double sum = 0;
			foreach (double f in _times)
			{
				sum += f;
			}

			double average = _times.Median();
			_averageFps = 1f / average;
			_times.Clear();
		}


		void Update()
		{
			if (_counter <= 0)
			{
				CalcFPS();
				_fpsSinceLastRecalculate.Add(_averageFps);
				if (_fpsSinceLastRecalculate.Count > 100)
					_fpsSinceLastRecalculate.RemoveAt(0);
				_counter = Granularity;
			}

			_times.Add(Time.unscaledDeltaTime);
			_counter--;
		}

		public void RecalculateScaleFactorBasedOnCurrentFps()
		{
			var averageFps = CalcAverageFpsSinceLastRecalc();
			_fpsSinceLastRecalculate.Clear();
			if (averageFps < _fpsToChangeScale)
			{
				if (_currentScaleFactor > _minScale1)
				{
					_currentScaleFactor = _minScale1;
					OnScaleFactorChanged();
				}
				else if (_currentScaleFactor > _minScale2)
				{
					_currentScaleFactor = _minScale2;
					OnScaleFactorChanged();
				}
			}

			_lastTimeToCheck = Time.unscaledTime;
			Invoke(nameof(RecalculateScaleFactorBasedOnCurrentFps),
				_firstTime ? 5f : 20f);
			_firstTime = false;
		}

		private double CalcAverageFpsSinceLastRecalc()
		{
			var fps = _averageFps;
			if (_fpsSinceLastRecalculate.Count > 1)
			{
				fps = _fpsSinceLastRecalculate
					.Median(); //Aggregate((x1, x2) => x1 + x2) / (double) _fpsSinceLastRecalculate.Count;
			}

			return fps;
		}

		private void OnScaleFactorChanged()
		{
			var pipelineAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
			if (pipelineAsset != null)
			{
				pipelineAsset.renderScale = _currentScaleFactor;
			}
		}

		public void RecalcRenderScaleAfterTime(float time)
		{
			StartCoroutine(RecalcAferTime(time));
		}

		private IEnumerator RecalcAferTime(float time)
		{
			yield return new WaitForSeconds(time);
			RecalculateScaleFactorBasedOnCurrentFps();
		}
	}
}