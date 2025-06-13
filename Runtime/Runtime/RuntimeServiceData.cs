namespace Behaviours.BT.Runtime
{
    public class RuntimeServiceData
    {
        public RuntimeNodeData runtimeNode;
        public IService associatedService;

        public float _tickTotalTime = 1; // 1 : ticksPerSecond = 0.0(3);
        public float _tickElapsedTime = 0;

        public virtual void init(RuntimeNodeData runtimeNode, IService associatedService)
        {
            this.runtimeNode = runtimeNode;
            this.associatedService = associatedService;
            _tickTotalTime = 1.0f / associatedService.ticksPerSecond;
        }
    }
}