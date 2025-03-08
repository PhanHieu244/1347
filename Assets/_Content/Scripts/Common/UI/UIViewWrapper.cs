using System.Collections;
using Doozy.Engine.UI;
using UnityEngine;

namespace Common.UI
{
	[RequireComponent(typeof(UIView))]
	public class UIViewWrapper : MonoBehaviour
	{
		protected UIView _uiView;
		protected bool _shown;
		protected Canvas _canvas;

		private void Awake()
		{
			OnAwake();
		}

		protected virtual void OnAwake()
		{
			GetUIView();
			_canvas = GetComponentInParent<Canvas>();
			if (_canvas != null)
			{
				/*_canvas.overrideSorting = true;
				_canvas.sortingOrder = 1;
				StartCoroutine(DisableSortingOrder());*/
				//_canvas.sortingOrder = 0;
			}
		}

		private IEnumerator DisableSortingOrder()
		{
			yield return null;
			_canvas.overrideSorting = false;
			_canvas.sortingOrder = 0;
		}

		private UIView GetUIView()
		{
			if (_uiView == null)
				_uiView = GetComponent<UIView>();

			return _uiView;
		}

		public virtual void ShowView(bool force = false)
		{
			if (_shown)
				return;
			GetUIView().Show(force);
			_shown = true;
		}

		public virtual void HideView(bool force = false)
		{
			if (!_shown)
				return;
			GetUIView().Hide(force);
			_shown = false;
		}

		public virtual bool CanShow()
		{
			return true;
		}

		public virtual bool IsShown => _shown;
	}
}