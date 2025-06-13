using Behaviours.BT.Runtime;

namespace Behaviours.BT.Conditions
{
    public class CheckProbability : ICondition
    {
        public float probability;

        public override State check(RuntimeTaskData taskData)
        {
            float r = UnityEngine.Random.value;

            if (probability >= r)
                return State.Success;
            else
                return State.Failed;
        }    
    }
}