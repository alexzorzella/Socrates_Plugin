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
    
    [Test]
    public void FancyTextTwoNestedTokenSets() {
        FancyText fancyText = new FancyText(testInputs[2]);

        Assert.AreEqual("The Zinhos!", fancyText.ToString());
    }

    [Test]
    public void FancyTextSingleTokenNestedWithRichTextTag() {
        FancyText fancyText = new FancyText("[wave]<size=120%>The Zinhos!</size>[!wave]");

        string expectedContent = "<size=120%>The Zinhos!</size>";
        int expectedclosingTokenIndex = 11;
        
        string actualContent = fancyText.ToString();
        
        Assert.AreEqual(expectedContent, actualContent);
        Assert.AreEqual(expectedclosingTokenIndex, fancyText.GetAnnotationTokens()[1].startCharIndex);
    }
}