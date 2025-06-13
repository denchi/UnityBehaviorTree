using System.Collections.Generic;

namespace Behaviours.BT.Runtime
{
    public static class BTLog
    {
        private static Dictionary<Task, List<object[]>> logsDict = new Dictionary<Task, List<object[]>>();

        public static void Clear()
        {
            logsDict.Clear();
        }

        public static void Do(Task task, System.Action<string, float, int> action)
        {
            if (logsDict.ContainsKey(task))
            {
                List<object[]> list = logsDict[task];
                for (int i = 0, n = list.Count; i < n; i++)
                {
                    object[] arr = list[i];
                    action.Invoke( (string) arr[0], (float) arr[1], i );
                }
            }
        }

        public static void Log(Task state, string text)
        {
            if (!logsDict.ContainsKey(state))
            {
                logsDict[state] = new List<object[]>();
            }

            logsDict[state].Add(new object[] { text, 0.0f });
        }

        public static void Update(float dt)
        {
            foreach (var kvp in logsDict)
            {
                for (int i = 0, n = kvp.Value.Count; i < n; i++)
                {
                    float t = (float) kvp.Value[i][1];
                    t += dt;
                    kvp.Value[i][1] = t;
                }

                kvp.Value.RemoveAll(x => (float) x[1] > 1);
            }
        }
    }
}