using UnityEngine;
using System.Collections;
using UnityEditor;
[CustomPropertyDrawer(typeof(AnimDeployMenu))]
public class AnimDeployMenuEditor : PropertyDrawer
{
    private AnimDeployMenu menu;
    private int count;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty root = property.FindPropertyRelative("deployOptions");
        //menu = (AnimDeployMenu)property.serializedObject.targetObject;
        position.width = 500;

        count = 0;
        SerializedProperty skillHurtCount = property.FindPropertyRelative("skillHurtCount");
        EditorGUILayout.PropertyField(skillHurtCount, new GUIContent("skillHurtCount"));
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("ADD", GUILayout.Width(position.width)))
        {
            root.InsertArrayElementAtIndex(root.arraySize);
        }
        GUILayout.EndHorizontal();

        SerializedProperty b = property.FindPropertyRelative("viewChild");
        b.boolValue = EditorGUILayout.Foldout(b.boolValue, "Options");

        if (b.boolValue)
        {
            for (int i = 0; i < root.arraySize; i++)
            {
                SerializedProperty pro = root.GetArrayElementAtIndex(i);
                SerializedProperty isZd = pro.FindPropertyRelative("isZd");
                SerializedProperty type = pro.FindPropertyRelative("type");
                DameCount(pro, type.enumValueIndex);
                isZd.boolValue = EditorGUILayout.Foldout(isZd.boolValue, ((AnimDeployOption.DeployType)type.enumValueIndex).ToString());
                if (!isZd.boolValue)
                    continue;
                EditorGUILayout.PropertyField(type, new GUIContent("Type"), GUILayout.Width(position.width));

                //SerializedProperty isDynamicPos = pro.FindPropertyRelative("isDynamicPos");

                GetType(position, pro, type.enumValueIndex);


                if (GUILayout.Button("Delete", GUILayout.Width(position.width)))
                {
                    root.DeleteArrayElementAtIndex(i);
                    return;
                }
                GUILayout.Label("======================================================================================================================================================", GUILayout.Width(position.width));
                GUILayout.Space(50);
            }
        }
        skillHurtCount.intValue = count;
    }
    private void DameCount(SerializedProperty pro, int type)
    {

        switch (type)
        {
            case 0:
                {
                    SerializedProperty isBullet = pro.FindPropertyRelative("isBullet");
                    if (isBullet.boolValue)
                    {
                        SerializedProperty damageCount = pro.FindPropertyRelative("damageCount");
                        count += damageCount.arraySize;
                    }
                }
                break;
            case 3:
                {
                    SerializedProperty damageCount = pro.FindPropertyRelative("damageCount");
                    count += damageCount.arraySize;
                }
                break;

        }
    }
    /// <summary>
    /// 选择特效类型
    /// </summary>
    private void GetType(Rect position, SerializedProperty pro, int type)
    {
        SerializedProperty eventTime = pro.FindPropertyRelative("eventTime");
        switch (type)
        {
            case 0:

                EditorGUILayout.PropertyField(eventTime, new GUIContent("eventTime%0-1"), GUILayout.Width(position.width));

                SerializedProperty effectName = pro.FindPropertyRelative("effectName");
                EditorGUILayout.PropertyField(effectName, new GUIContent("effectName", "The GameObject or Prefab that should be Instantiated."), GUILayout.Width(position.width));



                SerializedProperty isBullet = pro.FindPropertyRelative("isBullet");
                EditorGUILayout.PropertyField(isBullet, new GUIContent("isBullet", "The GameObject or Prefab that should be Instantiated."), GUILayout.Width(position.width));
                if (isBullet.boolValue)
                {
                    SerializedProperty bulletSpeed = pro.FindPropertyRelative("bulletSpeed");
                    EditorGUILayout.PropertyField(bulletSpeed, new GUIContent("bulletSpeed", "The GameObject or Prefab that should be Instantiated."), GUILayout.Width(position.width));

                    SerializedProperty configPosType = pro.FindPropertyRelative("configPosType");
                    EditorGUILayout.PropertyField(configPosType, new GUIContent("configPosType", "The GameObject or Prefab that should be Instantiated."), GUILayout.Width(position.width));

                    SerializedProperty isReflectEffect = pro.FindPropertyRelative("isReflectEffect");
                    EditorGUILayout.PropertyField(isReflectEffect, new GUIContent("isReflectEffect", "是否需要产生子弹碰撞特效"), GUILayout.Width(position.width));
                    //GetReflectType(pro);
                    if (isReflectEffect.boolValue)
                    {
                        SerializedProperty ReflectEffect = pro.FindPropertyRelative("ReflectEffect");
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("AddEff"))
                        {
                            ReflectEffect.InsertArrayElementAtIndex(ReflectEffect.arraySize);
                        }
                        if (GUILayout.Button("DeleEff"))
                        {
                            ReflectEffect.DeleteArrayElementAtIndex(ReflectEffect.arraySize - 1);
                        }
                        GUILayout.EndHorizontal();
                        //EditorGUILayout.PropertyField(ReflectEffect, new GUIContent("configPosType", "The GameObject or Prefab that should be Instantiated."), GUILayout.Width(position.width));
                        for (int i = 0; i < ReflectEffect.arraySize; i++)
                        {
                            EditorGUILayout.PropertyField(ReflectEffect.GetArrayElementAtIndex(i), new GUIContent("     effectName", "子弹爆炸的反馈特效"), GUILayout.Width(position.width));
                        }
                    }
                    SerializedProperty damageCount = pro.FindPropertyRelative("damageCount");
                    MatchInspenctorTool.OnGUIFeedback(damageCount);
                }
                //SerializedProperty target_00 = pro.FindPropertyRelative("targetPostion");
                //EditorGUILayout.PropertyField(target_00, new GUIContent("EffectTarget", "The position where the object should be Instatiated."), GUILayout.Width(position.width));

                break;

            case 1:

                EditorGUILayout.PropertyField(eventTime, new GUIContent("eventTime%0-1"), GUILayout.Width(position.width));
                SerializedProperty effectName1 = pro.FindPropertyRelative("effectName");
                EditorGUILayout.PropertyField(effectName1, new GUIContent("effectName", "The GameObject or Prefab that should be Instantiated."), GUILayout.Width(position.width));
                break;

            case 2:

                EditorGUILayout.PropertyField(eventTime, new GUIContent("eventTime%0-1"), GUILayout.Width(position.width));
                SerializedProperty audioCli = pro.FindPropertyRelative("audioName");
                EditorGUILayout.PropertyField(audioCli, new GUIContent("audioName", "音效名"), GUILayout.Width(position.width));

                break;

            case 3:
                {
                    SerializedProperty isDamageUpdatTime = pro.FindPropertyRelative("isDamageUpdatTime");
                    EditorGUILayout.PropertyField(isDamageUpdatTime, new GUIContent("isDamageUpdatTime"));
                    //SerializedProperty effectName2 = pro.FindPropertyRelative("effectName");
                    //EditorGUILayout.PropertyField(effectName2, new GUIContent("effectName", "The GameObject or Prefab that should be Instantiated."), GUILayout.Width(position.width));
                    //GetReflectType(pro);
                    SerializedProperty damageCount = pro.FindPropertyRelative("damageCount");
                    MatchInspenctorTool.OnGUIFeedback(damageCount);
                }
                break;
            case 4:
                EditorGUILayout.PropertyField(eventTime, new GUIContent("eventTime%0-1"), GUILayout.Width(position.width));
                SerializedProperty skillRecoverTime = pro.FindPropertyRelative("skillRecoverTime");
                EditorGUILayout.PropertyField(skillRecoverTime, new GUIContent("recoverTime"), GUILayout.Width(position.width));
                //GetCameraState(position, pro, cameraState.enumValueIndex);
                break;
            case 5:
                //EditorGUILayout.PropertyField(eventTime, new GUIContent("eventTime%0-1"), GUILayout.Width(position.width));
                SerializedProperty reflectStartTime = pro.FindPropertyRelative("reflectStartTime");
                EditorGUILayout.PropertyField(reflectStartTime, new GUIContent("reflectStartTime"), GUILayout.Width(position.width));
                SerializedProperty reflectAnimName = pro.FindPropertyRelative("reflectAnimName");
                EditorGUILayout.PropertyField(reflectAnimName, new GUIContent("reflectAnimName"), GUILayout.Width(position.width));
                //GetCameraState(position, pro, cameraState.enumValueIndex);
                break;
            default: break;
        }
    }
}
