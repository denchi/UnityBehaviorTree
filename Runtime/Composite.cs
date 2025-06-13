using System.Collections.Generic;

using Behaviours.BT.Runtime;

namespace Behaviours.BT
{
    [RuntimeData(typeof(CompositeTaskData))]
    public class Composite : Task
    {
        public List<Node> nodes = new List<Node>();        

        public class CompositeTaskData : RuntimeTaskData
        {
            public int currentIndex;
            public List<RuntimeNodeData> runtimeChildNodes;

            public override void init(RuntimeNodeData runtimeNode, Task task)
            {
                base.init(runtimeNode, task);

                var comp = task as Composite;

                this.runtimeChildNodes = new List<RuntimeNodeData>();
                for (int i = 0, n = comp.nodes.Count; i < n; ++i)
                {
                    var runtimeChildNode = new RuntimeNodeData();
                    runtimeChildNode.init(runtimeNode.runtimeTree, comp.nodes[i]);
                    runtimeChildNodes.Add(runtimeChildNode);
                }
            }
        }

        public override void start(RuntimeTaskData taskData)
        {
            base.start(taskData);

            var comp = taskData as CompositeTaskData;
            comp.currentIndex = -1;

            changeNode(taskData as CompositeTaskData, 0);
        }

        public override State update(RuntimeTaskData taskData, float dt)
        {
            var comp = taskData as CompositeTaskData;
            return comp.runtimeChildNodes[comp.currentIndex].update(dt);
        }

        public override void end(RuntimeTaskData taskData)
        {
            changeNode(taskData as CompositeTaskData, -1);
            base.end(taskData);
        }

        public void changeNode(CompositeTaskData comp, int nodeIndex)
        {
            if (comp.currentIndex != -1)
            {
                comp.runtimeChildNodes[comp.currentIndex].end();
            }

            comp.currentIndex = nodeIndex;

            if (comp.currentIndex != -1)
            {
                comp.runtimeChildNodes[comp.currentIndex].start();
            }
        }
    }    
}