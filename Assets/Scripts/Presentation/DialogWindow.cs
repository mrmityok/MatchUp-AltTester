using System;

namespace Presentation
{
    public class DialogWindow : Window<Action, Action>
    {
        private Action _onConfirm;
        private Action _onCancel;

        public override bool IsModal => true;

        protected override void OnShow(Action onConfirm, Action onCancel)
        {
            _onConfirm = onConfirm;
            _onCancel = onCancel;
        }

        public void Confirm()
        {
            _onConfirm?.Invoke();
            Hide();
        }

        public void Cancel()
        {
            _onCancel?.Invoke();
            Hide();
        }
    }
}