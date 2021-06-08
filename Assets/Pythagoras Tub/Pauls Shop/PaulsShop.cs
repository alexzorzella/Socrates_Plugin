using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PaulsShop : MonoBehaviour
{
    private static PaulsShop _i;

    public static PaulsShop i
    {
        get
        {
            if(_i == null)
            {
                PaulsShop x = Resources.Load<PaulsShop>("PaulsShop");
                _i = Instantiate(x);
            }

            return _i;
        }
    }

    private void Start()
    {
        group.alpha = 0;
    }

    public TextMeshProUGUI seenTitleText;
    public TextMeshProUGUI purchaseButtonText;
    public List<ShopEntry> currentStock;
    public int currentItemSelected;

    public Button purchaseButton;

    public CanvasGroup group;
    public bool visible;

    private void Update()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        group.alpha = Mathf.Lerp(group.alpha, visible ? 1 : 0, Time.deltaTime * 10);
        group.interactable = visible;
        group.blocksRaycasts = visible;

        if (currentStock == null)
        {
            return;
        }

        if(currentStock.Count <= 0)
        {
            purchaseButton.interactable = false;
            purchaseButtonText.text = "Purchase";
            purchaseButtonText.color = Color.red;
            seenTitleText.text = "Out";
            return;
        }

        purchaseButtonText.text = $"Purchase ({GetCurrentEntry().cost})";

        purchaseButton.interactable = GetCurrentEntry().Prerequisite();
        purchaseButtonText.color = GetCurrentEntry().Prerequisite() ? Color.green : Color.red;
        seenTitleText.text = GetCurrentEntry().title;
    }

    private void SetVisible()
    {
        visible = true;
    }

    private void SetHidden()
    {
        visible = false;
    }

    public static void Static_SetVisibile()
    {
        i.SetVisible();
    }

    public static void Static_SetHidden()
    {
        i.SetHidden();
    }

    private void ClearShop()
    {
        currentStock = new List<ShopEntry>();
    }
    
    private void StockShop(params ShopEntry[] entries)
    {
        foreach (var entry in entries)
        {
            currentStock.Add(entry);
        }
    }

    private void ResetSelectedIndex()
    {
        currentItemSelected = 0;
    }

    public static void Static_ClearShop()
    {
        i.ClearShop();
    }

    public static void Static_StockShop(params ShopEntry[] entries)
    {
        i.StockShop(entries);
    }

    public static void Static_ResetSelectedIndex()
    {
        i.ResetSelectedIndex();
    }

    public void AlterSelectedIndex(int amount)
    {
        AudioManager.i.Play("ui_blip");

        currentItemSelected = IncrementWithOverflow.Run(currentItemSelected, currentStock.Count, amount);
    }

    public void TryBuy()
    {
        AudioManager.i.Play("enter_game");

        ShopEntry cachedEntry = GetCurrentEntry();

        if (cachedEntry.Prerequisite())
        {
            //Subtract cash
            cachedEntry.action.Invoke();
        }
    }

    private ShopEntry GetCurrentEntry()
    {
        return currentStock[currentItemSelected];
    }
}

public class ShopEntry
{
    public ShopEntry(string title, int cost, Predicate<object> predicate, Action action)
    {
        this.title = title;
        this.cost = cost;
        this.action = action;
        this.predicate = predicate;
    }

    public string title;
    public Action action;
    public int cost;
    private Predicate<object> predicate;

    public bool Prerequisite()
    {
        return predicate.Invoke(null);
    }
}