using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System;
    
[CustomPropertyDrawer(typeof(InterfaceOutletAttribute))]
public class InterfaceOutletDrawer : PropertyDrawer {
	float rows = 2;
	const float minRows = 2;
	float extraHeight = 0;

	public override void OnGUI (Rect pos, SerializedProperty properties, GUIContent label) {
//		That's how you wold get the property type without passing it to the annotation
//		Debug.Log (properties.serializedObject.targetObject.GetType().GetMember(properties.name)[0].ReflectedType);
//		Unfortunatelly, Unity will only call this custom property drawer if it's type is serializable and interfaces are NOT serializable
        Type interfaceType = getAttribute().interfaceType;

        // pass through label
        label.text = "["+interfaceType.Name+"]"+label.text+":";
        EditorGUI.LabelField(
			new Rect (pos.x, pos.y, pos.width, pos.height/rows),
			label
		);
        EditorGUI.indentLevel += 2;
        UnityEngine.Object tempObj = null;
        tempObj = EditorGUI.ObjectField(new Rect(pos.x, pos.y += pos.height / rows, pos.width, pos.height / rows), properties.objectReferenceValue, typeof(UnityEngine.Object), true);
        if(tempObj is GameObject)
        {
            var gameobject = tempObj as GameObject;
            var monoBehaviourList = gameobject.GetComponents<MonoBehaviour>();
            MonoBehaviour selectedScript = null;
            foreach (MonoBehaviour mb in monoBehaviourList)
            {
                if (mb.GetType().GetInterface(interfaceType.Name) != null)
                {
                    selectedScript = mb;
                    break;
                }
            }
            if (selectedScript != null)
            {
                properties.objectReferenceValue = selectedScript;
            } else {
                properties.objectReferenceValue = null;
            }
        }
        else if(tempObj != null) {
            if (tempObj.GetType().GetInterface(interfaceType.Name) != null)
            {
                properties.objectReferenceValue = tempObj;
            }else
            {
                properties.objectReferenceValue = null;
            }
        }else
        {
            properties.objectReferenceValue = null;
        }
        EditorGUI.indentLevel--;

    }

    public InterfaceOutletAttribute getAttribute(){
		return (InterfaceOutletAttribute)attribute;
	}
			
	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return base.GetPropertyHeight (property, label) * rows+ extraHeight + 5;
	}
}