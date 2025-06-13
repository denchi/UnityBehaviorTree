using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Behaviours.BT
{
    [CustomPropertyDrawer(typeof(BTVarAttribute))]
    public class BTVarDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // First get the attribute since it contains the range for the slider
            BTVarAttribute attr = attribute as BTVarAttribute;

            if (property.propertyType == SerializedPropertyType.String)
            {
                List<int> codes;
                List<string> names = getValueNames(attr.type, out codes);
                
                int oldValueIndex = names.IndexOf(property.stringValue);
                int newValueIndex = EditorGUI.Popup(position, label.text + " (" + attr.type.ToString() + ")", oldValueIndex, names.ToArray());
                if (oldValueIndex != newValueIndex)
                {
                    if (newValueIndex >= 0 && newValueIndex < names.Count)
                    {
                        property.stringValue = names[newValueIndex];
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            if (property.propertyType == SerializedPropertyType.Integer)
            {
                List<int> codes;
                List<string> names = getValueNames(attr.type, out codes);

                int oldValueIndex = codes.IndexOf(property.intValue);
                int newValueIndex = EditorGUI.Popup(position, label.text + " (" + attr.type.ToString() + ")", oldValueIndex, names.ToArray());
                if (oldValueIndex != newValueIndex)
                {
                    if (newValueIndex >= 0 && newValueIndex < names.Count)
                    {
                        property.intValue = codes[newValueIndex];
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
            }
        }

        public static List<string> getValueNames(ValueType type, out List<int> codes)
        {
            List<string> names = new List<string>();
            codes = new List<int>();
            {
                Editor.BTWindow window = Editor.BTWindow.GetWindow<Editor.BTWindow>();
                if (window)
                {
                    if (window.tree)
                    {
                        foreach (Value x in window.tree.values)
                        {
                            if (x.type == type)
                            {
                                names.Add(x.name);
                                codes.Add(x.name.GetHashCode());
                            }
                        }
                    }
                }
            }
            return names;
        }
    }    
}