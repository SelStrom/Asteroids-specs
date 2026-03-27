using System;
using System.Collections.Generic;

namespace SelStrom.Asteroids
{
    public class ActionScheduler
    {
        private struct ScheduledAction
        {
            public float TriggerTime;
            public Action Callback;
        }

        private readonly List<ScheduledAction> _actions = new();
        private float _time;
        private float _nextTrigger = float.MaxValue;

        public void Update(float deltaTime)
        {
            _time += deltaTime;
            if (_time < _nextTrigger) { return; }

            _nextTrigger = float.MaxValue;
            for (var i = _actions.Count - 1; i >= 0; i--)
            {
                if (_actions[i].TriggerTime <= _time)
                {
                    var action = _actions[i].Callback;
                    _actions.RemoveAt(i);
                    action?.Invoke();
                }
                else
                {
                    _nextTrigger = Math.Min(_nextTrigger, _actions[i].TriggerTime);
                }
            }
        }

        public void Schedule(float delay, Action callback)
        {
            var triggerTime = _time + delay;
            _actions.Add(new ScheduledAction { TriggerTime = triggerTime, Callback = callback });
            _nextTrigger = Math.Min(_nextTrigger, triggerTime);
        }

        public void CleanUp()
        {
            _actions.Clear();
            _nextTrigger = float.MaxValue;
        }
    }
}
