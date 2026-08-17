using OperationSystem.Assets;
using OperationSystem.Component.Types;
using OperationSystem.Containers.Components;
using OperationSystem.Units;

namespace OperationSystem.Containers.Tests.Mocks
{
    public class FooAsset 
        : AbstractAsset
        , IAssetPull<Size>
    {
        private Size? _size;

        public FooAsset(Size? size = null)
        {
            _size = size;
        }
        
        public void Set(Size? size = null)
        {
            _size = size;
        }

        public override ComponentMask GetComponentMask()
        {
            var mask = base.GetComponentMask();
            if (_size.HasValue)
                mask.Add<Size>();

            return mask;
        }
        
        public void PullInto(ref Size component, UnitWorld world)
        {
            if (_size.HasValue)
                component = _size.Value;
        }
    }
}