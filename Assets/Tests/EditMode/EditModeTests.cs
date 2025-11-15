using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public static class IListExtensions {
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> ts) {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i) {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
}

public class EditModeTests {
    readonly List<Tuple<string, string>> tokens = new List<Tuple<string, string>>() {
        new("[wave]", "[!wave]"),
        new("[shake,4]", "[!shake]"),
        new("[delay,2]", "[!delay]"),
        new("<i>", "</i>"),
        new("<size=150%>", "</size>"),
        new("<size=50%>", "</size>"),
        new("<size=10%>", "</size>"),
        new("<color=#E9BC31>", "</color>")
    };
    
    readonly List<string> wrappedTestStrings = new() {
        "Thanks, dad.",
        "Obrigado, pai.",
        "The Zinhos!",
        "Bonanza, sphinx, miraculous",
        "A test in the hand is worth hours of editing in the bush",
        "All tests passed, why not write some more?",
        "Did you remember to clone?",
        "YEAHHHH!",
        "What's a five letter word for 'within a group'?",
        "Prueba caso",
        "今日はソクラテスを撫でましたか？",
        "마치 쥐가 부엌으로 달려가서 치즈를 훔친 다음, 굴로 돌아가기 직전에 'ㅋㅋㅋ'라고 소리치는 것과 같습니다.",
        "C'est un plaisir de tester",
        "查爾斯！好久不見！",
        "नेपाली खाना मेरो मनपर्ने खाना हो।"
    };

    const int iterations = 100;
    const int maxTags = 20;
    
    void TestGenericWrappedText(string rawText) {
        int expectedClosingTokenStartCharIndex = rawText.Length;

        List<string> openingTokens = new();
        List<string> closingTokens = new();
        
        for (int i = 0; i < UnityEngine.Random.Range(0, maxTags); i++) {
            openingTokens.Add(tokens[UnityEngine.Random.Range(0, tokens.Count)].Item1);            
            closingTokens.Add(tokens[UnityEngine.Random.Range(0, tokens.Count)].Item2);            
        }
        
        openingTokens.Shuffle();
        closingTokens.Shuffle();

        string prefix = "";
        string suffix = "";
        
        for (int i = 0; i < openingTokens.Count; i++) {
            prefix += openingTokens[i];
            suffix += closingTokens[i];
        }

        string annotatedText = $"{prefix}{rawText}{suffix}";
        
        FancyText fancyText = new FancyText(rawText);

        foreach (var token in fancyText.GetAnnotationTokens()) {
            int actualTokenStartChar = token.startCharIndex;

            if (token.opener) {
                Assert.AreEqual(0, actualTokenStartChar);
            } else {
                Assert.AreEqual(expectedClosingTokenStartCharIndex, actualTokenStartChar);
            }
        }
        
        Debug.Log(fancyText.ToString());
    }

    [Test]
    public void _TestWrappedFancyTextBulk() {
        for (int i = 0; i < iterations; i++) {
            string randomLine = wrappedTestStrings[UnityEngine.Random.Range(0,  wrappedTestStrings.Count)];
            TestGenericWrappedText(randomLine);
        }
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
    
    [Test]
    public void FancyTextSingleTokenNestedWithRichTextTagB() {
        FancyText fancyText = new FancyText("[wave]<size=20%>The Zinhos!</size>[!wave]");

        string expectedContent = "<size=20%>The Zinhos!</size>";
        int expectedclosingTokenIndex = 11;
        
        string actualContent = fancyText.ToString();
        
        Assert.AreEqual(expectedContent, actualContent);
        Assert.AreEqual(expectedclosingTokenIndex, fancyText.GetAnnotationTokens()[1].startCharIndex);
    }
    
    [Test]
    public void FancyTextSingleTokenNestedWithRichTextTagC() {
        FancyText fancyText = new FancyText("[wave]<size=20%>The Zinhos![!wave]</size>");

        int expectedclosingTokenIndex = 11;
        
        Assert.AreEqual(expectedclosingTokenIndex, fancyText.GetAnnotationTokens()[1].startCharIndex);
    }
    
    [Test]
    public void FancyTextSingleTokenNestedWithMultipleRichTextTags() {
        FancyText fancyText = new FancyText("[wave]<size=20%><i>The Zinhos!</i></size>[!wave]</size>");

        int expectedclosingTokenIndex = 11;
        
        Assert.AreEqual(expectedclosingTokenIndex, fancyText.GetAnnotationTokens()[1].startCharIndex);
    }
    
    [Test]
    public void FancyTextSingleTokenNestedWithMultipleRichTextTagB() {
        FancyText fancyText = new FancyText("[wave]<size=20%><i><color=#F1B82B>The Zinhos!</i></size>[!wave]</color></size>");

        int expectedclosingTokenIndex = 11;
        
        Assert.AreEqual(expectedclosingTokenIndex, fancyText.GetAnnotationTokens()[1].startCharIndex);
    }
}