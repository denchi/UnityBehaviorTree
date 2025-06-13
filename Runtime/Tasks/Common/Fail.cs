using Behaviours.BT.Runtime;

namespace Behaviours.BT
{
    [TaskColor("#F08")]
    public class Fail : Task
    {
        public override State update(RuntimeTaskData taskData, float dt)
        {
            return State.Failed;
        }
    }
}