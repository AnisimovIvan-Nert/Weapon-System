using OperationSystem.Assets;

namespace OperationSystem.Containers.Tests.Mocks
{
    public class ContainerAsset : AbstractAsset
    {
        public void Set(params IAsset[] children)
        {
            foreach (var child in children)
                TryAddChild(child);
        }
    }
}