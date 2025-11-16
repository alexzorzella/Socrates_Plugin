using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Inventory : MonoBehaviour {
    public TextMeshProUGUI hotbarPrompt;
    const float hotbarFadeInTime = 0.2F;
    const float hotbarPromptLasts = 2F;
    const float hotbarFadeOutTime = 1F;

    const int slotCount = 5;

    readonly List<Item> items = new();
    readonly List<List<InventorySlotListener>> inventorySlotListeners = new();

    int currentHotbarSelection = 0;
    int lastHotbarSelection = -1;
    int lastScrollDirection;

    bool scrollSnapsToDirection = false;

    const float scrollSens = 1F;

    void Awake() {
        for (int i = 0; i < slotCount; i++) {
            items.Add(null);
            inventorySlotListeners.Add(new List<InventorySlotListener>());
        }

        hotbarPrompt.text = "";
    }

    public void RegisterListener(InventorySlotListener listener, int toSlot) {
        inventorySlotListeners[toSlot].Add(listener);
        listener.OnItemUpdated(items[toSlot]);
    }

    public void NotifyListenersOfItem(int forSlot) {
        foreach (var listener in inventorySlotListeners[forSlot]) {
            listener.OnItemUpdated(items[forSlot]);
        }
    }

    public void NotifyListenersOfSlotSelection() {
        foreach (var listenerList in inventorySlotListeners) {
            foreach (var listener in listenerList) {
                listener.OnNewSlotSelected(currentHotbarSelection, lastHotbarSelection);
            }
        }
    }

    public void SetItem(Item item, int slot) {
        items[slot] = item;
        NotifyListenersOfItem(slot);
    }

    public Item CurrentItem() {
        return items[currentHotbarSelection];
    }

    public bool HoldingNothing() {
        return items[currentHotbarSelection] == null;
    }

    public Item DropItem() {
        Item currentItem = items[currentHotbarSelection];
        items[currentHotbarSelection] = null;
        return currentItem;
    }

    public bool HotbarContainsItem() {
        foreach (var item in items) {
            if (item != null) {
                return true;
            }
        }
        
        return false;
    }

    void Update() {
        UpdateScroll();
    }

    void UpdateScroll() {
        float scroll = (int)(-Input.mouseScrollDelta.y * scrollSens);

        IncrementWithOverflow.Run(
            currentHotbarSelection,
            slotCount, (int)scroll, out currentHotbarSelection);

        lastScrollDirection = scroll > 0 ? 1 : -1;

        if (scrollSnapsToDirection) {
            if (HotbarContainsItem()) {
                while (HoldingNothing()) {
                    IncrementWithOverflow.Run(
                        currentHotbarSelection,
                        slotCount, 
                        lastScrollDirection, 
                        out currentHotbarSelection);
                }
            }
        }

        if (lastHotbarSelection != currentHotbarSelection) {
            NotifyListenersOfSlotSelection();
            lastHotbarSelection = currentHotbarSelection;
        }
    }

    public void SetHotbarPrompt(string textContent) {
        LeanTween.cancel(gameObject);

        hotbarPrompt.text = textContent;
        hotbarPrompt.alpha = 0;

        LeanTween.value(gameObject, 0, 1, hotbarFadeInTime).setOnComplete(() => {
                LeanTween.delayedCall(hotbarPromptLasts, FadeHotbarPromptOut);
            }).setOnUpdate((value) => { hotbarPrompt.alpha = value; })
            .setEase(LeanTweenType.easeOutQuad);
    }

    public void FadeHotbarPromptOut() {
        LeanTween.cancel(gameObject);

        // hotbarPrompt.alpha = 1;

        LeanTween.value(gameObject, hotbarPrompt.alpha, 0, hotbarFadeOutTime * hotbarPrompt.alpha)
            .setOnUpdate((value) => { hotbarPrompt.alpha = value; })
            .setEase(LeanTweenType.easeOutQuad);
    }
}