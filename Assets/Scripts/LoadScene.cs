using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour {

    public string loadLevel;

    // Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ChangeScene () {
        SceneManager.LoadScene(loadLevel);
    }

    public static void ChangeScene (string loadLevel) {
        SceneManager.LoadScene(loadLevel);
    }
}
