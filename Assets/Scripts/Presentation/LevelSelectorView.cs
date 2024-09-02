using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Presentation
{
    public interface ILevelSelectorView
    {
        event Action<int> Selected;
        
        void Show(int levelId);
        void Hide();
    }
    
    public interface ILevelSelectorViewModifier
    {
        void SetLevelName(string name);
        void SetBestResult(float? time, int? tryCount);
        void RiseSelected(int levelId);
    }
    
    public class LevelSelectorView : MonoBehaviour, ILevelSelectorView
    {
        public event Action<int> Selected;
        
        [SerializeField] private Button button = null;
        [SerializeField] private TextMeshProUGUI nameText = null;
        [SerializeField] private TextMeshProUGUI bestResultText = null;
        [SerializeField] private string bestResultTemplate = "Tries: <b>{0}</b> Time: <b>{1:0.00}</b>";

        private ILevelSelectorViewPresenter _presenter;

        private void Start()
        {
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            _presenter.Select();
        }

        [Inject]
        private void Init(ILevelSelectorViewPresenter presenter)
        {
            _presenter = presenter;
            _presenter.Init(new Modifier(this));
        }

        public void Show(int levelId)
        {
            gameObject.SetActive(true);
            
            _presenter.SetLevelId(levelId);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        private class Modifier : ILevelSelectorViewModifier
        {
            private readonly LevelSelectorView _view;

            public Modifier(LevelSelectorView view)
            {
                _view = view;
            }

            public void SetLevelName(string name)
            {
                _view.nameText.text = name;
            }

            public void SetBestResult(float? time, int? tryCount)
            {
                _view.bestResultText.gameObject.SetActive(time.HasValue || tryCount.HasValue);
                _view.bestResultText.text = string.Format(_view.bestResultTemplate, tryCount, time);
            }

            public void RiseSelected(int levelId)
            {
                _view.Selected?.Invoke(levelId);
            }
        }
    }
}