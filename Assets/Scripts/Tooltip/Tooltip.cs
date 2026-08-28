using TMPro;
using UnityEngine;

public class ScreenspaceTooltip : MonoBehaviour {
    static ScreenspaceTooltip _i;

    RectTransform canvasRectTransform;

    public RectTransform tooltipRect;
    public RectTransform backgroundRect;
    public TextMeshProUGUI textDisplay;

    public static ScreenspaceTooltip i {
        get {
            if (_i == null) {
                GameObject x = ResourceLoader.LoadObject("Tooltip");
                _i = Instantiate(x).GetComponent<ScreenspaceTooltip>();
            }

            return _i;
        }
    }

    void Awake() {
        DontDestroyOnLoad(gameObject);
        canvasRectTransform = GetComponent<RectTransform>();
        HideTooltip();
    }

    void UpdateText(string tooltipText) {
        textDisplay.SetText(tooltipText);
        textDisplay.ForceMeshUpdate();

        Vector2 textSize = textDisplay.GetRenderedValues(false);
        Vector2 paddingSize = new Vector2(8, 8);

        backgroundRect.sizeDelta = textSize + paddingSize;
    }

    public void SetPosition(Vector2 anchoredPosition) {
        tooltipRect.anchoredPosition = anchoredPosition;
    }

    void Update() {
        Vector2 anchoredPosition = Input.mousePosition / canvasRectTransform.localScale.x;

        if (anchoredPosition.x + backgroundRect.rect.width > canvasRectTransform.rect.width) {
            anchoredPosition.x = canvasRectTransform.rect.width - backgroundRect.rect.width;
        }

        if (anchoredPosition.x < 0) { anchoredPosition.x = 0; }
        if (anchoredPosition.y < 0) { anchoredPosition.y = 0; }

        if (anchoredPosition.y + backgroundRect.rect.height > canvasRectTransform.rect.height) {
            anchoredPosition.y = canvasRectTransform.rect.height - backgroundRect.rect.height;
        }

        tooltipRect.anchoredPosition = anchoredPosition;
    }

    public static void SetText(string tooltipText) {
        i.ShowTooltip(tooltipText);
    }

    public static void Hide() {
        i.HideTooltip();
    }

    void ShowTooltip(string tooltipText) {
        gameObject.SetActive(true);
        UpdateText(tooltipText);
    }

    void HideTooltip() {
        gameObject.SetActive(false);
    }

    public void SetFont(TMP_FontAsset debugFont) {
        textDisplay.font = debugFont;
    }

    public void SetFontSize(float size) {
        textDisplay.fontSize = size;
    }
}