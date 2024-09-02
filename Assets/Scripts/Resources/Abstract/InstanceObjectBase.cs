using UnityEngine;
using System;

namespace UnityLibrary.Helpers
{
    [Serializable]
    public abstract class InstanceObjectBase<T> where T : UnityEngine.Object
    {
        [SerializeField]//, ReadOnly]
        private string assetGUID = null;

        [SerializeField]
        protected string assetPath = null;

        private T assetInstance = null;

#if UNITY_EDITOR
        private string assetInstanceGUID = null;
        private string assetInstancePath = null;
#endif

        protected T AssetInstance
        {
            get
            {
#if UNITY_EDITOR
                if (assetInstance != null)
                {
                    if (!string.Equals(GUID, assetInstanceGUID) || !string.Equals(Path, assetInstancePath))
                    {
                        AssetInstance = null;
                    }
                }
                
#endif
                return assetInstance;
            }
            set
            {
                assetInstance = value;

#if UNITY_EDITOR
                assetInstanceGUID = assetInstance != null ? assetGUID : null;
                assetInstancePath = assetInstance != null ? assetPath : null;
#endif
            }
        }

        public string GUID { get { return assetGUID; } }

        public string Path { get { return assetPath; } }

        public T Value
        {
            get
            {
                if (AssetInstance != null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying && !string.Equals(assetPath, GetAssetPath(AssetInstance)))
                        UpdateLinks();
#endif

                    return AssetInstance;
                }

#if UNITY_EDITOR
                
                return LoadObjectInEditorMode();

#else

                return LoadObjectOnRuntimePlatform();

#endif

            }

#if UNITY_EDITOR

            set
            {
                if (AssetInstance != value)
                {
                    if (value == null)
                        Free();
                    
                    AssetInstance = value;

#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        UpdateLinks();

#endif
                }
            }
#endif
        }

        public virtual void Free()
        {
            //assetInstance = null;
        }

        protected virtual T LoadObjectOnRuntimePlatform()
        {
            return AssetInstance;
        }

#if UNITY_EDITOR

        public void Reset()
        {
            if (AssetInstance != null)
            {
                Value = null;
            }
            else
            {
                if (!Application.isPlaying)
                    UpdateLinks();
            }
        }

        protected virtual T LoadObjectInEditorMode()
        {
            if (string.IsNullOrEmpty(assetGUID))
                return null;

            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(assetGUID);
            if (string.IsNullOrEmpty(path))
            {
				Debug.LogWarningFormat("{0} failed to get the path name where the asset with '{1}' GUID of {2} type is stored (path is '{3}')",
				                       GetType().Name, assetGUID, typeof(T).Name, assetPath);
                return null;
            }

            AssetInstance = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            if (AssetInstance == null)
            {
                Debug.LogWarningFormat("{0} failed to get asset object of {1} type at given path '{2}' (path is '{3}')",
                                 GetType().Name, typeof(T).Name, path, assetPath);
                return null;
            }

            if (!Application.isPlaying)
                UpdateLinks();

            return AssetInstance;
        }
        
        private void UpdateAssetPath()
        {
            if (AssetInstance != null)
                assetPath = GetAssetPath(AssetInstance);
            else
                assetPath = null;

#if UNITY_EDITOR
            assetInstancePath = assetInstance != null ? assetPath : null;
#endif
        }

        private void UpdateAssetGUID()
        {
            if (AssetInstance != null && !string.IsNullOrEmpty(assetPath))
                assetGUID = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
            else
                assetGUID = null;

#if UNITY_EDITOR
            assetInstanceGUID = assetInstance != null ? assetGUID : null;
#endif
        }

        protected virtual void UpdateLinks()
        {
            UpdateAssetPath();
            UpdateAssetGUID();
        }

        private string GetAssetPath(T asset)
        {
            if (asset == null)
                return null;

            var path = UnityEditor.AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarningFormat("{0} failed to get the path name where the '{1}' asset of {2} type is stored (path is '{3}')",
				                       GetType().Name, asset.name, typeof(T).Name, assetPath);
                return null;
            }

            return path;
        }
#endif

    }
}