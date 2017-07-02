using UnityEngine;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Field)]
public class InterfaceOutletAttribute : PropertyAttribute{

    public Type interfaceType;

	public InterfaceOutletAttribute(Type interfaceType){
        this.interfaceType = interfaceType;
	}

}
