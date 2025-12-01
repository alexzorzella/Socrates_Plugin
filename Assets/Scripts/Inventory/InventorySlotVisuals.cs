using System;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotVisuals : MonoBehaviour, InventorySlotListener {
    public int slot;
    
    Image backing;
    Image itemImage;

    readonly Color regularBackingColor = new Color(1, 1, 1, 0.25F);
    readonly Color selectedBackingColor = new Color(1, 1, 1, 0.5F);
    readonly Color transparent = new Color(1, 1, 1, 0.5F);

    Inventory inventory;

    Item currentItem;

    MultiAudioSource selectSharp;
    MultiAudioSource selectSoft;
    
    void Start() {
        selectSharp = MultiAudioSource.FromResource(gameObject, "select_sharp");
        selectSoft = MultiAudioSource.FromResource(gameObject, "select_soft");
        
        selectSharp.SetPitch(0.75F);
        selectSoft.SetPitch(0.75F);
        
        backing = GetComponent<Image>();
        itemImage = GetComponentInChildren<Image>();
        inventory = FindFirstObjectByType<Inventory>();
        inventory.RegisterListener(this, slot);
    }
    
    public void OnItemUpdated(Item item) {
        if (item == null) {
            currentItem = null;
            itemImage.color = transparent;
            return;
        }

        currentItem = item;
        
        itemImage.color = Color.white;
        itemImage.sprite = item.GetSprite();
    }

    public void OnNewSlotSelected(int isSlot, int wasSlot) {
        // if (currentItem != null) {
        //     backing.color = Color.white;
        // }
        // else {
            backing.color = this.slot == isSlot ? selectedBackingColor : regularBackingColor;
        // }

        if (slot == isSlot) {
            if (currentItem != null) {
                inventory.SetHotbarPrompt(currentItem.GetItemName());
                selectSharp.PlayRoundRobin();
                backing.color = Color.white;
            } else {
                inventory.FadeHotbarPromptOut();
                selectSoft.PlayRoundRobin();
            }
        }
    }
}