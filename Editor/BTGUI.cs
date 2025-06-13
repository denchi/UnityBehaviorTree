using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;

namespace Behaviours.BT.Editor
{
    public class BTGUI
    {
        private static readonly Color ColorCurveDefault = Color.gray;
        
        private static readonly Color FrameColorDefault = new Color(38/225f, 38/225f, 38/225f);
        private static readonly Color FrameColorComposite = new Color(28/225f, 28/225f, 28/225f);
        private static readonly float WidthCurveDefault = 5.0f;
        private static readonly float MultiplierCurveDefault = 3.0f;

        private static readonly Vector2 Spacing = new Vector2(15, 50);
        private static readonly Vector2 NodeSize = new Vector2(200, 70);
        
        private const float ServiceBoxHeight = 24;
        private const float ServiceBoxSpacing = 2;
        private const float ServiceBoxSpacingBottom = 5;
        private const float ServiceBoxButtonSize = 18;
        
        private const float PinSize = 5;

        private static Color GetColorByState(State state) =>
            state switch
            {
                State.Failed => Color.red,
                State.Running => new Color(1, 0.5f, 0),
                _ => Color.green
            };

        private static void DrawTreeNode(Node node, Node parent, Node selectedNode, Event evt, IBehaviourTreeProvider behaviourTreeProvider, int treeIndex)
        {
            DrawLines();

            // GET COLOR
            var frameColor = GetBackColor();

            if (node == selectedNode)
            {
                DrawSelected();
            }
            
            DrawNode();
            //DrawPins();
            DrawServices();

            if (node.task)
            {
                DrawLogs();
            }

            DrawChildren();
            
            //
            
            Color GetBackColor()
            {
                var color = FrameColorDefault;
                if (node.task is Composite)
                {
                    color = FrameColorComposite;
                }

                if (Application.isPlaying && behaviourTreeProvider != null)
                {
                    var nodeData = behaviourTreeProvider.FindRuntimeNodeDataForNode(treeIndex, node);

                    Debug.AssertFormat(nodeData != null, "Task data for {0}-{1} is NULL!", node.title, node.tree.name);

                    if (nodeData._editorOnlyRunning)
                    {
                        color = GetColorByState(nodeData._editorReturnedState);
                    }
                    else if (nodeData._editorOnlyTriggerred)
                    {
                        nodeData._editorOnlyTriggerred = false;
                        nodeData._editorTimeMark = Time.unscaledTime;
                        color = GetColorByState(nodeData._editorReturnedState);
                        if (nodeData.associatedNode.task.debugBreak &&
                            nodeData.associatedNode.task.debugBreakCondition == nodeData._editorReturnedState)
                        {
                            Debug.Break();
                        }
                    }
                    else if (nodeData._editorTimeMark > 0)
                    {
                        color = Color.Lerp(GetColorByState(nodeData._editorReturnedState), color,
                            (Time.unscaledTime - nodeData._editorTimeMark));
                    }
                }

                return color;
            }

            void DrawLines()
            {
                if (node.task is not Composite task) 
                    return;
                
                if (task is Selector or Random)
                {
                    foreach (var otherNode2 in task.nodes)
                    {
                        var pp0 = new Vector2(otherNode2.rect.center.x - 4 + 5, otherNode2.rect.yMin + 8 + 5);
                        var pp2 = new Vector2(node.rect.center.x - 4 + 5, node.rect.yMax - 20 + 5);
                        EditorBehaviourGUI.DrawCurveVertical(pp0, pp2, ColorCurveDefault, WidthCurveDefault, MultiplierCurveDefault);
                    }

                    return;
                }

                if (!task.nodes.IsNullOrEmpty())
                {
                    var otherNode = task.nodes[0];
                    var p0 = new Vector2(otherNode.rect.center.x - 4 + 5, otherNode.rect.yMin + 8 + 5);
                    var p2 = new Vector2(node.rect.center.x - 4 + 5, node.rect.yMax - 20 + 5);
                    EditorBehaviourGUI.DrawCurveVertical(p0, p2, ColorCurveDefault, WidthCurveDefault,
                        MultiplierCurveDefault);

                    for (var i = 1; i < task.nodes.Count; ++i)
                    {
                        p0 = task.nodes[i - 1].rect.center;
                        var p1 = task.nodes[i].rect.center;
                        EditorBehaviourGUI.DrawCurve(p0, p1, ColorCurveDefault, WidthCurveDefault);
                    }
                }
            }

            void DrawSelected()
            {
                float b = 3;
                var sel = new Rect(node.rect.position - Vector2.one * b, node.rect.size + Vector2.one * (b * 2));
                GUI.BeginGroup(sel);
                {
                    EditorBehaviourGUI.BeginTint(new Color(0, 0.5f, 1));
                    {
                        GUI.Box(new Rect(0, 0, sel.width, sel.height), " ", EditorBehaviourGUI.boxState);
                    }
                    EditorBehaviourGUI.EndTint();
                }
                GUI.EndGroup();
            }

            void DrawNode()
            {
                // DRAW NODE FRONT
                GUI.BeginGroup(node.rect);
                {
                    if (node.task)
                    {
                        EditorBehaviourGUI.BeginTint(frameColor);
                        {
                            GUI.Box(
                                new Rect(0, 0, node.rect.width, node.rect.height),
                                string.IsNullOrEmpty(node.task.Subtitle) ? 
                                    new GUIContent($"{node.task.Title}") : 
                                    new GUIContent($"{node.task.Title}\n<size=8><color=cyan><i>{node.task.Subtitle}</i></color></size>"), 
                                EditorBehaviourGUI.boxNodeBack);
                        }
                        EditorBehaviourGUI.EndTint();

                        if (GetTaskColor(node.task, out var color))
                        {
                            EditorBehaviourGUI.FillRectangle(new Rect(0, 0, node.rect.width, 3), color);
                        }
                    }
                    else
                    {
                        EditorBehaviourGUI.BeginTint(frameColor);
                        {
                            GUI.Box(
                                new Rect(0, 0, node.rect.width, node.rect.height), 
                                new GUIContent(node.name),
                                EditorBehaviourGUI.boxNode);
                        }
                        EditorBehaviourGUI.EndTint();
                    }
                }
                GUI.EndGroup();
            }

            void DrawChildren()
            {
                // DRAW CHILD NODES
                if (node.task is Composite composite)
                {
                    foreach (var otherNode in composite.nodes)
                    {
                        DrawTreeNode(otherNode, node, selectedNode, evt, behaviourTreeProvider, treeIndex);
                    }
                }
            }

            void DrawLogs()
            {
                Runtime.BTLog.Do(node.task, (text, time, idx) =>
                {
                    var rectServiceBox = new Rect(
                        node.rect.x, 
                        node.rect.y - ServiceBoxSpacingBottom - (idx + 1) * (ServiceBoxHeight + ServiceBoxSpacing),
                        node.rect.width, 
                        ServiceBoxHeight);
                    
                    EditorBehaviourGUI.BeginTint(Color.yellow);
                    {
                        EditorBehaviourGUI.FillRectangle(rectServiceBox, Color.yellow);
                        GUI.Label(rectServiceBox, new GUIContent(text));
                    }
                    EditorBehaviourGUI.EndTint();
                });
            }

            void DrawPins()
            {
                if (ShouldShowTopPin())
                {
                    DrawPin(node.rect.center.x, node.rect.yMin + 8);
                }

                if (ShouldShowLeftPin())
                {
                    DrawPin(node.rect.xMin - 4 + 10 + 1, node.rect.center.y);
                }

                // if (ShouldShowBottomPin())
                // {
                //     GUI.DrawTexture(new Rect(node.rect.center.x - 4, node.rect.yMax - 20, 10, 10), EditorBehaviourGUI.getTexture("pin"));
                // }

                bool ShouldShowTopPin()
                {
                    if (!node.parent)
                        return false;

                    if (node == selectedNode)
                    {
                        Debug.Break();
                    }

                    if (node.parent.task is Sequence)
                    {
                        return node.parent.Children.FirstOrDefault() == node;
                    }

                    return true;

                }

                bool ShouldShowLeftPin()
                {
                    if (node.parent)
                    {
                        if (node.parent.task is Sequence)
                        {
                            if (node.parent.Children.FirstOrDefault() != node)
                                return true;
                        }
                    }

                    return false;
                }

                bool ShouldShowBottomPin()
                {
                    return node.Children != null && node.Children.Count > 0;
                }
            }

            void DrawPin(float x, float y)
            {
                Color c = GUI.color;
                GUI.color = Color.gray;
                GUI.DrawTexture(
                    new Rect(x - PinSize/2, y - PinSize/2, PinSize, PinSize),
                    EditorBehaviourGUI.getTexture("pin"));
                GUI.color = c;
            }
            
            void DrawServices()
            {
                var serviceToDelete = -1;

                for (var i = 0; i < node.services.Count; i++)
                {
                    var service = node.services[i];
                    var rectServiceBox = new Rect(
                        node.rect.x,
                        node.rect.y - ServiceBoxSpacingBottom - (i + 1) * (ServiceBoxHeight + ServiceBoxSpacing),
                        node.rect.width,
                        ServiceBoxHeight);

                    EditorBehaviourGUI.FillRectangle(rectServiceBox, frameColor);
                    GUI.Label(rectServiceBox, new GUIContent(service.name));

                    var rectServiceButton = new Rect(
                        rectServiceBox.x + rectServiceBox.width - ServiceBoxButtonSize - 2,
                        rectServiceBox.y + rectServiceBox.height / 2 - ServiceBoxButtonSize / 2f,
                        ServiceBoxButtonSize,
                        ServiceBoxButtonSize);

                    if (GUI.Button(rectServiceButton, "x"))
                    {
                        serviceToDelete = i;
                    }
                }

                if (serviceToDelete != -1)
                {
                    BTUtils.removeService(node.services[serviceToDelete], node);
                }
            }
        }

