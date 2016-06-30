using UnityEngine;
using System.Collections;
using UnityEditor;

public class MatchInspenctorTool
{

    public static void OnGUIMacth(SerializedProperty property)
    {


        SerializedProperty startTime = property.FindPropertyRelative("startTime");
        EditorGUILayout.PropertyField(startTime, new GUIContent("    startTime"));

        SerializedProperty endTime = property.FindPropertyRelative("endTime");
        EditorGUILayout.PropertyField(endTime, new GUIContent("    endTime"));

        SerializedProperty runTime = property.FindPropertyRelative("runTime");
        EditorGUILayout.PropertyField(runTime, new GUIContent("    runTime"));

        SerializedProperty speedCurve = property.FindPropertyRelative("speedCurve");
        EditorGUILayout.PropertyField(speedCurve, new GUIContent("    speedCurve"));

        SerializedProperty type = property.FindPropertyRelative("type");
        EditorGUILayout.PropertyField(type, new GUIContent("    type"));
        if (type.enumValueIndex == 0)
        {
            SerializedProperty distance = property.FindPropertyRelative("distance");
            EditorGUILayout.PropertyField(distance, new GUIContent("     distance"));
        }
        else if (type.enumValueIndex == 1)
        {
            SerializedProperty xCurve = property.FindPropertyRelative("xCurve");
            EditorGUILayout.PropertyField(xCurve, new GUIContent("     xCurve"));

            SerializedProperty yCurve = property.FindPropertyRelative("yCurve");
            EditorGUILayout.PropertyField(yCurve, new GUIContent("     yCurve"));

            SerializedProperty zCurve = property.FindPropertyRelative("zCurve");
            EditorGUILayout.PropertyField(zCurve, new GUIContent("     zCurve"));
        }
    }
    public static int OnGUIFeedback(SerializedProperty damageCount)
    {
        GUILayout.BeginHorizontal(GUILayout.Width(150));
        if (GUILayout.Button("ADDDC", GUILayout.Width(100)))
        {
            damageCount.InsertArrayElementAtIndex(damageCount.arraySize);
        }
        GUILayout.Label("伤害次数：" + damageCount.arraySize);
        GUILayout.EndHorizontal();
        for (int i = 0; i < damageCount.arraySize; i++)
        {

            SerializedProperty damage = damageCount.GetArrayElementAtIndex(i);
            //SerializedProperty reflectName = damage.FindPropertyRelative("reflectName");
            //EditorGUILayout.PropertyField(reflectName, new GUIContent("    reflectName"));

            //SerializedProperty reflectStartTime = damage.FindPropertyRelative("reflectStartTime");
            //EditorGUILayout.PropertyField(reflectStartTime, new GUIContent("    reflectStartTime"));

            SerializedProperty damageTime = damage.FindPropertyRelative("damageTime");
            EditorGUILayout.PropertyField(damageTime, new GUIContent("    damageTime"));

            //SerializedProperty isOpenMatchEff = damage.FindPropertyRelative("isOpenMatchEff");
            //EditorGUILayout.PropertyField(isOpenMatchEff, new GUIContent("    isOpenMatchEff"));

            //if (isOpenMatchEff.boolValue)
            //{
            //    MatchInspenctorTool.OnGUIMacth(damage.FindPropertyRelative("matchEff"));
            //}

            if (GUILayout.Button("DeleDC", GUILayout.Width(100)))
            {
                damageCount.DeleteArrayElementAtIndex(i);
            }
            GUILayout.Space(5);
        }
        return damageCount.arraySize;
    }
}
