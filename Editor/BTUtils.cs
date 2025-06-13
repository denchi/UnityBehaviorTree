using UnityEngine;
using System.Collections.Generic;

using UnityEditor;

namespace Behaviours.BT.Editor
{
    public class BTUtils
    {
        #region MENU

        [MenuItem("Behaviour/Tree/Create Tree Asset...")]
        static void BG_NEW_TASK()
        {
            string filePath = EditorUtility.SaveFilePanelInProject("Select a tree path...", "NewTree", "asset", "Creating a new Tree...");
            if (string.IsNullOrEmpty(filePath) == false)
            {
                Behaviours.BT.Editor.BTUtils.createTree(filePath);

                AssetDatabase.ImportAsset(filePath);
            }
        }

        #endregion

        #region CREATE

        public static Tree createTree(string assetPath)
        {
            Tree tree = CreateAsset<Tree>(assetPath);
            {
                tree.name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                EditorUtility.SetDirty(tree);

                tree.root = createNode();
                AssetDatabase.AddObjectToAsset(tree.root, tree);

                tree.root.tree = tree;
                tree.root.task = createTask(typeof(Composite), tree.root);
            }
            EditorUtility.SetDirty(tree);

            EditorUtility.SetDirty(tree.root);

            return tree;
        }

        public static Node createNode(Composite parent = null)
        {
            Node node = ScriptableObject.CreateInstance<Node>();
            {
                node.title = "Node";
            }

            node.rect = new Rect(0, 0, 200, 50);

            if (parent)
            {
                node.tree = parent.node.tree;
                node.parent = parent.node;

                parent.nodes.Add(node);

                AssetDatabase.AddObjectToAsset(node, node.tree); 

                EditorUtility.SetDirty(parent);
            }

            EditorUtility.SetDirty(node);

            return node;
        }

        public static Task createTask(System.Type stateType, Node node)
        {
            if (node.task)
            {
                removeTask(node.task);
            }

            Task state = ScriptableObject.CreateInstance(stateType) as Task;
            {
                state.name = stateType.Name;
                state.node = node;
                //state.hideFlags = HideFlags.HideInHierarchy;
            }

            node.task = state;

            AssetDatabase.AddObjectToAsset(state, node.tree);

            EditorUtility.SetDirty(node);

            return state;
        }

        public static IService createService(System.Type serviceType, Node node)
        {
            IService service = ScriptableObject.CreateInstance(serviceType) as IService;
            {
                service.name = serviceType.Name;
                service.node = node;
            }

            node.services.Add(service);

            AssetDatabase.AddObjectToAsset(service, node.tree);

            EditorUtility.SetDirty(node);

            return service;
        }

        public static void removeService(IService service, Node parentNode)
        {
            parentNode.services.Remove(service);

            Object.DestroyImmediate(service, true);

            EditorUtility.SetDirty(parentNode);
        }    

        #endregion        

        #region REMOVE        

        public static void removeNode(Node node, Composite parent = null)
        {
            if (node)
            {
                if (parent)
                {
                    parent.nodes.Remove(node);
                    EditorUtility.SetDirty(parent);
                }

                if (node.services != null)
                {
                    var temp = new List<IService>(node.services);
                    foreach (var s in temp)
                    {
                        removeService(s, node);
                    }
                    node.services.Clear();
                }

                removeTask(node.task);

                Object.DestroyImmediate(node, true);
            }
        }

        public static void removeTask(Task state, Node parent = null)
        {
            if (state)
            {
                if (state is Composite)
                {
                    Composite comp = state as Composite;
                    for (int i = 0; i < comp.nodes.Count; ++i)
                    {
                        removeNode(comp.nodes[i]);
                    }
                    Object.DestroyImmediate(comp, true);
                }
                else
                {
                    Object.DestroyImmediate(state, true);
                }
            }
        }

        //public static void removeTransition(Transition transition, Node parent = null)
        //{
        //    if (transition)
        //    {
        //        if (parent)
        //        {
        //            parent.transitions.Remove(transition);
        //            EditorUtility.SetDirty(parent);
        //        }

        //        for (int i = 0; i < transition.conditions.Count; ++i)
        //        {
        //            removeCondition(transition.conditions[i]);
        //        }

        //        Object.DestroyImmediate(transition, true);
        //    }
        //}

        //public static void removeCondition(Condition condition, Transition parent = null)
        //{
        //    if (condition)
        //    {
        //        if (parent)
        //        {
        //            parent.conditions.Remove(condition);
        //            EditorUtility.SetDirty(parent);
        //        }

        //        ValueUtils.removeCondition(condition);
        //    }
        //}

        #endregion

        public static Node cloneNode(Node originalNode, Node parent, ScriptableObject asset)
        {
            if (originalNode != null)
            {
                Node newNode = ScriptableObject.Instantiate(originalNode);
                {
                    AssetDatabase.AddObjectToAsset(newNode, asset);
                }

                newNode.parent = parent;

                if (originalNode.services != null)
                {
                    newNode.services = new List<IService>(originalNode.services);
                    for (var i = 0; i < originalNode.services.Count; i++)
                    {
                        newNode.services[i] = cloneService(originalNode.services[i], newNode, asset);
                    }
                }

                newNode.task = cloneTask(originalNode.task, newNode, asset);

                EditorUtility.SetDirty(newNode);

                if (parent)
                {
                    EditorUtility.SetDirty(parent);
                }

                return newNode;
            }

            return null;
        }

        public static IService cloneService(IService originalService, Node node, ScriptableObject asset)
        {
            if (originalService != null)
            {
                IService newService = ScriptableObject.Instantiate(originalService);
                {
                    AssetDatabase.AddObjectToAsset(newService, asset);
                }

                newService.node = node;

                EditorUtility.SetDirty(newService);

                if (node)
                {
                    EditorUtility.SetDirty(node);
                }

                return newService;
            }
            return null;
        }

        public static Task cloneTask(Task originalTask, Node node, ScriptableObject asset)
        {
            if (originalTask != null)
            {
                Task newTask = ScriptableObject.Instantiate(originalTask);
                {
                    AssetDatabase.AddObjectToAsset(newTask, asset);
                }
                
                newTask.node = node;

                if (newTask is Composite)
                {
                    Composite newComposite = newTask as Composite;
                    for (int i = 0; i < newComposite.nodes.Count; ++i)
                    {
                        newComposite.nodes[i] = cloneNode(newComposite.nodes[i], node, asset);
                    }
                }

                EditorUtility.SetDirty(newTask);

                if (node)
                {
                    EditorUtility.SetDirty(node);
                }

                return newTask;
            }
            return null;
        }

        #region MISC

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        static T1 CreateAsset<T1>(string assetPath) where T1 : ScriptableObject
        {
            T1 asset = ScriptableObject.CreateInstance<T1>();

            string assetFileName = AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.GetDirectoryName(assetPath) + "/" + System.IO.Path.GetFileNameWithoutExtension(assetPath) + ".asset");

            AssetDatabase.CreateAsset(asset, assetFileName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;

            return asset;
        }

        #endregion
    }    
}