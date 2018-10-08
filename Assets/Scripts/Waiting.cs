using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Waiting : MonoBehaviour {

    public Sprite[] frames;
    public Image loading;
    int framesPerSecond = 3;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        int index = (int)((Time.time * framesPerSecond) % frames.Length);
        loading.sprite = frames[index];
    }
}
