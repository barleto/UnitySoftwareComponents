using UnityEngine;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

[AttributeUsage(AttributeTargets.Field)]
public class InterfaceOutletAttribute : PropertyAttribute{

    public Type interfaceType;
	public bool allowSceneObjects = true;

	public InterfaceOutletAttribute(Type interfaceType){
        this.interfaceType = interfaceType;
	}

}

public static class UnityEngineObjectExtension
{
	public static T As<T> (this UnityEngine.Object obj) where T : class
	{
		return obj as T;
	}
}
