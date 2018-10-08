using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARContent : MonoBehaviour {

    public GameObject content;
    public GameObject background;
    public Sprite[] backgroundSprite;
    public GameObject previousButton;
    public GameObject nextButton;

    public static int index = -1;
    public static GameObject contentStatic;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnMouseDown() {
        // Open first content
        if (index == -1)
        {
            contentStatic = content;
            OpenContent(0);
        }
    }

    public void PreviousContent() {
        OpenContent(index - 1);
    }

    public void NextContent() {
        OpenContent(index + 1);
    }

    public void CloseContent() {
        contentStatic.transform.GetChild(index).gameObject.SetActive(false);
        background.SetActive(false);
        index = -1;
    }

    private void OpenContent(int newIndex) {
        int backgroundIndex = newIndex % backgroundSprite.Length;
        background.GetComponent<Image>().sprite = backgroundSprite[backgroundIndex];
        background.SetActive(true);

        // Manage previous button
        if (newIndex == 0)
            previousButton.SetActive(false);
        else
            previousButton.SetActive(true);

        // Manage next button
        if (newIndex == contentStatic.transform.childCount - 1)
            nextButton.SetActive(false);
        else
            nextButton.SetActive(true);

        // Close old content
        if (index != -1)
            contentStatic.transform.GetChild(index).gameObject.SetActive(false);

        // Open new conetent
        contentStatic.transform.GetChild(newIndex).gameObject.SetActive(true);

        index = newIndex;
    }
}
