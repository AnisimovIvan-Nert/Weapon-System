using System;

namespace SecondScratch.Shared
{
    public delegate bool EventCondition();

    public sealed class ConditionalEvent
    {
        private readonly EventCondition _condition;

        private bool _pass;
        private Action? _handlers;

        public ConditionalEvent(EventCondition condition)
        {
            _condition = condition;
        }

        public bool CallOrSubscribe(Action action)
        {
            if (_pass || _condition.Invoke())
            {
                action.Invoke();
                return true;
            }

            lock (this)
            {
                if (_pass || _condition.Invoke())
                {
                    action.Invoke();
                    return true;
                }

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