using UnityEngine;

namespace Behaviours
{
    public class BehaviourUtils
    {
        static void checkAddToAsset(ScriptableObject target, string assetPath, UnityEngine.ScriptableObject parent = null)
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(assetPath) == false)
            {
                if (parent)
                {
                    Debug.LogFormat("Adding {0}:{1} to {2}", target.GetType().Name, target.name, assetPath);

                    UnityEditor.AssetDatabase.AddObjectToAsset(target, assetPath);

                    UnityEditor.EditorUtility.SetDirty(parent);
                }
                else
                {
                    Debug.LogFormat("Creating {0}:{1} at {2}", target.GetType().Name, target.name, assetPath);

                    UnityEditor.AssetDatabase.CreateAsset(target, assetPath);
                    // UnityEditor.AssetDatabase.SaveAssets();
                    // UnityEditor.AssetDatabase.Refresh();
                }
            }
#endif
        }

        public static BT.Node Clone(BT.Node original, BT.Composite parent, string assetPath)
        {
            BT.Node node = UnityEngine.Object.Instantiate<BT.Node>(original);
            {
                node.name = original.name;

                if (original.task)
                {
                    node.task = Clone(original.task, node, assetPath);
                }

                checkAddToAsset(node, assetPath, parent);
            }

            return node;
        }

        public static BT.Node Clone(BT.Node original, BT.Tree parent, string assetPath)
        {
            BT.Node node = UnityEngine.Object.Instantiate<BT.Node>(original);
            {
                node.name = original.name;

                if (original.task)
                {
                    node.task = Clone(original.task, node, assetPath);
                }

                parent.root = node;

                checkAddToAsset(node, assetPath, parent);
            }

            return node;
        }

        public static BT.Task Clone(BT.Task original, BT.Node node, string assetPath)
        {
            BT.Task task = UnityEngine.Object.Instantiate<BT.Task>(original);
            {
                task.name = original.name;
                task.node = node;

                if (task is BT.Composite)
                {
                    BT.Composite composite = task as BT.Composite;
                    for (int i = 0; i < composite.nodes.Count; ++i)
                    {
                        composite.nodes[i] = Clone(composite.nodes[i], composite, assetPath);                        
                    }
                }

                checkAddToAsset(task, assetPath, node);
            }

            return task;
        }
    }
}
