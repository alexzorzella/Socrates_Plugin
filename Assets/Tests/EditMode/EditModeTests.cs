using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EditModeTests {
    readonly List<string> testInputs = new() {
        "The Zinhos!",
        "[wave]The Zinhos![!wave]",
        "[wave][shake,2]The Zinhos![!shake][!wave]",
        "[wave]<size=120%>The Zinhos!</size>[!wave]",
        "[wave]<size=120%>The Zinhos![!wave]</size>"
    };
    
    [Test]
    public void FancyTextNoTokens() {
        FancyText fancyText = new FancyText(testInputs[0]);

        Assert.AreEqual("The Zinhos!", fancyText.ToString());
    }
    
    [Test]
    public void FancyTextSingleTokenSet() {
        FancyText fancyText = new FancyText(testInputs[1]);

        Assert.AreEqual("The Zinhos!", fancyText.ToString());
    }
}