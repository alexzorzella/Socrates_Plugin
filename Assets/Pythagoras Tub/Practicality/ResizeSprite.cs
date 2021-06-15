using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResizeSprite : MonoBehaviour
{
    Image display;
    float displaySizeLerpSpeed = 30F;
    RectTransform rect;
    public static float targetHeight = 100;

    private void Start()
    {
        display = GetComponent<Image>();
        rect = display.GetComponent<RectTransform>();
    }

    private void Update()
    {
        UpdateSprite();
    }

    public void UpdateSprite()
    {
        float percent = GetPercent(display.sprite);

        rect.sizeDelta = new Vector2(
            Mathf.Lerp(rect.sizeDelta.x, CurrentImageSize().y * percent, Time.deltaTime * displaySizeLerpSpeed),
            Mathf.Lerp(rect.sizeDelta.y, CurrentImageSize().x * percent, Time.deltaTime * displaySizeLerpSpeed));
    }

    public static float GetPercent(Sprite sprite)
    {
        return targetHeight / sprite.rect.height;
    }

    public Vector2 CurrentImageSize()
    {
        return new Vector2(display.sprite.rect.height, display.sprite.rect.width);
    }
}