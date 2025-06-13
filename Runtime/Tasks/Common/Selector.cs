using Behaviours.BT.Runtime;

namespace Behaviours.BT
{
    public class Selector : Composite
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
                else if (state == State.Failed)
                {
                    int newIndex = comp.currentIndex + 1;

                    if (newIndex >= comp.runtimeChildNodes.Count)
                    {
                        return State.Failed;
                    }
                    else
                    {
                        changeNode(comp, newIndex);

                        return State.Running;
                    }
                }
                else
                {
                    return State.Success;
                }
            }

            return State.Failed;
        }
    }
}