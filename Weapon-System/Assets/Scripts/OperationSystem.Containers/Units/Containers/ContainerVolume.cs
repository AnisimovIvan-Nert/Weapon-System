using System.Linq;
using OperationSystem.Units;

namespace OperationSystem.Containers.Units.Containers
{
    public interface IContainerVolume : IUnit
    {
        int MaxIndividualItemVolume { get; }
    }
    
    public class ContainerVolume 
        : AbstractUnit
        , IContainerVolume
    {
        public int MaxIndividualItemVolume { get; }
        
        public ContainerVolume(int maxIndividualItemVolume) 
            : base(Enumerable.Empty<IUnit>())
        {
            MaxIndividualItemVolume = maxIndividualItemVolume;
        }
    }
}