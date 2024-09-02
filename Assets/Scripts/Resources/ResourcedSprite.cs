using System;
using UnityEngine;

namespace UnityLibrary.Helpers
{
    [Serializable]
    public class ResourcedSprite : ResourcedObjectBase<Sprite>
    {
#if UNITY_EDITOR

        [UnityEditor.CustomPropertyDrawer(typeof(ResourcedSprite))]
        public class ResourcedTextureDrawer : ResourcedObjectDrawer<ResourcedSprite, Sprite>
        {

        }

#endif
    }
}