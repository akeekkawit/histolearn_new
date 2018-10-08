using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoController : MonoBehaviour {

    public string url;
    public GameObject AR;
    public GameObject model;
    public GameObject videoPlayer;
    public GameObject videoController;
    public GameObject playPauseButton;
    public Sprite playButtonSprite;
    public Sprite pauseButtonSprite;
    public Text currentMinutes;
    public Text currentSeconds;
    public Text totalMinutes;
    public Text totalSeconds;
    public Image progressBar;

    private static VideoPlayer videoPlayerVP;
    private static Image playPauseButtonImage;
    private static int totalTime;

    void Awake() {
        videoPlayerVP = videoPlayer.GetComponent<VideoPlayer>();
        playPauseButtonImage = playPauseButton.GetComponent<Image>();
    }

    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
        if (videoPlayer.activeSelf && videoController.activeSelf)
        {
            SetCurrentTime();

            if ((int)videoPlayerVP.time >= totalTime)
                CloseVideo();
        }
	}

    public void OpenVideo() {
        videoPlayer.SetActive(true);

        playPauseButtonImage.sprite = pauseButtonSprite;
        progressBar.fillAmount = 0.0F;

        AR.SetActive(false);
        model.SetActive(false);

        videoPlayerVP.url = url;
        videoPlayerVP.prepareCompleted += (val) =>
        {
            SetTotalTime();
            videoController.SetActive(true);
        };
    }

    public void CloseVideo() {
        AR.SetActive(true);
        model.SetActive(true);
        videoController.SetActive(false);
        videoPlayer.SetActive(false);
    }

    public void PlayPause() {
        if(videoPlayerVP.isPlaying) {
            videoPlayerVP.Pause();
            playPauseButtonImage.sprite = playButtonSprite;
        }
        else {
            videoPlayerVP.Play();
            playPauseButtonImage.sprite = pauseButtonSprite;
        }
    }

    void SetTotalTime() {
        totalTime = (int)(videoPlayerVP.frameCount / videoPlayerVP.frameRate);
        string minutes = Mathf.Floor(totalTime / 60).ToString("00");
        string seconds = (totalTime % 60).ToString("00");

        totalMinutes.text = minutes;
        totalSeconds.text = seconds;
    }

    void SetCurrentTime() {
        string minutes = Mathf.Floor((int)videoPlayerVP.time / 60).ToString("00");
        string seconds = ((int)videoPlayerVP.time % 60).ToString("00");

        currentMinutes.text = minutes;
        currentSeconds.text = seconds;
    }
}
