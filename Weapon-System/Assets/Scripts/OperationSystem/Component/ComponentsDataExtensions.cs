using System;

namespace OperationSystem.Component
{
    public class ComponentFindException : Exception
    {
    }
    
    public static class ComponentsDataExtensions
    {
        public static T Get<T>(this ComponentsData componentsData)
            where T : IComponent
        {
            return componentsData.TryGet<T>() ?? throw new ComponentFindException();
        }
    }
}