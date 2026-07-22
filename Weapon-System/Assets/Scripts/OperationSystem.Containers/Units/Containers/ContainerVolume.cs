using System.Collections.Generic;
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
        
        public ContainerVolume(int maxIndividualItemVolume, IEnumerable<IUnit> children) 
            : base(children)
        {
            MaxIndividualItemVolume = maxIndividualItemVolume;
        }
    }
}