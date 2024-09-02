using System;
using System.Collections.Generic;
using UnityEngine;
using UnityLibrary.Helpers;

namespace MatchUp.Data
{
    [CreateAssetMenu(fileName = "ResourcedData", menuName = "Resourced Data", order = 1052)]
    public class ResourcedData : ScriptableObject, IResourcedData
    {
        [Serializable]
        public class ResourcedSpriteInfo : IResourcedSpriteInfo
        {
            public int Id => id;
            public ResourcedSprite Sprite => sprite;

            public int id;
            public ResourcedSprite sprite;
        }

        public IEnumerable<IResourcedSpriteInfo> CardSprites => cardSprites;

        public List<ResourcedSpriteInfo> cardSprites = new List<ResourcedSpriteInfo>();
    }
}