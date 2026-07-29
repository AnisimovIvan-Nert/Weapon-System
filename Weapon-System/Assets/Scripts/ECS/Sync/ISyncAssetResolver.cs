namespace ECS
{
    public interface ISyncAssetResolver
    {
        IAsset GetAsset(int assetHandle);
        int GetAssetTypeId(int assetHandle);
    }
}
