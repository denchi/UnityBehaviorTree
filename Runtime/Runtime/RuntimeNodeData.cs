using System.Collections.Generic;
using UnityEngine;

namespace Behaviours.BT.Runtime
{
    public class RuntimeNodeData
    {
        public Node associatedNode;
        public RuntimeTreeData runtimeTree;
        public RuntimeTaskData runtimeTask;
        public List<RuntimeServiceData> runtimeServices;

        [HideInInspector]
        [System.NonSerialized]
        public bool _editorOnlyTriggerred = false;

        [HideInInspector]
        [System.NonSerialized]
        public bool _editorOnlyRunning = false;

        [HideInInspector]
        [System.NonSerialized]
        public float _editorTimeMark;

        [HideInInspector]
        [System.NonSerialized]
        public State _editorReturnedState;

        public void init(RuntimeTreeData runtimeTree, Node node)
        {
            this.associatedNode = node;
            this.runtimeTree = runtimeTree;

            if (node.task)
            {
                this.runtimeTask = node.task.createRuntimeTaskData(this);
            }

            if (node.services != null)
            {
                runtimeServices = new List<RuntimeServiceData>();
                for (int i = 0, n = node.services.Count; i < n; i++)
                {
                    var service = node.services[i];
                    var runtimeService = service.createRuntimeServiceData(this);
                    runtimeServices.Add(runtimeService);
                }
            }
        }

        public void start()
        {
#if UNITY_EDITOR
            _editorOnlyRunning = true;
#endif

            if (runtimeTask != null)
            {
                runtimeTask.associatedTask.start(runtimeTask);
            }

            if (runtimeServices != null)
            {
                for (int i = 0, n = runtimeServices.Count; i < n; i++)
                {
                    var runtimeService = runtimeServices[i];
                    runtimeService.associatedService.onServiceStarted(runtimeService);
                }
            }
        }

        public void broadcastTickServicesExcept(IService service)
        {
            if (runtimeServices != null)
            {
                for (int i = 0, n = runtimeServices.Count; i < n; i++)
                {
                    var runtimeService = runtimeServices[i];

                    if (runtimeService.associatedService != service)
                        runtimeService.associatedService.tick(runtimeService);
                }
            }
        }

        public State update(float dt)
        {
            if (runtimeTask != null)
            {
                State result = runtimeTask.associatedTask.update(runtimeTask, dt);

                if (result != State.Running)
                    return result;
            }

            if (runtimeServices != null)
            {
                for (int i = 0, n = runtimeServices.Count; i < n; i++)
                {
                    var runtimeService = runtimeServices[i];

                    // update elapsed time
                    runtimeService._tickElapsedTime += dt;

                    // check time to tick
                    if (runtimeService._tickElapsedTime >= runtimeService._tickTotalTime)
                    {
                        // reset timer
                        runtimeService._tickElapsedTime = 0;

                        // tick
                        var result = runtimeService.associatedService.tick(runtimeService);

                        if (result != State.Running)
                            return result;
                    }
                }
            }

            return State.Running;
        }

        public void end()
        {
            if (runtimeServices != null)
            {
                for (int i = 0, n = runtimeServices.Count; i < n; i++)
                {
                    var runtimeService = runtimeServices[i];
                    runtimeService.associatedService.onServiceEnded(runtimeService);
                }
            }

            if (runtimeTask != null)
            {
                runtimeTask.associatedTask.end(runtimeTask);
            }

#if UNITY_EDITOR
            _editorOnlyRunning = false;
            _editorOnlyTriggerred = true;
#endif
        }
    }
}