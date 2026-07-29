using System;

namespace OperationSystem.Component
{
    public class ComponentFindException : Exception
    {
    }
    
    public static class ComponentsDataExtensions
    {
        public static ComponentsData.ComponentResource Get<T>(this ComponentsData componentsData)
            where T : IComponent
        {
            return componentsData.TryGet<T>() ?? throw new ComponentFindException();
        }
        
        public static T Read<T>(this ComponentsData componentsData)
            where T : IComponent
        {
            return componentsData.TryRead<T>() ?? throw new ComponentFindException();
        }
    }
}