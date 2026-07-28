using System;

namespace OperationSystem.Component
{
    public class ComponentDataFindException : Exception
    {
    }
    
    public static class ComponentsDataExtensions
    {
        public static T ReadComponent<T>(this ComponentsData componentsData)
            where T : IComponent
        {
            return componentsData.TryReadComponent<T>() ?? throw new ComponentDataFindException();
        }

        public static ComponentsData.ComponentAccess<T> AccessComponent<T>(this ComponentsData componentsData)
            where T : IComponent
        {
            return componentsData.TryAccessComponent<T>() ?? throw new ComponentDataFindException();
        }
        
        public static ComponentsData.ComponentResource GetResource<T>(this ComponentsData componentsData)
            where T : IComponent
        {
            return componentsData.TryGetResource<T>() ?? throw new ComponentDataFindException();
        }
    }
}