using UnityEngine;

public class SafeArea : MonoBehaviour
{
    public Canvas canvas;
    private RectTransform panelSafeArea;
    private Rect currentSafeArea = new Rect();
    ScreenOrientation currentOrientation = ScreenOrientation.LandscapeLeft;

    private void Start()
    {
        panelSafeArea = GetComponent<RectTransform>();

        currentOrientation = Screen.orientation;
        currentSafeArea = Screen.safeArea;

        ApplySafeArea();
    }

    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= canvas.pixelRect.width;
        anchorMin.y /= canvas.pixelRect.height;

        anchorMax.x /= canvas.pixelRect.width;
        anchorMax.y /= canvas.pixelRect.height;

        panelSafeArea.anchorMin = anchorMin;
        panelSafeArea.anchorMax = anchorMax;
    }
}
