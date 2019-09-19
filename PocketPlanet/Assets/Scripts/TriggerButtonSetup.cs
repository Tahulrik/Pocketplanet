using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TriggerButtonSetup : MonoBehaviour {

	public delegate void MenuActivated(GameObject Self);

	public static event MenuActivated CloseOthers;


	public GameObject Panel;

	bool isActivated = false;
	Animator anim;

	void OnEnable()
	{
		CloseOthers += ClosePanel;
	}

	void OnDisable()
	{
		CloseOthers -= ClosePanel;
	}


	// Use this for initialization
	void Start () {
		anim = Panel.GetComponent<Animator> ();

		var button = GetComponent<Button> ();

		button.onClick.AddListener (() => ActivatePanelButton ());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void ClosePanel(GameObject other)
	{
		if (other == gameObject) {
			return;
		}

		anim.SetBool ("Trigger", false);
	}
		
	void ActivatePanelButton()
	{
		var value = !isActivated;

		anim.SetBool ("Trigger", value);
		isActivated = value;

		if (CloseOthers != null) {
			CloseOthers (gameObject);
		}
	}
}
