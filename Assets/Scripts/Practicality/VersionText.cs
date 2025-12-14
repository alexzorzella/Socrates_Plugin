using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class VersionText : MonoBehaviour {
    static readonly Regex VERSION_EXTRACTOR_REGEXP = new(@"((\d{2}.\d{2}).(\d+))(?:-(\w{4}))?");

    /**
     * Returns a version string suitable for printing as a watermark.
     *
     * It adds a "v" prefix, and strips the `-<sha>`.
     */
    static internal string PrintableVersionNoSha(string applicationVersion) {
        return "v" + VERSION_EXTRACTOR_REGEXP.Match(applicationVersion).Groups[1];
    }

    void Start() {
        GetComponent<TextMeshProUGUI>().text = PrintableVersionNoSha(Application.version);
    }
}