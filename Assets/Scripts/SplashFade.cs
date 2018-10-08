using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashFade : MonoBehaviour {

    public Image splashImage;
    public string tutorialScene;
    public string menuScene;

	IEnumerator Start () {
        splashImage.canvasRenderer.SetAlpha(0.0f);
        FadeIn();
        yield return new WaitForSeconds(2.5f);
        FadeOut();
        yield return new WaitForSeconds(2.5f);

        // change scene
        if (!PlayerPrefs.HasKey("FirstTime"))
        {
            PlayerPrefs.SetInt("FirstTime", 1);
            SceneManager.LoadScene(tutorialScene);
        }
        else
        {
            SceneManager.LoadScene(menuScene);
        }
    }

    void FadeIn () {
        splashImage.CrossFadeAlpha(1.0f, 1.5f, false);
    }

    void FadeOut () {
        splashImage.CrossFadeAlpha(0.0f, 2.5f, false);
    }
}
