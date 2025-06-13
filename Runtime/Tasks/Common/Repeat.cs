using Behaviours.BT.Runtime;

namespace Behaviours.BT
{
    public class Repeat : Composite
    {
        public StopCondition until;

        public override State update(RuntimeTaskData runtimeTask, float dt)
        {
            CompositeTaskData comp = runtimeTask as CompositeTaskData;

            if (comp.currentIndex != -1)
            {
                var currentNode = comp.runtimeChildNodes[comp.currentIndex];
                State state = currentNode.update(dt);
                
                #if UNITY_EDITOR
                currentNode._editorReturnedState = state;
                #endif

                if (state != State.Running)
                {
                    switch (until)
                    {
                        case StopCondition.Any:
                            {
                                return state;
                            }

                        case StopCondition.Success:
                            {
                                if (state == State.Success)
                                {
                                    return state;
                                }                                
                            }
                            break;

                        case StopCondition.Fail:
                            {
                                if (state == State.Failed)
                                {
                                    return state;
                                }                                
                            }
                            break;
                    }

                    currentNode.end();
                    currentNode.start();
                }

                return State.Running;
            }

            return State.Failed;
        }
    }
}