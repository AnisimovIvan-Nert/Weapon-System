using SecondScratch.ThreadSafe.Commands;

namespace SecondScratch.ThreadSafe.Tests.Mocks.Commands
{
    public class DependencyCommand : AbstractCommand<DependencyObject>
    {
        public DependencyCommand(DependencyObject target, ICommand? dependency = null) 
            : base(target, dependency)
        {
        }

        public override bool CanExecute()
        {
            return base.CanExecute() && TypedTarget.CanExecute;
        }

        protected override void InnerExecute()
        {
        }
    }

    public class DependencyObject
    {
        public bool CanExecute;
    }
}