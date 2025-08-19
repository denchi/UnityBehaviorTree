using System;
using System.Linq;
using System.Reflection;
using Behaviours.BT.Runtime;
using UnityEditor;
using UnityEngine;

namespace Behaviours.BT.Editor
{
    public class BTWindow : GenericWindow<Node>
    {
        public Tree tree;
        public Node selectedNode;
        public int _selectedTreeIndex = -1;

        private const string KeyPrefSelectedTreeIndex = "BehavioursSelectedIndex";
        
        private IBehaviourTreeProvider _behaviourTreeProvider;
        private UnityEditor.Editor selectedTaskEditor;
        private float _update_time;
        private int _DoMyWindowTab;
        private Node _copyBuffer_TREE_Node;

        public int SelectedTreeIndex
        {
            get { return _selectedTreeIndex; }
            set
            {
                _selectedTreeIndex = value;
            }
        }

        // Add menu named "My Window" to the Window menu
        [MenuItem("Window/Behaviour Tree Window")]
        private static void Init()
        {
            GetWindow(typeof(BTWindow), false);
        }

        #region Generic Window Implementation
        
        public override string GetWindowTitle() => "Tree";
        
        public override string GetViewTitle() => tree ? tree.name : "";
        
        public override void OnEnable()
        {
            base.OnEnable();

            windowWidth = 300;
            rectOffset = new RectOffset(1000, 1000, 500, 500);

            BTLog.Clear();
            _update_time = Time.time;

            SelectedTreeIndex = EditorPrefs.GetInt("BehavioursSelectedIndex", 0);

            OnSelectionChange();
        }

        public override void Update()
        {
            base.Update();

            var t = Time.time;
            var dt = t - _update_time;
            BTLog.Update(dt);
            _update_time = t;
        }

        public override void OnSelectionChange()
        {
            base.OnSelectionChange();

            if (selectedTaskEditor != null)
            {
                DestroyImmediate(selectedTaskEditor);
                selectedTaskEditor = null;
            }

            if (Selection.activeGameObject && Selection.activeGameObject.TryGetComponent<IBehaviourTreeProvider>(out var behaviourTreeProvider))
            {
                SetBehaviour(behaviourTreeProvider, Selection.activeGameObject);
                return;
            }
            
            if (Selection.activeObject is Tree activeTree)
            {
                _behaviourTreeProvider = null;
                SetTree(activeTree, null);
            }
        }
        
        #endregion
        
        #region Setting The Data
        
        private void SetBehaviour(IBehaviourTreeProvider behaviourTreeProvider, GameObject go)
        {
            this._behaviourTreeProvider = behaviourTreeProvider;

            if (this._behaviourTreeProvider != null)
            {
                SelectedTreeIndex = Mathf.Clamp(SelectedTreeIndex, 0, this._behaviourTreeProvider.GetTreesCount() - 1);
                SetTree(this._behaviourTreeProvider.GetTree(SelectedTreeIndex), go);
            }
        }

        private void SetTree(Tree newTree, GameObject go)
        {
            tree = newTree;
            if (tree)
            {
                tree._editorOnlyGameObject = go;

                FindMissingNodes(tree.root);
            }

            Repaint();
        }

        private void SelectNode(Node node)
        {
            if (node)
            {
                selectedNode = node;
            }
            else
            {
                selectedNode = null;
            }

            OnSelectedNodeChanged();
        }
        
        private bool FindMissingNodes(Node parent)
        {
            if (parent is not { task: Composite comp })
                return false;
            
            for (var i = 0; i < comp.nodes.Count; i++)
            {
                var child = comp.nodes[i];
                if (!FindMissingNodes(child)) 
                    continue;
                
                if (EditorUtility.DisplayDialog("Found a missing node!", "Delete the node?", "Yes", "No"))
                {
                    comp.nodes.RemoveAt(i);
                    EditorUtility.SetDirty(comp);
                    return false;
                }

                return true;
            }
        
            return false;
        }

