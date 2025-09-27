using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipScreenspaceUI : MonoBehaviour {
    RectTransform backgroundRectTransform;

    RectTransform canvasRectTransform;

    List<string> currentIcons = new();
    RectTransform rectTransform;
    TextMeshProUGUI textMeshPro;
    public static TooltipScreenspaceUI Instance { get; private set; }

    void Awake() {
        Instance = this;

        canvasRectTransform = transform.root.GetComponent<RectTransform>();

        backgroundRectTransform = transform.Find("Background").GetComponent<RectTransform>();
        textMeshPro = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        rectTransform = transform.GetComponent<RectTransform>();

        HideTooltip();
    }

    void Update() {
        Vector2 anchoredPosition = Input.mousePosition / canvasRectTransform.localScale.x;

        if (anchoredPosition.x + backgroundRectTransform.rect.width > canvasRectTransform.rect.width)
            anchoredPosition.x = canvasRectTransform.rect.width - backgroundRectTransform.rect.width;

        if (anchoredPosition.x < 0) anchoredPosition.x = 0;

        if (anchoredPosition.y < 0) anchoredPosition.y = 0;

        if (anchoredPosition.y + backgroundRectTransform.rect.height > canvasRectTransform.rect.height)
            anchoredPosition.y = canvasRectTransform.rect.height - backgroundRectTransform.rect.height;

        rectTransform.anchoredPosition = anchoredPosition;
    }

    void UpdateText(string tooltipText) {
        textMeshPro.SetText(tooltipText);
        textMeshPro.ForceMeshUpdate();

        var textSize = textMeshPro.GetRenderedValues(false);
        var paddingSize = new Vector2(8, 8);

        backgroundRectTransform.sizeDelta = textSize + paddingSize;
    }

    public static void SetText(string tooltipText) {
        if (Instance != null) Instance.ShowTooltip(tooltipText);
    }

    public static void Hide() {
        if (Instance != null) Instance.HideTooltip();
    }

    void ShowTooltip(string tooltipText) {
        gameObject.SetActive(true);
        UpdateText(tooltipText);
    }

    void HideTooltip() {
        gameObject.SetActive(false);
    }
}