using UnityEngine;

namespace Behaviours.BT
{
    [CreateAssetMenu(menuName = "Behaviours/Tree")]
    public class Tree : ILayer
    {
        public Node root;        
    }
}