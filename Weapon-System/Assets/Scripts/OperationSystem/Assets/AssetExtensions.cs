using System;
using System.Linq;

namespace OperationSystem.Assets
{
    public static class AssetExtensions
    {
        public static T? TryGetChild<T>(this IAsset asset)
            where T : IAsset
        {
            return asset.Children.OfType<T>().SingleOrDefault();
        }
        
        public static T GetChild<T>(this IAsset asset)
            where T : IAsset
        {
            return asset.TryGetChild<T>() ?? throw new InvalidOperationException();
        }
    }
}