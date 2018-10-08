using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServeyMode : MonoBehaviour {

    public string loadLevel;
    public GameObject closeButton;

    // Use this for initialization
    void Start () {
        Screen.orientation = ScreenOrientation.Landscape;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void CloseScene() {
        Screen.orientation = ScreenOrientation.Portrait;
        LoadScene.ChangeScene(loadLevel);
    }
}