        private static Node FindNode(Node node, Vector2 at)
        {
            if (node.rect.Contains(at))
                return node;

            if (node.task is Composite)
            {
                for (int i = 0; i < (node.task as Composite).nodes.Count; ++i)
                {
                    Node temp = FindNode((node.task as Composite).nodes[i], at);
                    if (temp != null)
                        return temp;
                }
            }

            return null;
        }

        private void RenameNodesRecursive(Node parent, Node node)
        {
            if (node.task)
            {
                if (parent)
                    node.title = node.tree.name + "-" + node.task.GetType().Name + "[" + parent.Children.IndexOf(node) + "]";
                else
                    node.title = node.tree.name + "-" + node.task.GetType().Name;

                EditorUtility.SetDirty(node);

                if (node.task is Composite)
                {
                    var com = node.task as Composite;
                    for (var i = 0; i < com.nodes.Count; i++)
                    {
                        RenameNodesRecursive(node, com.nodes[i]);
                    }
                }
            }
        }
        
        #endregion

        #region Drawing Items

        public override void drawItems()
        {
            base.drawItems();

            if (tree)
            {
                BTGUI.DrawTreeLayer(tree, offset, selectedNode, Event.current, _behaviourTreeProvider, SelectedTreeIndex);
            }
        }

        public override Rect CalcClientRect()
        {
            float xmin = float.PositiveInfinity;
            float xmax = float.NegativeInfinity;
            float ymin = float.PositiveInfinity;
            float ymax = float.NegativeInfinity;

            if (tree)
            {
                BTGUI.CalcClientRect(tree.root, ref xmin, ref xmax, ref ymin, ref ymax);
            }

            return Rect.MinMaxRect(xmin - rectOffset.left, ymin - rectOffset.top, xmax + rectOffset.right, ymax + rectOffset.bottom);
        }
        
        #endregion

        #region Input Events

        public override void OnCanvasMouseDown(Event evt)
        {
            if (tree == null)
                return;

            if (evt.button == 0)
            {
                Node newSelectedNode = FindNode(tree.root, evt.mousePosition);

                if (newSelectedNode != null)
                {
                    evt.Use();
                    SelectNode(newSelectedNode);
                }
                else
                {
                    evt.Use();
                    SelectNode(null);
                }

                Repaint();
            }
        }

        public override void OnCanvasMouseDragged(Event evt)
        {
            if (evt.button == 2)
            {
                offset.x -= evt.delta.x;
                offset.y -= evt.delta.y;

                if (offset.x < 0)
                    offset.x = 0;

                if (offset.y < 0)
                    offset.y = 0;

                _mouseDragged = true;

                evt.Use();

                Repaint();
            }            
        }        

