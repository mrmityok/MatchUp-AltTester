using System.Collections.Generic;
using UnityLibrary.Helpers;

namespace MatchUp.Data
{
    public interface IResourcedSpriteInfo
    {
        int Id { get; }
        ResourcedSprite Sprite { get; }
    }
    
    public interface IResourcedData
    {
        IEnumerable<IResourcedSpriteInfo> CardSprites { get; }
    }
}