using Behaviours.BT.Runtime;

namespace Behaviours.BT
{
    public class Invert : Composite
    {
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

                    if (state != State.Running)
                    {
                        return state == State.Failed ? State.Success : State.Failed;
                    }

                    return State.Running;
                }
            }

            return State.Failed;
        }        
    }
}