using Behaviours.BT.Runtime;

namespace Behaviours.BT.Services
{
    [RuntimeData(typeof(ServiceData))]
    public class Timeout : IService
    {
        public float timeoutTimeMin = 1;
        public float timeoutTimeMax = 1;
        public State timeoutResult = State.Failed;

        public class ServiceData : Runtime.RuntimeServiceData
        {
            public float elapsedTime = 0;
            public float timeoutTime = 0;
        }

        public override void onServiceStarted(RuntimeServiceData serviceData)
        {
            base.onServiceStarted(serviceData);

            ServiceData data = (ServiceData)serviceData;
            data.elapsedTime = 0;
            data.timeoutTime = UnityEngine.Random.Range(timeoutTimeMin, timeoutTimeMax);
        }

        public override State tick(RuntimeServiceData serviceData)
        {
            ServiceData data = (ServiceData)serviceData;
            data.elapsedTime += data._tickTotalTime;
            if (data.elapsedTime >= data.timeoutTime)
            {
                return State.Failed;
            }

            return base.tick(serviceData);
        }
    }
}