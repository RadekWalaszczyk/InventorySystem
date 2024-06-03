using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

[CustomEditor(typeof(DatabaseItem))]
public class DatabaseItemEditor : Editor
{
    #region Properties

    SerializedProperty Name;
    SerializedProperty Icon;
    SerializedProperty Prefab;
    SerializedProperty Mesh;
    SerializedProperty Material;
    SerializedProperty OnUse;

    #endregion

    GenericMenu menu = new GenericMenu();

    private void OnEnable()
    {
        Name = serializedObject.FindProperty("Name");
        Icon = serializedObject.FindProperty("Icon");
        Prefab = serializedObject.FindProperty("Prefab");
        Mesh = serializedObject.FindProperty("Mesh");
        Material = serializedObject.FindProperty("Material");   
        OnUse = serializedObject.FindProperty("OnUse");
    }

    public override void OnInspectorGUI()
    {
        DatabaseItem item = (DatabaseItem)target;

        serializedObject.Update();

        EditorGUILayout.PropertyField(Name);
        EditorGUILayout.PropertyField(Icon);

        EditorGUILayout.Space(2);
        EditorGUILayout.PropertyField(Prefab);
        EditorGUILayout.PropertyField(Mesh);
        EditorGUILayout.PropertyField(Material);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("____ On Use Effects ____");

        EditorGUILayout.Space(10);
        if (EditorGUILayout.DropdownButton(new GUIContent("Add New Effect..."), FocusType.Keyboard))
        {
            var effects = Assembly.GetAssembly(typeof(ItemUsable)).GetTypes().Where(x => x.IsClass && x.IsSubclassOf(typeof(ItemUsable)));
            foreach (var effect in effects)
            {
                var instance = (ItemUsable)Activator.CreateInstance(effect);
                instance.name = $"{effect}";
                menu.AddItem(new GUIContent($"{effect}"), false, () => item.OnUse.Add(instance));
            }
             
            menu.ShowAsContext();
        }

        EditorGUILayout.PropertyField(OnUse);

        serializedObject.ApplyModifiedProperties();
    }
}
