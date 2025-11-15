using UnityEngine;

public class Item {
    string name;
    Sprite visuals;

    public Item(string name, string visualsName) {
        this.name = name;
        visuals = ResourceLoader.LoadSprite(visualsName);
    }
    
    public string GetItemName() {
        return name;
    }

    public Sprite GetSprite() {
        return visuals;
    }
}