        public override void OnContextMouseUp(Event evt)
        {
            if (evt.button == 1)
            {
                if (_mouseDragged == false)
                {
                    Node node = FindNode(tree.root, evt.mousePosition);
                    if (node != null)
                    {
                        SelectNode(node);

                        Vector2 mousePos = evt.mousePosition;

                        // Now create the menu, add items and show it
                        GenericMenu menu = new GenericMenu();

                        menu.AddItem(new GUIContent("Delete node"), false, Callback_OnContextMenuItemClick_DeleteNode, node);

                        menu.AddSeparator("");

                        menu.AddItem(new GUIContent("Copy task"), false, Callback_OnContextMenuItemClick_Copy, node);
                        menu.AddItem(new GUIContent("Paste task"), false, Callback_OnContextMenuItemClick_Paste, node);

                        menu.AddSeparator("");

                        menu.AddItem(new GUIContent("Encapsulate in.../Sequence"), false, Callback_OnContextMenuItemClick_EncapsulateIn<Sequence>, node);
                        menu.AddItem(new GUIContent("Encapsulate in.../Selector"), false, Callback_OnContextMenuItemClick_EncapsulateIn<Selector>, node);
                        menu.AddItem(new GUIContent("Encapsulate in.../Repeat"), false, Callback_OnContextMenuItemClick_EncapsulateIn<Repeat>, node);
                        menu.AddItem(new GUIContent("Encapsulate in.../Invert"), false, Callback_OnContextMenuItemClick_EncapsulateIn<Invert>, node);
                        menu.AddItem(new GUIContent("Encapsulate in.../Convert"), false, Callback_OnContextMenuItemClick_EncapsulateIn<Convert>, node);
                        menu.AddItem(new GUIContent("Encapsulate in.../Random"), false, Callback_OnContextMenuItemClick_EncapsulateIn<Random>, node);

                        Node parent = node.parent;
                        if (parent)
                        {
                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("Move back"), false, Callback_OnContextMenuItemClick_MoveBack, node);
                        }

                        menu.AddSeparator("");

                        var baseType = typeof(Task);
                        
                        // need all assemblies that contain tasks
                        var types = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(assembly => assembly.GetTypes())
                            .Where(type => type.IsSubclassOf(baseType) && type != typeof(Task))
                            .ToArray();
                        
                        for (int i = 0; i < types.Length; ++i)
                        {
                            if (types[i].IsSubclassOf(baseType) && (types[i] != typeof(Composite)))
                            {
                                SetTaskInfo ssi = new SetTaskInfo();
                                ssi.node = node;
                                ssi.type = types[i];
                                menu.AddItem(new GUIContent("Set task.../" + types[i].FullName.Replace("Behaviour.Tree.", "").Replace(".", "/")), false, Callback_OnContextMenuItemClick_SetTask, ssi);
                            }
                        }

                        menu.AddSeparator("");

                        baseType = typeof(IService);
                        types = Assembly.GetAssembly(baseType).GetTypes();
                        for (int i = 0; i < types.Length; ++i)
                        {
                            if (types[i].IsSubclassOf(baseType) && (types[i] != typeof(IService)))
                            {
                                SetTaskInfo ssi = new SetTaskInfo();
                                ssi.node = node;
                                ssi.type = types[i];
                                menu.AddItem(new GUIContent("Add service.../" + types[i].FullName.Replace("Behaviours.BT.", "").Replace(".", "/")), false, Callback_OnContextMenuItemClick_AddService, ssi);
                            }
                        }

                        menu.AddSeparator("");

                        menu.AddItem(new GUIContent("Other/Rename nodes recursively"), false, Callback_OnContextMenuItemClick_RenameNodes, node);
                        menu.AddItem(new GUIContent("Other/Check nodes recursively"), false, Callback_OnContextMenuItemClick_CheckNodes, node);
                        menu.AddItem(new GUIContent("Other/Check hierarchy recursively"), false, Callback_OnContextMenuItemClick_CheckNodesHierarchy, node);

                        menu.AddSeparator("");
                        menu.AddItem(new GUIContent("Remove Task"), false, Callback_OnContextMenuItemClick_RemoveTask, node);

                        if (node.task is Composite)
                        {
                            menu.AddSeparator("");

                            menu.AddItem(new GUIContent("Add node"), false, Callback_OnContextMenuItemClick_AddNode, node);
                        }

                        menu.ShowAsContext();
                        evt.Use();
                    }
                }
            }
        }

        #endregion
        
        #region Properties Window 

        public override void DoPropertiesWindowGUI(int wndId)
        {
            base.DoPropertiesWindowGUI(wndId);

            if (_behaviourTreeProvider != null)
            {                
                var names = new string[_behaviourTreeProvider.GetTreesCount()];
                for (int i = 0; i < names.Length; i++)
                {
                    names[i] = _behaviourTreeProvider.GetTree(i).GetType().Name;
                }
                
                int sel = EditorGUILayout.Popup("Behaviours", SelectedTreeIndex, names);
                if (sel != SelectedTreeIndex)
                {
                    SelectedTreeIndex = sel;
                    EditorPrefs.SetInt(KeyPrefSelectedTreeIndex, SelectedTreeIndex);
                }
            }

            _DoMyWindowTab = GUILayout.Toolbar(_DoMyWindowTab, new[] { "Tree", "Task" });
            if (_DoMyWindowTab == 0)
            {
                ValueGUI.DrawLayerValues("Vars", tree);
            }
            else if (_DoMyWindowTab == 1)
            {
                // draw editor
                if (selectedTaskEditor != null)
                {
                    selectedTaskEditor.OnInspectorGUI();
                }
            }
        }

