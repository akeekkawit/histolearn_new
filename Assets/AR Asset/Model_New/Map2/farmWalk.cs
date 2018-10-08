using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class farmWalk : MonoBehaviour {

	// Use this for initialization
	public GameObject farmer;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnMouseDown()
	{
		farmer.GetComponent<Animation>().Play("farmer walk");
	}
}