        private static readonly Dictionary<System.Type, Color> ColorsByType = new Dictionary<Type, Color>();

        private static bool GetTaskColor(Task nodeTask, out Color color)
        {
            if (!nodeTask)
            {
                color = default;
                return false;
            }

            var type = nodeTask.GetType();

            if (ColorsByType.TryGetValue(type, out var taskColor))
            {
                color = taskColor;
                return true;
            }

            var attrs = type.GetCustomAttributes(typeof(TaskColorAttribute),true);
            
            if (attrs.Length > 0 && attrs[0] is TaskColorAttribute attr)
            {
                color = ColorsByType[type] = attr.Color;
                return true;
            }
            
            /*
            
            if (nodeTask is Selector)
            {
                color = new Color(1, 0.5f, 0.0f, alpha);
                return true;
            }
            
            if (nodeTask is Sequence)
            {
                color = new Color(0.5f, 0.0f, 1.0f, alpha);
                return true;
            }
            
            if (nodeTask is Repeat)
            {
                color = new Color(1.0f, 0.0f, 0.25f, alpha);
                return true;
            }
            
            if (nodeTask is Random)
            {
                color = new Color(0.5f, 0.25f, 0.5f, alpha);
                return true;
            }
            
            if (nodeTask is Fail)
            {
                color = new Color(1.0f, 0.0f, 0.25f, alpha);
                return true;
            }
            
            if (nodeTask is not Composite)
            {
                color = new Color(0.5f, 0.0f, 1.0f, alpha);
                return true;
            }
            
            */

            color = default;
            return false;
        }

