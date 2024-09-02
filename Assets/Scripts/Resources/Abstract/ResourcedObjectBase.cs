using UnityEngine;
using System;

namespace UnityLibrary.Helpers
{
    [Serializable]
    public abstract class ResourcedObjectBase<T> : InstanceObjectBase<T> where T : UnityEngine.Object
    {
        private const string ResourcesDir = "Assets/Resources/";

        private bool isLoadedFromResources = false;

        private string ObjectResourcePathWithoutExtension
        {
            get
            {
                if (string.IsNullOrEmpty(assetPath))
                    return null;

                if (!assetPath.StartsWith(ResourcesDir))
                {
                    Debug.LogWarningFormat("{0} failed to get '{1}' assets due to it is not located in '{2}' folder", 
                        GetType().Name, assetPath, ResourcesDir);

                    return null;
                }

                string path = assetPath.Substring(ResourcesDir.Length, assetPath.Length - ResourcesDir.Length);

                return path.Substring(0, path.LastIndexOf("."));
            }
        }

        public override void Free()
        {
            if (AssetInstance != null && isLoadedFromResources)
            {
                Resources.UnloadAsset(AssetInstance);
                isLoadedFromResources = false;
            }

            base.Free();
        }

        protected override T LoadObjectOnRuntimePlatform()
        {
            if (string.IsNullOrEmpty(assetPath))
                return null;

            string path = ObjectResourcePathWithoutExtension;

            if (string.IsNullOrEmpty(path))
                return null;

            AssetInstance = Resources.Load<T>(ObjectResourcePathWithoutExtension);
            if (AssetInstance == null)
            {
                Debug.LogWarningFormat("{0} failed to translate object path {1} to a reference to object of {2} type",
                                 GetType().Name, assetPath, typeof(T).Name);
                return null;
            }

            isLoadedFromResources = true;

            return AssetInstance;
        }
    }
}