using System;
using System.Linq;
using System.Reflection;
using Scripts;
using UnityEditor;
using UnityEngine;

namespace Editor.Scripts
{
    [CustomEditor(typeof(TransitionManager))]
    public class TransitionManagerEditor : UnityEditor.Editor
    {
        private bool _foldout = true;

        public override void OnInspectorGUI()
        {
            var transitionManager = target as TransitionManager;

            if (transitionManager == null)
            {
                base.OnInspectorGUI();
                return;
            }

            serializedObject.Update();
            EditorGUILayout.ObjectField(serializedObject.FindProperty("_mainCamera"));
            EditorGUILayout.ObjectField(serializedObject.FindProperty("_leftEyeTransform"));
            EditorGUILayout.ObjectField(serializedObject.FindProperty("_rightEyeTransform"));

            _foldout = EditorGUILayout.Foldout(_foldout, "Transitions");
            if (_foldout)
            {
                foreach (Type type in typeof(Transition).Assembly.GetTypes()
                             .Where(t => t.IsSubclassOf(typeof(Transition))))
                {
                    if (GUILayout.Button("Add " + type.Name))
                    {
                        transitionManager.Transitions.Add(Activator.CreateInstance(type) as Transition);
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                EditorGUILayout.Separator();

                EditorGUILayout.BeginVertical();
                var t = serializedObject.FindProperty("Transitions");
                for (int i = 0; i < t.arraySize; i++)
                {
                    var element = t.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(element, new GUIContent(element.managedReferenceValue.GetType().Name),
                        true);
                    if (GUILayout.Button("-"))
                    {
                        transitionManager.Transitions.Remove(element.managedReferenceValue as Transition);
                    }
                }

                EditorGUILayout.EndVertical();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}