using System;
using System.Collections.Generic;

namespace Superfluid.Engine
{
    public class StateMachine<TState> where TState : struct
    {
        private readonly Dictionary<TState, ActiveState> _states = new Dictionary<TState, ActiveState>();

        private ActiveState _state;
        private TState _current = default;
        private TState _goto;

        public TState State => _goto;

        public void Add(TState state, Action enter, Action<float> update, Action exit)
        {
            if (_states.ContainsKey(state)) { throw new InvalidOperationException("State already is defined"); }
            _states[state] = new ActiveState(enter, update, exit);
        }

        public void Goto(TState state)
        {
            if (!_states.ContainsKey(state))
            {
                throw new ArgumentException("Unknown target state");
            }

            // Assign target state
            _goto = state;
        }

        public void Update(float dt)
        {
            // Difference in state
            if (!Equals(_goto, _current) || _state == null)
            {
                // Invoke state exit
                _state?.Exit?.Invoke();

                // Change states
                _current = _goto;
                _state = _states[_current];

                // Invoke state enter
                _state?.Enter?.Invoke();
            }

            // Invoke state update
            _state.Update?.Invoke(dt);
        }

        private sealed class ActiveState
        {
            public readonly Action Enter, Exit;
            public readonly Action<float> Update;

            public ActiveState(Action enter, Action<float> update, Action exit)
            {
                Enter = enter;
                Update = update;
                Exit = exit;
            }
        }
    }
}
