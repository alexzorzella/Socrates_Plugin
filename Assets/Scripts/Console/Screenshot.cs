using UnityEngine;
using UnityEngine.InputSystem;

public class Screenshot : MonoBehaviour {
    void Update() {
        if (Keyboard.current.leftCtrlKey.isPressed && 
            Keyboard.current.leftShiftKey.isPressed &&
            Keyboard.current.cKey.wasPressedThisFrame) {
            string folderPath = "Assets/StreamingAssets/Screenshots/";

            if (!System.IO.Directory.Exists(folderPath)) {
                System.IO.Directory.CreateDirectory(folderPath);
            }

            var screenshotName = "screenshot_" + System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") + ".png";
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folderPath, screenshotName), 2);
            Debug.Log($"Saved {screenshotName} to {folderPath}");
        }
    }
}