        #endregion

        #region Callbacks
        
        private void OnSelectedNodeChanged()
        {
            if (selectedTaskEditor != null)
            {
                DestroyImmediate(selectedTaskEditor);
                selectedTaskEditor = null;
            }

            if (selectedNode != null)
            {
                if (selectedNode.task != null)
                {
                    selectedTaskEditor = UnityEditor.Editor.CreateEditor(selectedNode.task);
                }
            }
        }

        private void Callback_OnContextMenuItemClick_DeleteNode(object userData)
        {
            Node node = userData as Node;
            Node parent = node.parent;
            if (parent != null)
            {
                Composite parentCompositeTask = parent.task as Composite;

                selectedNode = null;

                BTUtils.removeTask(node.task, node);

                parentCompositeTask.nodes.Remove(node);

                DestroyImmediate(node, true);

                EditorUtility.SetDirty(parentCompositeTask);
            }
            else
            {
                Debug.Log("Node not found!");
            }

            Repaint();
        }

        private void Callback_OnContextMenuItemClick_Copy(object userData)
        {
            _copyBuffer_TREE_Node = userData as Node;

            Repaint();
        }

        private void Callback_OnContextMenuItemClick_Paste(object userData)
        {
            if (_copyBuffer_TREE_Node && _copyBuffer_TREE_Node.task)
            {
                Node node = userData as Node;

                Task taskToDelete = node.task;

                Task taskToClone = _copyBuffer_TREE_Node.task;

                node.task = BTUtils.cloneTask(taskToClone, node, tree);

                BTUtils.removeTask(taskToDelete, node);

                Repaint();
            }
        }

        private void Callback_OnContextMenuItemClick_EncapsulateIn<T>(object userData) where T : Task
        {
            Node node = userData as Node;
            {
                // copy old task
                Task taskToCopy = node.task;

                // delete old task info
                node.task = null;

                // create parent task
                Composite taskToCreate = BTUtils.createTask(typeof(T), node) as Composite;
                taskToCreate.node = node;
                node.task = taskToCreate;

                // create a child node and add itto composite
                Node childNode = BTUtils.createNode(taskToCreate);

                // add old task to child node
                childNode.task = taskToCopy;

                if (taskToCopy)
                {
                    taskToCopy.node = childNode;
                }

                EditorUtility.SetDirty(childNode);
            }
        }

        private void Callback_OnContextMenuItemClick_MoveBack(object userData)
        {
            Node node = userData as Node;
            if (node)
            {
                Node parent = node.parent;
                if (parent)
                {
                    Composite composite = parent.task as Composite;
                    if (composite)
                    {
                        int index = composite.nodes.IndexOf(node);
                        if (index > 0)
                        {
                            Node temp = composite.nodes[index - 1];
                            composite.nodes[index - 1] = node;
                            composite.nodes[index] = temp;

                            EditorUtility.SetDirty(composite);

                            Repaint();
                        }
                    }
                }
            }
        }

        private void Callback_OnContextMenuItemClick_InsertSelector(object userData)
        {
            Node node = userData as Node;
            if (node)
            {
                Node parent = node.parent;
                if (parent)
                {
                    Composite parentCompositeTask = parent.task as Composite;

                    if (parentCompositeTask != null)
                    {
                        Node newnode = BTUtils.createNode(parentCompositeTask);
                        newnode.tree = node.tree;
                        newnode.rect = new Rect(0, 0, newnode.rect.width, newnode.rect.height);
                        EditorUtility.SetDirty(newnode);

                        newnode.task = BTUtils.createTask(typeof(Selector), newnode);
                        newnode.task.node = newnode;
                        (newnode.task as Composite).nodes.Add(node);
                        EditorUtility.SetDirty(newnode.task);

                        selectedNode = newnode;

                        Repaint();
                    }
                }
            }
        }

