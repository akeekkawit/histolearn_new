using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class General : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public static string[] ShuffleStringList(string[] array)
    {
        string[] shuffle = new string[array.Length];
        array.CopyTo(shuffle, 0);
        for (int i = 0; i < shuffle.Length; i++)
        {
            string temp = shuffle[i];
            int randomIndex = Random.Range(i, shuffle.Length);
            shuffle[i] = shuffle[randomIndex];
            shuffle[randomIndex] = temp;
        }

        return shuffle;
    }
}
