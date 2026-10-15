using System;

namespace SecondScratch.Shared
{
    public delegate bool EventCondition();

    public sealed class ConditionalEvent
    {
        private readonly EventCondition _condition;
        private readonly bool _alwaysValidateCondition;

        private bool _pass;
        private Action? _handlers;

        private bool Pass => !_alwaysValidateCondition && _pass;

        public ConditionalEvent(EventCondition condition, bool alwaysValidateCondition = true)
        {
            _condition = condition;
            _alwaysValidateCondition = alwaysValidateCondition;
        }

        public bool CallOrSubscribe(Action action)
        {
            var pass = PassOrSubscribe(action);
            if (pass)
                action.Invoke();
            return pass;
        }
        
        public bool PassOrSubscribe(Action action)
        {
            if (Pass || _condition.Invoke())
                return true;

            lock (this)
            {
                if (Pass || _condition.Invoke())
                    return true;

                _handlers += action;
                return false;
            }
        }

        public void Unsubscribe(Action action)
        {
            lock (this)
                _handlers -= action;
        }

        public void Invoke()
        {
            Action? handlers;
            lock (this)
            {
                handlers = _handlers;
                _pass = true;
            }

            handlers?.Invoke();
        }

        public void Reset()
        {
            lock (this)
                _pass = false;
        }
    }
}