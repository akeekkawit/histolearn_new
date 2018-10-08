using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;

public class VideoProgressBar : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [SerializeField]
    private VideoPlayer videoPlayer;

    private Image progressBar;

	// Use this for initialization
	void Start () {
        progressBar = GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () {
        if (videoPlayer.frameCount > 0)
            progressBar.fillAmount = (float)videoPlayer.frame / (float)videoPlayer.frameCount;
    }

    public void OnDrag(PointerEventData eventData) {
        Skip(eventData);
    }

    public void OnPointerDown(PointerEventData eventData) {
        Skip(eventData);
    }

    public void Skip(PointerEventData eventData) {
        Vector2 localPoint;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(progressBar.rectTransform, eventData.position, null, out localPoint)) {
            float pct = Mathf.InverseLerp(progressBar.rectTransform.rect.xMin, progressBar.rectTransform.rect.xMax, localPoint.x);
            var frame = videoPlayer.frameCount * pct;
            videoPlayer.frame = (long)frame;
        }
    }
}
