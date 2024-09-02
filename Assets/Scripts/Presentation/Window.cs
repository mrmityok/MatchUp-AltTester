using System;
using UnityEngine;

namespace Presentation
{
    public interface IWindow
    {
        event Action<IWindow> Showing;
        
        void Show();
        void Hide();

        bool IsVisible { get; }
        bool IsModal { get; }
    }

    public abstract class Window : MonoBehaviour, IWindow
    {
        public event Action<IWindow> Showing;

        public virtual void Show()
        {
            RaiseShowing();

            IsVisible = true;
            
            OnShow();
        }

        public virtual void Hide()
        {
            OnHide();
            
            IsVisible = false;
        }

        public bool IsVisible
        {
            get => gameObject.activeSelf;
            protected set => gameObject.SetActive(value);
        }

        public virtual bool IsModal => false;

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }

        protected void RaiseShowing()
        {
            Showing?.Invoke(this);
        }
    }

    public abstract class Window<T> : Window
    {
        public void Show(T data)
        {
            RaiseShowing();

            IsVisible = true;

            OnShow(data);   
        }

        protected abstract void OnShow(T data);
    }
    
    public abstract class Window<T1, T2> : Window
    {
        public void Show(T1 arg1, T2 arg2)
        {
            RaiseShowing();

            IsVisible = true;

            OnShow(arg1, arg2);   
        }

        protected abstract void OnShow(T1 arg1, T2 arg2);
    }
    
    public abstract class Window<T1, T2, T3> : Window
    {
        public void Show(T1 arg1, T2 arg2, T3 arg3)
        {
            RaiseShowing();
            
            IsVisible = true;

            OnShow(arg1, arg2, arg3);   
        }

        protected abstract void OnShow(T1 arg1, T2 arg2, T3 arg3);
    }
}