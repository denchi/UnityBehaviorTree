using UnityEngine;
using Behaviours.BT.Runtime;

namespace Behaviours.BT
{
    public class Task : ScriptableObject
    {
        [SerializeField]
        public bool debugBreak;
        
        [SerializeField]
        public State debugBreakCondition = State.Failed;

        [SerializeField]
        public Node node;

        public virtual string Subtitle => "";

        public virtual string Title => GetType().Name;

        public RuntimeTaskData createRuntimeTaskData(Runtime.RuntimeNodeData runtimeNode)
        {
            var runtimeTask = RuntimeDataAttribute.createRuntimeDataFromAttribute<Runtime.RuntimeTaskData>(GetType());
            if (runtimeTask == null)
            {
                runtimeTask = new RuntimeTaskData();
            }

            runtimeTask.init(runtimeNode, this);

            return runtimeTask;
        }

        public virtual void start(RuntimeTaskData taskData)
        {
            
        }        

        public virtual State update(RuntimeTaskData taskData, float dt)
        {
            return State.Running;
        }

        public virtual void end(RuntimeTaskData taskData)
        {
            
        }

        public virtual void OnEnable()
        {
            hideFlags = HideFlags.HideInHierarchy;
        }
    }
}