using UnityEngine;
using System.Collections;
using UnityEditor;
[CustomEditor(typeof(CameraAnimationState))]
public class CameraAnimationPathInspector : Editor
{

    public override void OnInspectorGUI()
    {
        SerializedProperty effs = serializedObject.FindProperty("info");
        if (GUILayout.Button("Add Item"))
        {
            effs.InsertArrayElementAtIndex(effs.arraySize);
        }
        for (int i = 0; i < effs.arraySize; i++)
        {
            SerializedProperty info = effs.GetArrayElementAtIndex(i);
            GUIDrew(info);
            if (GUILayout.Button("Delete"))
            {
                effs.DeleteArrayElementAtIndex(i);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
    void GUIDrew(SerializedProperty effs)
    {
        SerializedProperty startTime = effs.FindPropertyRelative("startTime");
        EditorGUILayout.PropertyField(startTime);
        SerializedProperty triggerType = effs.FindPropertyRelative("triggerType");
        EditorGUILayout.PropertyField(triggerType);

        SerializedProperty type = effs.FindPropertyRelative("type");
        EditorGUILayout.PropertyField(type);
        if (type.enumValueIndex == 0)
        {
            //SerializedProperty animClip = effs.FindPropertyRelative("animClip");
            //EditorGUILayout.PropertyField(animClip);
            SerializedProperty camreAnimaName = effs.FindPropertyRelative("camreAnimaName");
            EditorGUILayout.PropertyField(camreAnimaName);
            SerializedProperty clipLength = effs.FindPropertyRelative("clipLength");
            EditorGUILayout.PropertyField(clipLength);



            SerializedProperty isFuse = effs.FindPropertyRelative("isStartFuse");
            EditorGUILayout.PropertyField(isFuse);
            if (isFuse.boolValue)
            {
                SerializedProperty fuseTime_s = effs.FindPropertyRelative("startFuseTime_s");
                EditorGUILayout.PropertyField(fuseTime_s);
                if (fuseTime_s.floatValue <= 0)
                {

                    fuseTime_s.floatValue = 0.1f;
                }
            }
            SerializedProperty isAnimEndRestCameraPos = effs.FindPropertyRelative("isAnimEndRestCameraPos");
            EditorGUILayout.PropertyField(isAnimEndRestCameraPos);

            //SerializedProperty isClipEndRestPos = effs.FindPropertyRelative("isClipEndRestPos");
            //EditorGUILayout.PropertyField(isClipEndRestPos);
            //if (isClipEndRestPos.boolValue)
            //{
            //    SerializedProperty endPosSpeedCurve = effs.FindPropertyRelative("endPosSpeedCurve");
            //    EditorGUILayout.PropertyField(endPosSpeedCurve);

            //    SerializedProperty backTime = effs.FindPropertyRelative("backTime");
            //    EditorGUILayout.PropertyField(backTime);
            //}

        }
        else if (type.enumValueIndex == 1)
        {
            SerializedProperty followType = effs.FindPropertyRelative("followType");
            EditorGUILayout.PropertyField(followType);
        }
        else if (type.enumValueIndex == 2)
        {
            SerializedProperty bulletEndTime = effs.FindPropertyRelative("bulletEndTime");
            EditorGUILayout.PropertyField(bulletEndTime);

            SerializedProperty bulletCurve = effs.FindPropertyRelative("bulletCurve");
            EditorGUILayout.PropertyField(bulletCurve);
        }
        else if (type.enumValueIndex == 3)
        {
            SerializedProperty balckTime = effs.FindPropertyRelative("balckTime");
            EditorGUILayout.PropertyField(balckTime);

            SerializedProperty balckFadeOut = effs.FindPropertyRelative("balckFadeOut");
            EditorGUILayout.PropertyField(balckFadeOut);

            SerializedProperty backBg = effs.FindPropertyRelative("backBgName");
            EditorGUILayout.PropertyField(backBg);
            SerializedProperty textureAlpha = effs.FindPropertyRelative("textureAlpha");
            EditorGUILayout.PropertyField(textureAlpha);

        }
        else if (type.enumValueIndex == 4)
        {
            SerializedProperty textrueName = effs.FindPropertyRelative("textrueName");
            EditorGUILayout.PropertyField(textrueName);
            SerializedProperty continueTime = effs.FindPropertyRelative("continueTime");
            EditorGUILayout.PropertyField(continueTime);

        }
        else if (type.enumValueIndex == 5)
        {
            SerializedProperty isCustomValue = effs.FindPropertyRelative("isCustomValue");
            EditorGUILayout.PropertyField(isCustomValue);
            SerializedProperty shakeType = effs.FindPropertyRelative("shakeType");
            EditorGUILayout.PropertyField(shakeType);
            if (isCustomValue.boolValue)
            {
                SerializedProperty shakeTime = effs.FindPropertyRelative("shakeTime");
                EditorGUILayout.PropertyField(shakeTime);
                SerializedProperty shakeWeight = effs.FindPropertyRelative("shakeWeight");
                EditorGUILayout.PropertyField(shakeWeight);
                SerializedProperty shakeSpeed = effs.FindPropertyRelative("shakeSpeed");
                EditorGUILayout.PropertyField(shakeSpeed);
            }

        }
        else if (type.enumValueIndex == 6)
        {
            SerializedProperty isCustomValue = effs.FindPropertyRelative("isCustomValue");
            EditorGUILayout.PropertyField(isCustomValue);
            SerializedProperty isZoomIn = effs.FindPropertyRelative("isZoomIn");
            EditorGUILayout.PropertyField(isZoomIn);
            if (isCustomValue.boolValue)
            {
                SerializedProperty zoomValue = effs.FindPropertyRelative("zoomValue");
                EditorGUILayout.PropertyField(zoomValue);
                SerializedProperty zoomTime = effs.FindPropertyRelative("zoomTime");
                EditorGUILayout.PropertyField(zoomTime);
            }

        }
    }
}
