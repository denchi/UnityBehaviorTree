using Behaviours.BT.Runtime;

namespace Behaviours.BT
{
    [RuntimeData(typeof(WaitRuntimeData))]
    public class Wait : Task
    {
        public float time = 1;

        public class WaitRuntimeData : RuntimeTaskData
        {
            public int currentIndex;
            public float elapsedTime;
        }

        public override void start(RuntimeTaskData taskData)
        {
            base.start(taskData);

            var runtimeTask = taskData as WaitRuntimeData;
            runtimeTask.elapsedTime = 0;
        }

        public override State update(RuntimeTaskData taskData, float dt)
        {
            WaitRuntimeData runtimeTask = taskData as WaitRuntimeData;
            {
                runtimeTask.elapsedTime += dt;

                if (runtimeTask.elapsedTime >= time)
                    return State.Success;

                return State.Running;
            }            
        }

        public override string Title
        {
            get
            {
                return base.Title + " " + time.ToString() + "(s)";
            }
        }
    }
}