using Behaviours.BT.Runtime;

namespace Behaviours.BT
{
    public class Convert : Composite
    {
        public State fromState;
        public State toState;

        public override State update(RuntimeTaskData runtimeTask, float dt)
        {
            CompositeTaskData comp = runtimeTask as CompositeTaskData;
            {
                if (comp.currentIndex != -1)
                {
                    var currentNode = comp.runtimeChildNodes[comp.currentIndex];
                    State state = currentNode.update(dt);
                    
                    #if UNITY_EDITOR
                    currentNode._editorReturnedState = state;
                    #endif

                    if (state == fromState)
                    {
                        return toState;
                    }

                    return state;
                }
            }

            return State.Failed;
        }
    }
}