        private void Callback_OnContextMenuItemClick_InsertSequence(object userData)
        {
            Node node = userData as Node;
            if (node)
            {
                Node parent = node.parent;
                if (parent)
                {
                    Composite parentCompositeTask = parent.task as Composite;

                    if (parentCompositeTask != null)
                    {
                        Node newnode = BTUtils.createNode(parentCompositeTask);
                        newnode.tree = node.tree;
                        newnode.rect = new Rect(0, 0, newnode.rect.width, newnode.rect.height);
                        EditorUtility.SetDirty(newnode);

                        newnode.task = BTUtils.createTask(typeof(Sequence), newnode);
                        newnode.task.node = newnode;
                        (newnode.task as Composite).nodes.Add(node);
                        EditorUtility.SetDirty(newnode.task);

                        selectedNode = newnode;

                        Repaint();
                    }
                }
            }
        }

        private void Callback_OnContextMenuItemClick_SetTask(object userData)
        {
            SetTaskInfo ssi = userData as SetTaskInfo;

            selectedNode = null;

            ssi.node.task = BTUtils.createTask(ssi.type, ssi.node);

            ssi.node.task.node = ssi.node;

            EditorUtility.SetDirty(ssi.node.task);

            Repaint();
        }

        private void Callback_OnContextMenuItemClick_AddService(object userData)
        {
            SetTaskInfo ssi = userData as SetTaskInfo;

            selectedNode = null;

            BTUtils.createService(ssi.type, ssi.node);

            Repaint();
        }

        private void Callback_OnContextMenuItemClick_RemoveTask(object userData)
        {
            Node node = userData as Node;

            selectedNode = null;

            BTUtils.removeTask(node.task, node);

            node.task = null;

            EditorUtility.SetDirty(node);

            Repaint();
        }
        
        private void Callback_OnContextMenuItemClick_RenameNodes(object userData)
        {
            Node node = userData as Node;

            RenameNodesRecursive(null, node);
            EditorUtility.SetDirty(node.tree);

            Repaint();
        }

        private void Callback_OnContextMenuItemClick_CheckNodes(object userData)
        {
            Node node = userData as Node;

            CheckNodesRecursive(node);

            Repaint();
            
            void CheckNodesRecursive(Node node)
            {
                if (node.task)
                {
                    var t1 = node.task.GetType();
                    var t2 = MonoScript.FromScriptableObject(node.task).GetClass();

                    if (t1 != t2)
                    {
                        Debug.LogErrorFormat("Type {0} found != {1}", t1.Name, t2.Name);
                    }

                    if (node.task is Composite)
                    {
                        var com = node.task as Composite;
                        for (var i = 0; i < com.nodes.Count; i++)
                        {
                            CheckNodesRecursive(com.nodes[i]);
                        }
                    }
                }
            }
        }
        
        private void Callback_OnContextMenuItemClick_CheckNodesHierarchy(object userData)
        {
            Node node = userData as Node;

            CheckNodesRecursive(node);

            Repaint();
            
            void CheckNodesRecursive(Node node)
            {
                if (node.Children != null)
                {
                    foreach (var child in node.Children)
                    {
                        if (child.parent != node)
                        {
                            Debug.LogWarning("Found child with wrong parent!. Fixing...");
                            child.parent = node;
                            EditorUtility.SetDirty(child);
                        }    
                        
                        CheckNodesRecursive(child);
                    }
                }
            }
        }

        private void Callback_OnContextMenuItemClick_AddNode(object userData)
        {
            Node node = userData as Node;
            Composite parentCompositeTask = node.task as Composite;

            Node newnode = BTUtils.createNode(parentCompositeTask);

            newnode.tree = node.tree;

            EditorUtility.SetDirty(newnode);

            newnode.rect = new Rect(0, 0, newnode.rect.width, newnode.rect.height);

            selectedNode = newnode;

            Repaint();
        }

        #endregion
        
        public class SetTaskInfo
        {
            public Type type;
            public Node node;
        }
    }
}