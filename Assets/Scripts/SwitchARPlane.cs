using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchARPlane : MonoBehaviour {

    public GameObject question;
    public GameObject AR;
    public GameObject model;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void OpenAR() {
        model.SetActive(true);
        AR.SetActive(true);
        question.SetActive(false);
        Screen.orientation = ScreenOrientation.Landscape;
    }

    public void CloseAR() {
        Screen.orientation = ScreenOrientation.Portrait;
        question.SetActive(true);
        model.SetActive(false);
        AR.SetActive(false);
    }
}
