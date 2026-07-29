using OperationSystem.Component;

namespace OperationSystem.Containers.Components.Containers
{
    public interface IContainerVolume : IComponent
    {
        int MaxIndividualItemVolume { get; }
    }
    
    public class ContainerVolume 
        : AbstractComponent
        , IContainerVolume
    {
        public int MaxIndividualItemVolume { get; }
        
        public ContainerVolume(int maxIndividualItemVolume) 
        {
            MaxIndividualItemVolume = maxIndividualItemVolume;
        }
    }
}