using Behaviours.BT.Runtime;

namespace Behaviours.BT
{
    [RuntimeData(typeof(RandomTaskData))]
    public class Random : Composite
    {
        public bool shuffleResults = true;

        public class RandomTaskData : CompositeTaskData
        {
            public int lastChosenIndex;

            public override void init(RuntimeNodeData runtimeNode, Task task)
            {
                base.init(runtimeNode, task);
                lastChosenIndex = -1;
            }
        }

        public override void start(RuntimeTaskData taskData)
        {
            var runtimeTask = taskData as RandomTaskData;

            if (shuffleResults)
            {
                if (nodes.Count == 1)
                {
                    runtimeTask.currentIndex = runtimeTask.lastChosenIndex = 0;
                }
                else if (nodes.Count == 2)
                {
                    runtimeTask.currentIndex = runtimeTask.lastChosenIndex = runtimeTask.lastChosenIndex == 0 ? 1 : 0;
                }
                else
                {
                    var tries = 5;
                    var idx = UnityEngine.Random.Range(0, nodes.Count);
                    while (idx == runtimeTask.lastChosenIndex && tries > 0)
                    {
                        idx = UnityEngine.Random.Range(0, nodes.Count);
                        tries++;
                    }

                    runtimeTask.currentIndex = runtimeTask.lastChosenIndex = idx;
                }
            }
            else
            {
                runtimeTask.currentIndex = runtimeTask.lastChosenIndex = UnityEngine.Random.Range(0, nodes.Count);
            }

            changeNode(taskData as CompositeTaskData, runtimeTask.currentIndex);
        }

        public override State update(RuntimeTaskData runtimeTask, float dt)
        {
            var comp = runtimeTask as RandomTaskData;
            if (comp.currentIndex != -1)
            {
                var currentNode = comp.runtimeChildNodes[comp.currentIndex];
                State state = currentNode.update(dt);
#if UNITY_EDITOR
                currentNode._editorReturnedState = state;
#endif
                return state;
            }

            return State.Failed;
        }
    }
}