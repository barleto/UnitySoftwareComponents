using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ObjectOutletTest : MonoBehaviour {

    [Header("EXAMPLE SCRIPT:")]
    [Header("Below is an interface outlet.")]
    [Header("You must plug an object implementing")]
    [Header("the interface below for it to work.")]
    [Header("")]

    [InterfaceOutlet(typeof(ComponentOutletInterface))]
    public UnityEngine.Object component;
    public Text textLabel;

	// Use this for initialization
	void Start () {
        if (component != null) {
            ((ComponentOutletInterface)component).interfaceMethod();
            ((ComponentOutletInterface)component).setLabeltext(textLabel);
        }
        else
        {
            Debug.LogError("No component attached.");
            textLabel.text = "Component not set.";
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

public interface ComponentOutletInterface
{
    void interfaceMethod();
    void setLabeltext(Text textlabel);
}
