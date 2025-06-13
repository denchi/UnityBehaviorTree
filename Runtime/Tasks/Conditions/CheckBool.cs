using Behaviours.BT.Runtime;
using Behaviours.Runtime;

namespace Behaviours.BT.Conditions
{
    [RuntimeData(typeof(CheckBoolData))]
    public class CheckBool : ICondition
    {
        public string varName;
        public bool varValue;

        public override string Subtitle
        {
            get
            {
                return string.Format("{0} {1} {2}", varName, "==", varValue);
            }
        }

        public class CheckBoolData : RuntimeTaskData
        {
            public RuntimeValueData runtimeValueData;

            public override void init(RuntimeNodeData runtimeNode, Task task)
            {
                base.init(runtimeNode, task);

                var runtimeTask = (CheckBool) task;
                runtimeValueData = runtimeNode.runtimeTree.runtimeValues.GetRuntimeValue(runtimeTask.varName);
            }
        }

        public override State check(RuntimeTaskData taskData)
        {
            var runtimeTask = taskData as CheckBoolData;
            return runtimeTask.runtimeValueData.bValue == varValue ? State.Success : State.Failed;
        }
    }
}