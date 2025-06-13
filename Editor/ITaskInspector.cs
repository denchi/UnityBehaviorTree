using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Behaviours.BT.Task), true)]
class ITaskInspector : OdinEditor
{
    Behaviours.BT.Task task { get { return target as Behaviours.BT.Task; } }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.FlexibleSpace();
        for (int i = 0; i < task.node.services.Count; i++)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            task.node.services[i]._editorOnlyExpanded = EditorGUILayout.Foldout(task.node.services[i]._editorOnlyExpanded, task.node.services[i].GetType().Name);
            if (task.node.services[i]._editorOnlyExpanded)
            {
                Editor editor = Editor.CreateEditor(task.node.services[i]);
                editor.OnInspectorGUI();
            }
            EditorGUILayout.EndVertical();
        }

        GUILayout.Space(20);
    }
}
