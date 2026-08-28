using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugView : MonoBehaviour {
    static DebugView _i;

    static readonly Dictionary<string, TextMeshProUGUI> objects = new();
    static readonly List<string> objectNames = new();

    public static DebugView i {
        get {
            if (_i == null) {
                GameObject x = ResourceLoader.LoadObject("DebugView");

                _i = Instantiate(x).GetComponent<DebugView>();
                _i.Initialize();
            }

            return _i;
        }
    }

    void Initialize() {
        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < transform.childCount; i++) {
            Transform child = transform.GetChild(i);

            if (child.GetComponent<TextMeshProUGUI>() == null) {
                continue;
            }

            string name = child.name.ToLower();
            objects.Add(name, child.GetComponent<TextMeshProUGUI>());
            objectNames.Add(name);
        }
    }

    public List<string> GetObjectNames() {
        return objectNames;
    }

    public void SetDebugTextObjectActive(string objectName, bool active) {
        if (!objects.ContainsKey(objectName)) {
            Debug.Log($"There isn't an object named {objectName} in DebugView");
        }

        objects[objectName].gameObject.SetActive(active);
    }

    public bool ToggleDebugTextObject(string objectName) {
        if (!objects.ContainsKey(objectName)) {
            Debug.Log($"There isn't an object named {objectName} in DebugView");
            return false;
        }

        bool toggleTo = !objects[objectName].gameObject.activeSelf;
        objects[objectName].gameObject.SetActive(toggleTo);
        return toggleTo;
    }

    public void SetDebugText(string objectName, string debugInfo) {
        if (!objects.ContainsKey(objectName)) {
            Debug.Log($"There isn't an object named {objectName} in DebugView");
            return;
        }

        objects[objectName].SetText(debugInfo);
    }
    
    public void ShowAll() {
        foreach (var objectName in objectNames) {
            SetDebugTextObjectActive(objectName, true);
        }
    }

    public void HideAll() {
        foreach (var objectName in objectNames) {
            SetDebugTextObjectActive(objectName, false);
        }
    }

    public bool ShowAllIfAtLeastOneObjectInactiveOtherwiseHideAll() {
        bool doHideAll = true;

        foreach (var objectName in objectNames) {
            if (!objects[objectName].gameObject.activeSelf) {
                doHideAll = false;
                break;
            }
        }

        if (doHideAll) {
            HideAll();
        } else {
            ShowAll();
        }

        return doHideAll;
    }
}
