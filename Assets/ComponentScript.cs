using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class ComponentScript : MonoBehaviour, ComponentOutletInterface
{
    public void interfaceMethod()
    {
        Debug.Log("From component named '"+this.name+"':\nComponent Called!");
    }

    public void setLabeltext(Text textlabel)
    {
        textlabel.text = "Component's message.";
    }
}
