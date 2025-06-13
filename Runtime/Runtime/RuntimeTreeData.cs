using UnityEngine;
using System.Collections.Generic;

using Behaviours.Runtime;

namespace Behaviours.BT.Runtime
{
    public class RuntimeTreeData
    {
        public RuntimeValueCollection runtimeValues;
        public RuntimeNodeData runtimeNode;
        public GameObject gameObject;
        public Tree associatedTree;

        public void init(GameObject gameObject, Tree tree)
        {
            this.gameObject = gameObject;
            associatedTree = tree;
            runtimeValues = new RuntimeValueCollection();
            runtimeValues.InitWithLayer(tree);
            runtimeNode = new RuntimeNodeData();
            runtimeNode.init(this, associatedTree.root);
        }

        public void start()
        {
            if (runtimeNode != null)
            {
                runtimeNode.start();
            }
        }

        public State update(float dt)
        {
            if (runtimeNode != null)
            {
                return runtimeNode.update(dt);                
            }

            return State.Failed;
        }

        public void end()
        {
            if (runtimeNode != null)
            {
                runtimeNode.end();
            }
        }

        private List<object> _servicesValues = new List<object>();

        public T GetService<T>()
        {
            var value = (T) _servicesValues.Find(x => x is T);
            Debug.Assert(value != null, $"[{gameObject?.name}] Can not find service of type {typeof(T)}");
            return value;
        }
        
        public void SetService<T>(T value)
        {
            var index = _servicesValues.FindIndex(x => x is T);
            if (index == -1)
                _servicesValues.Add(value);
            else
                _servicesValues[index] = value;
        }
    }
}