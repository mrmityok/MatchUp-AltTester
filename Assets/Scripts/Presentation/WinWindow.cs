using System;
using TMPro;
using UnityEngine;

namespace Presentation
{
    public struct WinWindowArgs
    {
        public WinWindowArgs(float time, int tryCount, bool isBestResult, Action onTryAgain, Action onClose)
        {
            Time = time;
            TryCount = tryCount;
            IsBestResult = isBestResult;
            OnTryAgain = onTryAgain;
            OnClose = onClose;
        }
        
        public float Time { get; }
        public int TryCount { get; }
        public bool IsBestResult { get; }
        public Action OnTryAgain { get; }
        public Action OnClose { get; }
    }
    
    public class WinWindow : Window<WinWindowArgs>
    {
        [SerializeField] private TextMeshProUGUI resultsText = null;
        [SerializeField] private GameObject bestResult = null;
        [SerializeField] private string resultsTemplate = "Tries: <b>{0}</b> Time: <b>{1:0.00}</b>";

        private Action _onTryAgain;
        private Action _onClose;

        public override bool IsModal => true;

        protected override void OnShow(WinWindowArgs args)
        {
            _onTryAgain = args.OnTryAgain;
            _onClose = args.OnClose;

            ShowResults(args.Time, args.TryCount, args.IsBestResult);
        }

        public void TryAgain()
        {
            _onTryAgain?.Invoke();
            Hide();
        }

        public void Close()
        {
            _onClose?.Invoke();
            Hide();
        }
        
        private void ShowResults(float time, int tryCount, bool isBestResult)
        {
            resultsText.text = string.Format(resultsTemplate, tryCount, time);
            bestResult.SetActive(isBestResult);
        }

        protected override void OnHide()
        {
            _onTryAgain = null;
            _onClose = null;
        }
    }
}