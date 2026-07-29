namespace ECS
{
    public interface IAssetPull<T> where T : IComponent
    {
        void PullInto(ref T component);
    }
}
