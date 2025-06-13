using UnityEditor;

[CustomEditor(typeof(Behaviours.BT.Tree), true)]
class TreeInspector : Editor
{
    Behaviours.BT.Tree tree { get { return target as Behaviours.BT.Tree; } }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //EditorGUILayout.HelpBox("List of sub assets:", MessageType.Info);

        //string assetPath = AssetDatabase.GetAssetPath(tree);

        //UnityEngine.Object[] obj = AssetDatabase.LoadAllAssetsAtPath(assetPath);

        //int indx = -1;
        //for (int i = 0; i < obj.Length; i++)
        //{
        //    EditorGUILayout.BeginHorizontal(GUI.skin.box);
        //    GUILayout.Label(obj[i].GetType().Name);
        //    if (GUILayout.Button("Remove", GUILayout.Width(80)))
        //    {
        //        indx = i;
        //    }
        //    EditorGUILayout.EndHorizontal();
        //}

        //if (indx!=-1)
        //{
        //    if (EditorUtility.DisplayDialog("Delete object?", "Do you really want to delete object " + obj[indx].GetType().Name + " ?", "Yes", "No"))
        //    {
        //        Object.DestroyImmediate(obj[indx], true);
        //    }
        //}

        //GUILayout.Space(20);
    }
}
