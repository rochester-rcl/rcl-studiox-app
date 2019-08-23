using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
public class FullscreenFade : MonoBehaviour
{
    // private properties
    private GameObject fadeCanvas;
    private Image fadeImage;
    private bool fadeInTriggered = false;
    private float timeSinceFadeInTriggered = 0.0f;
    private bool fadeOutTriggered = false;
    private float timeSinceFadeOutTriggered = 0.0f;
    private float padding = 0.2f;
    private enum EASING { IN, OUT };
    // public properties
    public float fadeDuration = 2.0f;
    public Vector2 referenceResolution = new Vector2(800, 600);
    [Tooltip("The fill color for the fullscreen fade effect")]
    public Color fadeColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
    public delegate void FadeInBegins();
    public delegate void FadeOutBegins();
    public delegate void FadeInEnds();
    public delegate void FadeOutEnds();
    public static event FadeInBegins OnFadeInBegins;
    public static event FadeOutBegins OnFadeOutBegins;
    public static event FadeInEnds OnFadeInEnds;
    public static event FadeOutEnds OnFadeOutEnds;

    void Awake()
    {
        initFadeImage();
    }

    void Update()
    {
        if (fadeInTriggered)
        {
            timeSinceFadeInTriggered += Time.deltaTime;
            doFadeIn();
        }
        else if (fadeOutTriggered)
        {
            timeSinceFadeOutTriggered += Time.deltaTime;
            doFadeOut();
        }
    }

    public int GetMaxSortingOrder()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        int maxSortingOrder = 0;
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            maxSortingOrder = canvas.sortingOrder > maxSortingOrder ? canvas.sortingOrder : maxSortingOrder;
        }
        return maxSortingOrder;
    }

    /*** Fades ***/
    public void initFadeImage()
    {
        fadeCanvas = new GameObject();
        fadeCanvas.name = "CanvasOverlay";
        Canvas canvas = fadeCanvas.AddComponent<Canvas>();
        CanvasScaler scaler = fadeCanvas.AddComponent<CanvasScaler>();
        GraphicRaycaster rayCaster = fadeCanvas.AddComponent<GraphicRaycaster>();
        rayCaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
        rayCaster.ignoreReversedGraphics = true;
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.matchWidthOrHeight = 0;
        scaler.referencePixelsPerUnit = 100;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = GetMaxSortingOrder() + 1;
        canvas.overrideSorting = true;
        fadeImage = fadeCanvas.AddComponent<Image>();
        fadeImage.color = fadeColor;
        fadeImage.enabled = true;
    }

    public void triggerFadeIn()
    {
        fadeImage.enabled = true;
        fadeInTriggered = true;
        if (OnFadeInBegins != null)
        {
            OnFadeInBegins();
        }
    }

    public void triggerFadeOut()
    {
        fadeImage.enabled = true;
        fadeOutTriggered = true;
        if (OnFadeOutBegins != null)
        {
            OnFadeOutBegins();
        }
    }

    public void finishFadeIn()
    {
        fadeInTriggered = false;
        fadeImage.enabled = false;
        timeSinceFadeInTriggered = 0.0f;
        if (OnFadeInEnds != null)
        {
            OnFadeInEnds();
        }
    }

    public void finishFadeOut()
    {
        fadeOutTriggered = false;
        timeSinceFadeOutTriggered = 0.0f;
        if (OnFadeOutEnds != null)
        {
            OnFadeOutEnds();
        }
    }

    private float ease(EASING mode)
    {
        float delta = mode == EASING.IN ? timeSinceFadeInTriggered : timeSinceFadeOutTriggered;
        return Mathf.SmoothStep(0.0f, 1.0f, (1.0f / (fadeDuration - delta) - padding));
    }

    public void doFadeOut()
    {
        if (fadeImage.color.a < 1.0f)
        {
            fadeImage.color = Color.Lerp(Color.clear, fadeColor, ease(EASING.OUT));
        }
        else
        {
            finishFadeOut();
        }
    }

    public void doFadeIn()
    {
        if (fadeImage.color.a > 0.0f)
        {
            fadeImage.color = Color.Lerp(fadeColor, Color.clear, ease(EASING.IN));
        }
        else
        {
            finishFadeIn();
        }
    }

    // ASYNC FADES 
    public async Task FadeInAsync()
    {
        triggerFadeIn();
        await Task.Delay((int)fadeDuration * 1000);
    }

    public async Task FadeOutAsync()
    {
        triggerFadeOut();
        await Task.Delay((int)fadeDuration * 1000);
    }
}