namespace OperationSystem.Component
{
    public interface IComponentAccess
    {
        bool IsDirty { get; }
        void SetDirty(bool dirty);

        T Access<T>() where T : IComponent;
    }
}