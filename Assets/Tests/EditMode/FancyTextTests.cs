using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

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

public class FancyTextTests {
    readonly List<Tuple<string, string>> tokens = new List<Tuple<string, string>>() {
        new("[wave]", "[!wave]"),
        new("[shake,4]", "[!shake]"),
        new("<i>", "</i>"),
        new("<size=150%>", "</size>"),
        new("<size=200%>", "</size>"),
        new("<size=50%>", "</size>"),
        new("<color=#E9BC31>", "</color>")
    };

    readonly List<string> safeWrappedTestStrings = new() {
        "bonanza",
        "sphinx",
        "miraculous"
    };
    
    readonly List<string> wrappedTestStrings = new() {
        "Thanks, dad.",
        "Obrigado, pai.",
        "The Zinhos!",
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
            Tuple<string, string> randomTuple = tokens[UnityEngine.Random.Range(0, tokens.Count)];
            
            openingTokens.Add(randomTuple.Item1);
            closingTokens.Add(randomTuple.Item2);
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
        
        FancyText fancyText = new FancyText(annotatedText);

        if (fancyText.GetAnnotationTokens().Count <= 0) {
            Assert.IsTrue(true);
        }

        // Debug.Log($"Raw Annotated: {annotatedText.Replace('<', '(').Replace('>', ')')}\nFancyText: {fancyText}\n\n" +
        //           $"Opening/closing ct: {openingTokens.Count}/{closingTokens.Count} \n\n");
        
        foreach (var token in fancyText.GetAnnotationTokens()) {
            if (token.richTextType == SocraticAnnotation.RichTextType.DELAY) {
                continue;
            }
            
            int actualTokenStartChar = token.startCharIndex;

            if (token.opener) {
                Assert.AreEqual(0, actualTokenStartChar, "Opening token index mismatched");
            } else {
                Assert.AreEqual(expectedClosingTokenStartCharIndex, actualTokenStartChar, "Closing token index mismatched");
            }
        }
    }

    [Test]
    public void _TestWrappedFancyTextBulkNoAnnotations() {
        foreach (var item in wrappedTestStrings) {
            FancyText fancyText = new FancyText(item);
            Assert.AreEqual(item, fancyText.ToString());
        }
    }
    
    [Test]
    public void _TestWrappedFancyTextBulk() {
        foreach (var item in wrappedTestStrings) {
            for (int i = 0; i < iterations; i++) {
                TestGenericWrappedText(item);    
            }
        }
    }
    
    [Test]
    public void TestFancyTextNoAnnotations() {
        string input = "efitzgerald";
        FancyText fancyText = new FancyText(input);
        
        Assert.AreEqual(0, fancyText.GetAnnotationTokens().Count);
    }
    
    [Test]
    public void TestFancyTextNoAnnotationsWithRichTextTag() {
        FancyText fancyText = new FancyText("<size=110%>tjobim</size>");
        Assert.AreEqual(0, fancyText.GetAnnotationTokens().Count);
    }
    
    [Test]
    public void TestFancyTextNoAnnotationsWithPunctutation() {
        FancyText fancyText = new FancyText("efitzgerald, mcarey: npert, hscott?! Amazing; an off-site event to remember");
        Assert.AreEqual(5, fancyText.GetAnnotationTokens().Count);
    }
    
    [Test]
    public void TestFancyTextNoAnnotationsWithRichTextTagWithPunctuation() {
        FancyText fancyText = new FancyText("<size=110%>tjobim, efitzgerald, mcarey: npert, hscott?!!! Amazing; an off-site event to remember</size>");
        Assert.AreEqual(6, fancyText.GetAnnotationTokens().Count);
    }
    
    [Test]
    public void _TestWrappedFancyTextBulkSimpleStrings() {
        for (int i = 0; i < iterations; i++) {
            string randomLine = safeWrappedTestStrings[UnityEngine.Random.Range(0,  safeWrappedTestStrings.Count)];
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

        int expectedClosingTokenIndex = 11;
        
        Assert.AreEqual(expectedClosingTokenIndex, fancyText.GetAnnotationTokens()[1].startCharIndex);
    }
    
    [Test]
    public void FancyTextSingleTokenNestedWithRichTextTagD() {
        FancyText fancyText = new FancyText("<size=20%>[wave]The Zinhos!</size>[!wave]");

        int expectedOpeningTokenCharIndex = 0;
        int expectedClosingTokenCharIndex = 11;
        
        int actualOpeningTokenCharIndex = fancyText.GetAnnotationTokens()[0].startCharIndex;
        int actualClosingTokenCharIndex = fancyText.GetAnnotationTokens()[1].startCharIndex;
        
        Assert.AreEqual(expectedOpeningTokenCharIndex, actualOpeningTokenCharIndex);
        Assert.AreEqual(expectedClosingTokenCharIndex, actualClosingTokenCharIndex);
    }
    
    [Test]
    public void FancyTextSingleTokenNestedWithMultipleRichTextTagB() {
        FancyText fancyText = new FancyText("[wave]<size=20%><i><color=#F1B82B>The Zinhos!</i></size>[!wave]</color></size>");

        int expectedClosingTokenIndex = 11;
        
        Assert.AreEqual(expectedClosingTokenIndex, fancyText.GetAnnotationTokens()[1].startCharIndex);
    }
    
    [Test]
    public void FancyTextAverageCase() {
        FancyText fancyText = new FancyText(
            "The forecast today expects <size=120%><i><color=#F1B82B>[wave]sunny</color></size> skies[!wave]. " +
            "Be safe out there-<i>don't forget your sunscreen</i>, or you might get [shake,2]sunburned![!shake]");

        int expT0StartChar = 27;
        int expT1StartChar = 38;
        int expT2StartChar = 104;
        int expT3StartChar = 114;
        
        int actT1StartChar = fancyText.GetAnnotationTokens()[0].startCharIndex;
        int actT2StartChar = fancyText.GetAnnotationTokens()[1].startCharIndex;
        int actT3StartChar = fancyText.GetAnnotationTokens()[2].startCharIndex;
        int actT4StartChar = fancyText.GetAnnotationTokens()[3].startCharIndex;
        
        Assert.AreEqual(expT0StartChar, actT1StartChar);
        Assert.AreEqual(expT1StartChar, actT2StartChar);
        Assert.AreEqual(expT2StartChar, actT3StartChar);
        Assert.AreEqual(expT3StartChar, actT4StartChar);
    }
}