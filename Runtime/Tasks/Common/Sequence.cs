using Behaviours.BT.Runtime;

namespace Behaviours.BT
{
    public class Sequence : Composite
    {
        public override State update(RuntimeTaskData runtimeTask, float dt)
        {
            CompositeTaskData comp = runtimeTask as CompositeTaskData;

            if (comp.currentIndex != -1)
            {
                var currentNode = comp.runtimeChildNodes[comp.currentIndex];
                var state = currentNode.update(dt);

                #if UNITY_EDITOR
                currentNode._editorReturnedState = state;
                #endif

                if (state == State.Running)
                {
                    return State.Running;
                }
                else if (state == State.Success)
                {
                    int newIndex = comp.currentIndex + 1;

                    if (newIndex >= comp.runtimeChildNodes.Count)
                    {
                        return State.Success;
                    }
                    else
                    {
                        changeNode(comp, newIndex);
                        return State.Running;
                    }
                }
            }

            return State.Failed;
        }
    }    
}