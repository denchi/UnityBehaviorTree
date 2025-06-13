using Behaviours.BT.Runtime;

namespace Behaviours.BT.Conditions
{
    public class ICondition : Task
    {
        public virtual State check(RuntimeTaskData task)
        {
            return State.Success;
        }

        public override State update(RuntimeTaskData task, float dt)
        {
            return check(task);
        }
    }
}