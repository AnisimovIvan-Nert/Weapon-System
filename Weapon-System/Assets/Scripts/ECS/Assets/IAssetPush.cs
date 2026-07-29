namespace ECS
{
    public interface IAssetPush<T> where T : IComponent
    {
        void PushFrom(in T component);
    }
}
