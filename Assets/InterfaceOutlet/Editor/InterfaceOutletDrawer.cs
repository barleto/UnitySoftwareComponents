using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System;




[CustomPropertyDrawer(typeof(InterfaceOutletAttribute))]
public class InterfaceOutletDrawer : PropertyDrawer {

	const int LABL_MARGIN = 2;
	private static GUIStyle _pathBackgroundStyle = new GUIStyle(EditorStyles.textField);
	private static GUIStyle _objectFieldDropWellButtonStyle = GetStyle("ObjectFieldButton");
	private int _pickerControlID;

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		//		That's how you wold get the property type without passing it to the annotation
		//		Debug.Log (properties.serializedObject.targetObject.GetType().GetMember(properties.name)[0].ReflectedType);
		//		Unfortunatelly, Unity will only call this custom property drawer if it's type is serializable and interfaces are NOT serializable
		var attribute = getAttribute();
		Type interfaceType = attribute.interfaceType;
		EditorGUI.BeginProperty(position, label, property);

		EditorGUI.PrefixLabel(position, label);
		position.x += EditorGUIUtility.labelWidth + LABL_MARGIN;
		position.width -= EditorGUIUtility.labelWidth + LABL_MARGIN;
		DrawField(position, property, interfaceType, attribute);
		EditorGUI.EndProperty();
	}

	private void DrawField (Rect position, SerializedProperty property, Type interfaceType, InterfaceOutletAttribute attribute)
	{
		var controlId = GUIUtility.GetControlID(FocusType.Keyboard);
		var ev = Event.current;
		var dropWellButtonPos = new Rect(position.xMax - 20, position.y + 2, 19, position.height - 4);
		switch (ev.type)
		{
			case EventType.MouseDown:
				if (dropWellButtonPos.Contains(Event.current.mousePosition))
				{
					this._pickerControlID = GUIUtility.GetControlID(FocusType.Passive);
					EditorGUIUtility.ShowObjectPicker<UnityEngine.Object>(property.objectReferenceValue, attribute.allowSceneObjects, "t:GameObject t:ScriptableObject", this._pickerControlID);
				}
				else if (position.Contains(Event.current.mousePosition))
				{
					GUIUtility.keyboardControl = controlId;
					Event.current.Use();
				}
				break;
			case EventType.KeyUp:
				if (GUIUtility.keyboardControl == controlId && (Event.current.keyCode == KeyCode.Backspace || Event.current.keyCode == KeyCode.Delete))
				{
					ClearSerializedProperty(property);
					Event.current.Use();
				}
				break;
			case EventType.Repaint:
				var backgroundStyle = _pathBackgroundStyle;
				if (position.Contains(ev.mousePosition) && IsDraggingInterfaceWithin(interfaceType))
				{
					//Highlight object field if the user us dragging the correct asset on top of the field
					backgroundStyle.Draw(position, new GUIContent(), true, false, true, false);

				}
				else if (GUIUtility.keyboardControl != controlId)
				{
					backgroundStyle.Draw(position, new GUIContent(), controlId, false, position.Contains(Event.current.mousePosition));
				}
				else
				{
					backgroundStyle.Draw(position, new GUIContent(), false, true, false, true);
				}

				var textStyle = new GUIStyle(EditorStyles.label);
				textStyle.padding.left += 2;
				if (EditorGUI.showMixedValue)
				{
					//Needs to call the mormal text field here, or else it's not possible to show the correct "---" string for multi editing different values
					EditorGUI.TextField(position, "---");
				}
				else
				{
					var content = property.objectReferenceValue == null ? 
						new GUIContent($"None ({interfaceType.Name})") :
						EditorGUIUtility.ObjectContent(property.objectReferenceValue, property.objectReferenceValue.GetType());
					textStyle.Draw(position, content, false, false, false, false);
				}

				var isHovering = dropWellButtonPos.Contains(Event.current.mousePosition);
				_objectFieldDropWellButtonStyle.Draw(dropWellButtonPos, isHovering, isActive: false, false, false);
				break;
			case EventType.DragUpdated:
				if (position.Contains(ev.mousePosition))
				{
					if (IsDraggingInterfaceWithin(interfaceType))
					{
						DragAndDrop.visualMode = DragAndDropVisualMode.Move;
						DragAndDrop.AcceptDrag();
						ev.Use();
					}
				}
				break;
			case EventType.DragPerform:
				if (position.Contains(ev.mousePosition))
				{
					if (IsDraggingInterfaceWithin(interfaceType))
					{
						property.objectReferenceValue = GetObjectWithInterface(DragAndDrop.objectReferences[0], interfaceType);
						property.serializedObject.ApplyModifiedProperties();
						GUI.changed = true;
						ev.Use();
					}
				}
				break;
			case EventType.ExecuteCommand:
				if (Event.current.commandName == "ObjectSelectorUpdated")
				{
					if (EditorGUIUtility.GetObjectPickerControlID() == this._pickerControlID)
					{
						if (CheckIfObjectHasInterface(EditorGUIUtility.GetObjectPickerObject(), interfaceType))
						{
							property.objectReferenceValue = GetObjectWithInterface(EditorGUIUtility.GetObjectPickerObject(), interfaceType);
						}
						else
						{
							EditorWindow.focusedWindow.ShowNotification(new GUIContent(
								$"'{EditorGUIUtility.GetObjectPickerObject().name}' does not implement interface {interfaceType.Name} within."
							), 1.5);
							property.objectReferenceValue = null;
						}
						property.serializedObject.ApplyModifiedProperties();
						GUI.changed = true;
						ev.Use();
					}
				}
				break;
		}
	}

	static GUIStyle GetStyle (string styleName)
	{
		GUIStyle gUIStyle = GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
		if (gUIStyle == null)
		{
			Debug.LogError("Missing built-in guistyle " + styleName);
			return null;
		}

		return gUIStyle;
	}

	private static void ClearSerializedProperty (SerializedProperty property)
	{
		property.objectReferenceValue = null;
	}

	private bool IsDraggingInterfaceWithin (Type interfaceType)
	{
		return DragAndDrop.objectReferences.Length > 0 &&
				DragAndDrop.objectReferences[0] != null &&
				CheckIfObjectHasInterface(DragAndDrop.objectReferences[0], interfaceType);
	}

	private bool CheckIfObjectHasInterface (UnityEngine.Object obj, Type interfaceType)
	{
		if (obj is GameObject)
		{
			var gameobject = obj as GameObject;
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
			
			return selectedScript != null;
		}
		else if (obj != null)
		{
			return obj.GetType().GetInterface(interfaceType.Name) != null;
		}
		else
		{
			return false;
		}
	}

	private UnityEngine.Object GetObjectWithInterface (UnityEngine.Object obj, Type interfaceType)
	{
		if (obj is GameObject)
		{
			var gameobject = obj as GameObject;
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

			return selectedScript;
		}
		else if (obj != null && obj.GetType().GetInterface(interfaceType.Name) != null)
		{
			return obj;
		}
		else
		{
			return null;
		}
	}

	public InterfaceOutletAttribute getAttribute(){
		return (InterfaceOutletAttribute)attribute;
	}
}
