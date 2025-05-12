using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public enum ScreenPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center,
        TopCenter,
        BottomCenter
    }

    public ScreenPosition screenPosition = ScreenPosition.TopLeft; // Default position
    public TextMeshProUGUI fpsText; // TextMeshPro Text element
    public Color goodFPS = Color.green;  // FPS > 30
    public Color mediumFPS = Color.yellow; // 15-20 FPS
    public Color badFPS = Color.red; // FPS < 15
    public bool Fps30Lock = false;

    private float deltaTime = 0.0f;

    void Start()
    {
        SetTextPosition();
        if (Fps30Lock)
        {
            Application.targetFrameRate = 30;  // Ya 45
        }
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        UpdateFPSDisplay(fps);
    }

    void UpdateFPSDisplay(float fps)
    {
        if (fpsText == null) return;

        fpsText.text = "FPS: " + Mathf.Round(fps);

        if (fps > 25)
            fpsText.color = goodFPS;
        else if (fps >= 15)
            fpsText.color = mediumFPS;
        else
            fpsText.color = badFPS;
    }


    void SetTextPosition()
    {
        if (fpsText == null) return;

        RectTransform rectTransform = fpsText.GetComponent<RectTransform>();

        switch (screenPosition)
        {
            case ScreenPosition.TopLeft:
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.pivot = new Vector2(0, 1);
                rectTransform.anchoredPosition = new Vector2(10, -10);
                break;

            case ScreenPosition.TopRight:
                rectTransform.anchorMin = new Vector2(1, 1);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.pivot = new Vector2(1, 1);
                rectTransform.anchoredPosition = new Vector2(-10, -10);
                break;

            case ScreenPosition.BottomLeft:
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);
                rectTransform.pivot = new Vector2(0, 0);
                rectTransform.anchoredPosition = new Vector2(10, 10);
                break;

            case ScreenPosition.BottomRight:
                rectTransform.anchorMin = new Vector2(1, 0);
                rectTransform.anchorMax = new Vector2(1, 0);
                rectTransform.pivot = new Vector2(1, 0);
                rectTransform.anchoredPosition = new Vector2(-10, 10);
                break;

            case ScreenPosition.Center:
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = new Vector2(0, 0);
                break;

            case ScreenPosition.TopCenter:
                rectTransform.anchorMin = new Vector2(0.5f, 1);
                rectTransform.anchorMax = new Vector2(0.5f, 1);
                rectTransform.pivot = new Vector2(0.5f, 1);
                rectTransform.anchoredPosition = new Vector2(0, -10);
                break;

            case ScreenPosition.BottomCenter:
                rectTransform.anchorMin = new Vector2(0.5f, 0);
                rectTransform.anchorMax = new Vector2(0.5f, 0);
                rectTransform.pivot = new Vector2(0.5f, 0);
                rectTransform.anchoredPosition = new Vector2(0, 10);
                break;
        }
    }
}