        public static void DrawTreeLayer(Tree tree, Vector2 offset, Node selectedNode, Event evt, IBehaviourTreeProvider behaviourTreeProvider, int treeIndex)
        {
            if (evt.type == EventType.Layout)
            {
                TreeHelpers.CalculateNodePositions(tree.root);

                TreeHelpers.TransposePositions(tree.root, NodeSize.x, NodeSize.y, Spacing.x, Spacing.y, Vector2.zero);                
            }

            DrawTreeNode(tree.root, null, selectedNode, evt, behaviourTreeProvider, treeIndex);
        }

        public static void CalcClientRect(Node node, ref float xMin, ref float xMax, ref float yMin, ref float yMax)
        {
            if (!node) 
                return;

            var r = node.rect;

            ClampRect(ref xMin, ref xMax, ref yMin, ref yMax, r);

            var comp = node.task as Composite;
            if (!comp) 
                return;

            for (int i = 0, cnt = comp.nodes.Count; i < cnt; ++i)
            {
                CalcClientRect(comp.nodes[i], ref xMin, ref xMax, ref yMin, ref yMax);
            }
        }

        private static void ClampRect(ref float xMin, ref float xMax, ref float yMin, ref float yMax, Rect r)
        {
            if (r.xMin < xMin)
            {
                xMin = r.xMin;
            }

            if (r.xMax > xMax)
            {
                xMax = r.xMax;
            }

            if (r.yMin < yMin)
            {
                yMin = r.yMin;
            }

            if (r.yMax > yMax)
            {
                yMax = r.yMax;
            }
        }

