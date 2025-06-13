using Behaviours.BT.Runtime;

namespace Behaviours.BT
{
    public interface IBehaviourTreeProvider
    {
        int GetTreesCount();
        RuntimeTreeData GetRuntimeTreeData(int index);
        Tree GetTree(int index);
        RuntimeNodeData FindRuntimeNodeDataForNode(int index, Node node);
        
        // void pause();
        // void resume();
        // void restart();
    }
}