using System;
using System.Collections.Generic;
using System.Linq;
using OperationSystem.Component;
using OperationSystem.Units;

namespace OperationSystem.Assets
{
    public abstract class AbstractAsset : IAsset
    {
        protected List<IAsset> ChildrenList = new();

        public virtual IEnumerable<IAsset> Children => ChildrenList;
        
        public virtual void AddChild(IAsset child) => ChildrenList.Add(child);
        public virtual void RemoveChild(IAsset child) => ChildrenList.Remove(child);

        public virtual IEnumerable<IComponentHandle> EnumerateComponents(UnitWorld unitWorld)
        {
            var children = Children.Select(unitWorld.CreateUnit).Select(o => o.Id);
            var childrenComponent = new ChildrenComponent(children.ToArray());
            yield return new ComponentHandle<ChildrenComponent>(childrenComponent, this);
        }

        public virtual T PullData<T>(T component, UnitWorld unitWorld) where T : IComponent
        {
            switch (component)
            {
                case ChildrenComponent childrenComponent:
                    unitWorld.RemoveUnits(childrenComponent.Children.ToArray());
                    childrenComponent.Children.Clear();
                    foreach (var child in Children.Select(unitWorld.CreateUnit).Select(o => o.Id))
                        childrenComponent.Children.Add(child);
                    return component;
            }

            throw new InvalidOperationException();
        }

        public virtual T PushData<T>(T component, UnitWorld unitWorld) where T : IComponent
        {
            switch (component)
            {
                case ChildrenComponent childrenComponent:
                    var children = childrenComponent.Children.Select(unitWorld.GetAsset).ToList();

                    var extraAssets = Children.Where(o => !children.Contains(o));
                    foreach (var extra in extraAssets)
                        RemoveChild(extra);

                    var missingAssets = children.Where(o => !Children.Contains(o));
                    foreach (var missing in missingAssets)
                        AddChild(missing);

                    return component;
            }

            throw new InvalidOperationException();
        }

        public bool IsDirty<T>(T component, UnitWorld unitWorld) where T : IComponent
        {
            switch (component)
            {
                case ChildrenComponent childrenComponent:
                    var children = childrenComponent.Children.Select(unitWorld.GetAsset).ToList();
                    return Children.Any(o => !children.Contains(o)) 
                           || children.Any(o => !Children.Contains(o));
            }

            throw new InvalidOperationException();
        }
    }
}