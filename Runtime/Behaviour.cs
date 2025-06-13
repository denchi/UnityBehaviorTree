using UnityEngine;
using Behaviours.BT.Runtime;

namespace Behaviours.BT
{
    /// <summary>
    /// Component that runs A Behaviour Tree Asset
    /// </summary>
    public class Behaviour : MonoBehaviour, IBehaviourTreeProvider
    {
        public Tree tree;
        public bool playOnAwake = true;
        public RuntimeTreeData runtimeTree;

        private bool isRunning = false;
        private bool isInitialized = false;

        [HideInInspector]
        public bool isPaused = false;

        private void Awake()
        {
            if (playOnAwake)
            {
                SetTree(tree);
            }
        }

        // Update is called once per frame
        private void Update()
        {
            update(Time.deltaTime);
        }

        //

        public void SetTree(Tree newTree)
        {
            if (tree && ((tree != newTree) || (isInitialized == false)))
            {
                tree = newTree;

                isInitialized = false;

                runtimeTree = new RuntimeTreeData();
                runtimeTree.init(gameObject, tree);

                isInitialized = true;

                isRunning = false;
                isPaused = false;
            }
        }

        public void update(float dt)
        {
            if (isInitialized)
            {
                // if not started them start
                if (isRunning == false)
                {
                    runtimeTree.start();

                    isRunning = true;
                }
                else
                {
                    // update only if not paused
                    if (isPaused == false)
                    {
                        runtimeTree.update(dt);
                    }
                }
            }
        }

        public void pause()
        {
            if (isInitialized)
            {
                if (!isPaused)
                {
                    runtimeTree.end();
                    isPaused = true;
                }
            }
        }

        public void resume()
        {
            if (isInitialized)
            {
                if (isPaused)
                {
                    runtimeTree.start();
                    isPaused = false;
                }
            }
        }

        public void restart()
        {
            if (isInitialized)
            {
                isRunning = true;
                isPaused = false;

                runtimeTree.end();
                runtimeTree.start();
            }
        }        

        #region SETTER

        public void setFloat(string name, float value)
        {
            runtimeTree.runtimeValues.SetFloat(name, value);
        }

        public void setInt(string name, int value)
        {
            runtimeTree.runtimeValues.SetInt(name, value);
        }

        public void setBool(string name, bool value)
        {
            runtimeTree.runtimeValues.SetBool(name, value);
        }

        public void setString(string name, string value)
        {
            runtimeTree.runtimeValues.SetString(name, value);
        }

        public void setTrigger(string name)
        {
            runtimeTree.runtimeValues.SetTrigger(name, this);
        }

        #endregion

        #region GETTER

        public float getFloat(string name)
        {
            return runtimeTree.runtimeValues.GetFloat(name);
        }

        public int getInt(string name)
        {
            return runtimeTree.runtimeValues.GetInt(name);
        }

        public bool getBool(string name)
        {
            return runtimeTree.runtimeValues.GetBool(name);
        }

        public string getString(string name)
        {
            return runtimeTree.runtimeValues.GetString(name);
        }

        #endregion

        #region SETTER

        public void setFloat(int hashCode, float value)
        {
            runtimeTree.runtimeValues.SetFloat(hashCode, value);
        }

        public void setInt(int hashCode, int value)
        {
            runtimeTree.runtimeValues.SetInt(hashCode, value);
        }

        public void setBool(int hashCode, bool value)
        {
            runtimeTree.runtimeValues.SetBool(hashCode, value);
        }

        public void setString(int hashCode, string value)
        {
            runtimeTree.runtimeValues.SetString(hashCode, value);
        }

        public void setTrigger(int hashCode)
        {
            runtimeTree.runtimeValues.SetTrigger(hashCode, this);
        }

        #endregion

        #region GETTER

        public float getFloat(int hashCode)
        {
            return runtimeTree.runtimeValues.GetFloat(hashCode);
        }

        public int getInt(int hashCode)
        {
            return runtimeTree.runtimeValues.GetInt(hashCode);
        }

        public bool getBool(int hashCode)
        {
            return runtimeTree.runtimeValues.GetBool(hashCode);
        }

        public string getString(int hashCode)
        {
            return runtimeTree.runtimeValues.GetString(hashCode);
        }

        #endregion

        #region IBehaviour Implementation        

        private RuntimeNodeData FindRuntimeTaskDataForNode(Node node, RuntimeNodeData nodeData)
        {
            if (nodeData.associatedNode != null && nodeData.associatedNode == node)
            {
                return nodeData;
            }

            if (nodeData.runtimeTask is Composite.CompositeTaskData)
            {
                var comp = nodeData.runtimeTask as Composite.CompositeTaskData;

                for (int i = 0; i < comp.runtimeChildNodes.Count; ++i)
                {
                    var temp = FindRuntimeTaskDataForNode(node, comp.runtimeChildNodes[i]);
                    if (temp != null)
                        return temp;
                }
            }

            return null;
        }

        public int GetTreesCount()
        {
            return 1;
        }

        public RuntimeTreeData GetRuntimeTreeData(int index)
        {
            return runtimeTree;
        }

        public Tree GetTree(int index)
        {
            return tree;
        }
        
        public RuntimeNodeData FindRuntimeNodeDataForNode(int index, Node node)
        {
            return runtimeTree != null ? FindRuntimeTaskDataForNode(node, runtimeTree.runtimeNode) : null;
        }

        #endregion
    }
}