using UnityEngine;
using System.Collections;

[RequireComponent (typeof (vsaa.Communicator))]
public class VerySimpleTester : MonoBehaviour {

	vsaa.Communicator Vsaa;

	string eventDescription = "Generic event";
	
	void Start () {
		if (Vsaa == null)
			Vsaa = this.GetComponent<vsaa.Communicator>();
	}
	
	void OnGUI() {
		eventDescription = GUILayout.TextField(eventDescription, GUILayout.Width(300));
		if(GUILayout.Button("Send", GUILayout.Width(300)))
		{
			Vsaa.SendGenericEvent(eventDescription);
		}
		GUILayout.Label(Vsaa.platformStatus,GUILayout.Width(300));
	}
}
