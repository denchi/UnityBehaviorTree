using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Behaviours;

[CustomPropertyDrawer(typeof(BTAnimatorParamAttribute))]
public class BTAnimatorParamDrawer : PropertyDrawer
{
    BTAnimatorParamAttribute animatorParam { get { return ((BTAnimatorParamAttribute)attribute); } }

    // Here you can define the GUI for your property drawer. Called by Unity.
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        var attr = attribute as BTAnimatorParamAttribute;

        List<string> paramsList = new List<string>();
        paramsList.Add("<NONE>");

        if (FindAndAddValues(prop.serializedObject, ref paramsList, attr.type))
        {
            int selected = 0;

            string old = prop.stringValue;
            if (paramsList.Contains(old))
            {
                selected = paramsList.IndexOf(old);
            }

            float btnWidth = 30;
            Rect rectId = new Rect(position.x, position.y, position.width - btnWidth - 5, position.height);
            Rect rectBtn = new Rect(position.xMax - btnWidth, position.y, btnWidth, position.height);

            EditorGUI.BeginChangeCheck();
            {
                Color c = GUI.color;
                GUI.color = new Color(0, 0.8f, 1);
                selected = EditorGUI.Popup(rectId, label.text, selected, paramsList.ToArray());
                GUI.color = c;
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (selected <= 0)
                {
                    prop.stringValue = "";
                    prop.serializedObject.ApplyModifiedProperties();
                }
                else if (selected < paramsList.Count)
                {
                    prop.stringValue = paramsList[selected];
                    prop.serializedObject.ApplyModifiedProperties();
                }
            }
        }
        else
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, prop, label);
            if (EditorGUI.EndChangeCheck())
            {
                prop.serializedObject.ApplyModifiedProperties();
            }
        }       
    }

    bool FindAndAddValues(SerializedObject obj, ref List<string> values, ValueType type)
    {
        if (obj.targetObject is not Behaviours.BT.Task task)
            return false;
        
        if (!task || !task.node || !task.node.tree || !task.node.tree._editorOnlyGameObject) 
            return false;
        
        // TODO: Add check for the type of task, if it is not a BTAnimatorTask, return false.
        return false;
        
        // var animator = task.node.tree._editorOnlyGameObject.GetComponent<Behaviours.HFSM.Animator>();
        // if (!animator || !animator.layer) 
        //     return false;
        
        // foreach (Value value in animator.layer.values)
        // {
        //     if (value.type == type)
        //         values.Add(value.name);
        // }
        //
        // return true;
    }
}