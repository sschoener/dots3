using Unity.Entities;

namespace Match3Game
{
    public static class StateMachineHelper
    {
        public interface IStateTransition<TState>
        {
            TState State { get; }
        }

        public static TState TryFindTransition<TTransition, TState>(
            EntityQueryBuilder entities, TState from, TState[] to
            )
            where TTransition : struct, IComponentData, IStateTransition<TState>
            where TState : System.Enum
        {
            TState target = from;

            var c = System.Collections.Generic.EqualityComparer<TState>.Default;
#if UNITY_EDITOR
            int numTransitions = 0;
            entities.ForEach((ref TTransition transition) =>
            {
                bool match = false;
                for (int i = 0; i < to.Length; i++) {
                    if (c.Equals(transition.State, to[i])) {
                        target = to[i];
                        match = true;
                        break;
                    }
                }

                if (!match)
                {
                    UnityEngine.Debug.LogError($"Invalid transition from {from} to {transition.State}, expected {to}.");
                }
                numTransitions += 1;
            });
            if (numTransitions > 1)
            {
                UnityEngine.Debug.LogError($"Multiple transitions from {from}.");
            }
#else
            entities.ForEach((ref TTransition transition) =>
            {
                for (int i = 0; i < to.Length; i++) {
                    if (c.Equals(transition.State, to[i])) {
                        target = to[i];
                        break;
                    }
                }
            }
#endif
            return target;
        }

        public static TState TryFindTransition<TTransition, TState>(
            EntityQueryBuilder entities, TState from, TState to
            )
            where TTransition : struct, IComponentData, IStateTransition<TState>
            where TState : System.Enum
        {
            TState target = from;

            var c = System.Collections.Generic.EqualityComparer<TState>.Default;
#if UNITY_EDITOR
            int numTransitions = 0;
            entities.ForEach((ref TTransition transition) =>
            {
                if (c.Equals(transition.State, to))
                {
                    target = to;
                }
                else
                {

                    UnityEngine.Debug.LogError($"Invalid transition from {from} to {transition.State}, expected {to}.");
                }
                numTransitions += 1;
            });
            if (numTransitions > 1)
            {
                UnityEngine.Debug.LogError($"Multiple transitions from {from}.");
            }
#else
            entities.ForEach((ref TTransition transition) =>
            {
                if (c.Equals(transition.State, to))
                {
                    target = to;
                }
            }
#endif
            return target;
        }
    }
}
