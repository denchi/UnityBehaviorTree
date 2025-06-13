namespace Behaviours.BT.Runtime
{
    public class RuntimeTaskData
    {
        public RuntimeNodeData runtimeNode;
        public Task associatedTask;
        public float ratio;

        public virtual void init(RuntimeNodeData runtimeNode, Task task)
        {
            this.runtimeNode = runtimeNode;
            this.associatedTask = task;
            this.ratio = 0;
        }
        
        public T GetService<T>()
        {
            return runtimeNode.runtimeTree.GetService<T>();
        }
    }
}