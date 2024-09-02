using UnityEngine;
using System.Collections;
using Zenject;
using ModestTree;

#if UNITY_EDITOR && !UNITY_WEBPLAYER
//using Moq;
#endif

namespace Zenject
{
    public static class ZenjectMoqExtensions
    {
        public static ScopeConcreteIdArgConditionCopyNonLazyBinder FromMock<TContract>(this FromBinderGeneric<TContract> binder)
            where TContract : class
        {
            return null;

        }

        public static ConditionCopyNonLazyBinder FromMock<TContract>(this FactoryFromBinder<TContract> binder)
            where TContract : class
        {
            return null;// binder.FromInstance(Mock.Of<TContract>());
        }
    }
}
