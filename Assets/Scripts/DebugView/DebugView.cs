using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Switch;

public class DebugView : MonoBehaviour {
   static DebugView _i;
   
   static readonly Dictionary<string, TextMeshProUGUI> children = new();
   static readonly List<string> childNames = new();
   
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
         children.Add(name, child.GetComponent<TextMeshProUGUI>());
         childNames.Add(name); 
      }
   }
	
   public List<string> GetObjectNames() {
      return childNames;
   }

   public bool ToggleDebugTextObject(string objectName) {
      if (!children.ContainsKey(objectName)) {
         Debug.Log($"There isn't an object named {objectName} in DebugView");
         return false;
      }
      
      bool toggleTo = !children[objectName].gameObject.activeSelf;
      children[objectName].gameObject.SetActive(toggleTo);
      return toggleTo;
   }

   public void SetDebugText(string objectName, string debugInfo) {
      if (!children.ContainsKey(objectName)) {
         Debug.Log($"There isn't an object named {objectName} in DebugView");
         return;
      }

      children[objectName].SetText(debugInfo);
   }
}