        public static class TreeHelpers
        {
            private static int nodeSize = 1;
            private static float siblingDistance = 0.0F;
            private static float treeDistance = 0.0F;

            public static void CalculateNodePositions(Node rootNode)
            {
                if (rootNode)
                {
                    // initialize node x, y, and mod values
                    InitializeNodes(rootNode, 0);

                    // assign initial X and Mod values for nodes
                    CalculateInitialX(rootNode);

                    // ensure no node is being drawn off screen
                    CheckAllChildrenOnScreen(rootNode);

                    // assign final X values to nodes
                    CalculateFinalPositions(rootNode, 0);
                }
            }

            // recusrively initialize x, y, and mod values of nodes
            private static void InitializeNodes(Node node, int depth)
            {
                if (node)
                {
                    node.X = -1;
                    node.Y = depth;
                    node.Mod = 0;

                    foreach (var child in node.Children)
                        InitializeNodes(child, depth + 1);
                }
            }

            private static void CalculateFinalPositions(Node node, float modSum)
            {
                node.X += modSum;
                modSum += node.Mod;

                foreach (var child in node.Children)
                    CalculateFinalPositions(child, modSum);

                if (node.Children.Count == 0)
                {
                    node.Width = node.X;
                    node.Height = node.Y;
                }
                else
                {
                    node.Width = node.Children.OrderByDescending(p => p.Width).First().Width;
                    node.Height = node.Children.OrderByDescending(p => p.Height).First().Height;
                }
            }

            public static void TransposePositions(Node node, float nw, float nh, float sx, float sy, Vector2 offset)
            {
                node.rect = new Rect(offset.x + node.X * (nw + sx), offset.y + node.Y * (nh + sy), nw, nh);

                //var maxServiceCount = 0;
                //if (node.task is Composite)
                //{
                //    var composite = node.task as Composite;
                //    for (int i = 0; i < composite.nodes.Count; ++i)
                //    {
                //        if (composite.nodes[i].services != null)
                //        {
                //            maxServiceCount = Mathf.Max(maxServiceCount, composite.nodes[i].services.Count);
                //        }
                //    }
                //}

                //offset.y += maxServiceCount * (30);

                foreach (Node n in node.Children)
                {
                    TransposePositions(n, nw, nh, sx, sy, offset);
                }
            }

            private static void CalculateInitialX(Node node)
            {
                if (node)
                {
                    foreach (var child in node.Children)
                        CalculateInitialX(child);

                    // if no children
                    if (node.IsLeaf())
                    {
                        // if there is a previous sibling in this set, set X to prevous sibling + designated distance
                        if (!node.IsLeftMost())
                            node.X = node.GetPreviousSibling().X + nodeSize + siblingDistance;
                        else
                            // if this is the first node in a set, set X to 0
                            node.X = 0;
                    }
                    // if there is only one child
                    else if (node.Children.Count == 1)
                    {
                        // if this is the first node in a set, set it's X value equal to it's child's X value
                        if (node.IsLeftMost())
                        {
                            node.X = node.Children[0].X;
                        }
                        else
                        {
                            node.X = node.GetPreviousSibling().X + nodeSize + siblingDistance;
                            node.Mod = node.X - node.Children[0].X;
                        }
                    }
                    else
                    {
                        var leftChild = node.GetLeftMostChild();
                        var rightChild = node.GetRightMostChild();
                        var mid = (leftChild.X + rightChild.X) / 2;

                        if (node.IsLeftMost())
                        {
                            node.X = mid;
                        }
                        else
                        {
                            node.X = node.GetPreviousSibling().X + nodeSize + siblingDistance;
                            node.Mod = node.X - mid;
                        }
                    }

                    if (node.Children.Count > 0 && !node.IsLeftMost())
                    {
                        // Since subtrees can overlap, check for conflicts and shift tree right if needed
                        CheckForConflicts(node);
                    }
                }
            }

