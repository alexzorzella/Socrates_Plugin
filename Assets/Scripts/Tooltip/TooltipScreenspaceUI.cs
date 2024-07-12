using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipScreenspaceUI : MonoBehaviour
{
    public static TooltipScreenspaceUI Instance { get; private set; }

    RectTransform canvasRectTransform;

    private RectTransform backgroundRectTransform;
    private TextMeshProUGUI textMeshPro;
    private RectTransform rectTransform;

    List<string> currentIcons = new List<string>();

    private void Awake()
    {
        Instance = this;

        canvasRectTransform = transform.root.GetComponent<RectTransform>();

        backgroundRectTransform = transform.Find("Background").GetComponent<RectTransform>();
        textMeshPro = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        rectTransform = transform.GetComponent<RectTransform>();

        HideTooltip();
    }

    private void UpdateText(string tooltipText)
    {
        textMeshPro.SetText(tooltipText);
        textMeshPro.ForceMeshUpdate();

        Vector2 textSize = textMeshPro.GetRenderedValues(false);
        Vector2 paddingSize = new Vector2(8, 8);

        backgroundRectTransform.sizeDelta = textSize + paddingSize;
    }

    private void Update()
    {
        Vector2 anchoredPosition = Input.mousePosition / canvasRectTransform.localScale.x;

        if(anchoredPosition.x + backgroundRectTransform.rect.width > canvasRectTransform.rect.width)
        {
            anchoredPosition.x = canvasRectTransform.rect.width - backgroundRectTransform.rect.width;
        }

        if (anchoredPosition.x < 0)
        {
            anchoredPosition.x = 0;
        }

        if (anchoredPosition.y < 0)
        {
            anchoredPosition.y = 0;
        }

        if (anchoredPosition.y + backgroundRectTransform.rect.height > canvasRectTransform.rect.height)
        {
            anchoredPosition.y = canvasRectTransform.rect.height - backgroundRectTransform.rect.height;
        }

        rectTransform.anchoredPosition = anchoredPosition;
    }

    public static void SetText(string tooltipText)
    {
		if(Instance != null)
		{
	        Instance.ShowTooltip(tooltipText);
		}
    }

    public static void Hide()
    {
		if(Instance != null)
		{
			Instance.HideTooltip();
		}
    }

    private void ShowTooltip(string tooltipText)
    {
        gameObject.SetActive(true);
        UpdateText(tooltipText);
    }

    private void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}