using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class openRoof : MonoBehaviour {

	bool isClick = false;
	public float targetScale = 0.1f;
	public float shrinkSpeed = 7.5f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		if (isClick)
		{
		
			transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(targetScale, targetScale, targetScale), Time.deltaTime * shrinkSpeed);
			Destroy(this.gameObject,1);
		}
		
	}
	private void OnMouseUp()
	{
		isClick = true;
	}

	
}