            private static void CheckForConflicts(Node node)
            {
                var minDistance = treeDistance + nodeSize;
                var shiftValue = 0F;

                var nodeContour = new Dictionary<int, float>();
                GetLeftContour(node, 0, ref nodeContour);

                var sibling = node.GetLeftMostSibling();
                while (sibling != null && sibling != node)
                {
                    var siblingContour = new Dictionary<int, float>();
                    GetRightContour(sibling, 0, ref siblingContour);

                    for (int level = node.Y + 1; level <= Mathf.Min(siblingContour.Keys.Max(), nodeContour.Keys.Max()); level++)
                    {
                        var distance = nodeContour[level] - siblingContour[level];
                        if (distance + shiftValue < minDistance)
                        {
                            shiftValue = minDistance - distance;
                        }
                    }

                    if (shiftValue > 0)
                    {
                        node.X += shiftValue;
                        node.Mod += shiftValue;

                        CenterNodesBetween(node, sibling);

                        shiftValue = 0;
                    }

                    sibling = sibling.GetNextSibling();
                }
            }

            private static void CenterNodesBetween(Node leftNode, Node rightNode)
            {
                var leftIndex = leftNode.Parent.Children.IndexOf(rightNode);
                var rightIndex = leftNode.Parent.Children.IndexOf(leftNode);

                var numNodesBetween = (rightIndex - leftIndex) - 1;

                if (numNodesBetween > 0)
                {
                    var distanceBetweenNodes = (leftNode.X - rightNode.X) / (numNodesBetween + 1);

                    int count = 1;
                    for (int i = leftIndex + 1; i < rightIndex; i++)
                    {
                        var middleNode = leftNode.Parent.Children[i];

                        var desiredX = rightNode.X + (distanceBetweenNodes * count);
                        var offset = desiredX - middleNode.X;
                        middleNode.X += offset;
                        middleNode.Mod += offset;

                        count++;
                    }

                    CheckForConflicts(leftNode);
                }
            }

            private static void CheckAllChildrenOnScreen(Node node)
            {
                var nodeContour = new Dictionary<int, float>();
                GetLeftContour(node, 0, ref nodeContour);

                float shiftAmount = 0;
                foreach (var y in nodeContour.Keys)
                {
                    if (nodeContour[y] + shiftAmount < 0)
                        shiftAmount = (nodeContour[y] * -1);
                }

                if (shiftAmount > 0)
                {
                    node.X += shiftAmount;
                    node.Mod += shiftAmount;
                }
            }

            private static void GetLeftContour(Node node, float modSum, ref Dictionary<int, float> values)
            {
                if (!values.ContainsKey(node.Y))
                    values.Add(node.Y, node.X + modSum);
                else
                    values[node.Y] = Mathf.Min(values[node.Y], node.X + modSum);

                modSum += node.Mod;
                foreach (var child in node.Children)
                {
                    GetLeftContour(child, modSum, ref values);
                }
            }

            private static void GetRightContour(Node node, float modSum, ref Dictionary<int, float> values)
            {
                if (!values.ContainsKey(node.Y))
                    values.Add(node.Y, node.X + modSum);
                else
                    values[node.Y] = Mathf.Max(values[node.Y], node.X + modSum);

                modSum += node.Mod;
                foreach (var child in node.Children)
                {
                    GetRightContour(child, modSum, ref values);
                }
            }
        }
    }
}