public interface InventorySlotListener {
    void OnItemUpdated(Item item);
    void OnNewSlotSelected(int isSlot, int wasSlot);
}