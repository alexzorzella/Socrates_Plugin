using NUnit.Framework;

public class SocraTSVReplacementTests {
    [Test]
    public void ReplaceTokenSingle() {
        DialogueManifest.ClearDialogueVariables();
        DialogueManifest.AddDialogueVariable("user", "Amy");

        string input = "{user} ate jamon.";

        string expected = "Amy ate jamon.";
        string actual = DialogueManifest.ReplaceTokensIn(input);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void ReplaceTokenSingleRegularWordFirst() {
        DialogueManifest.ClearDialogueVariables();
        DialogueManifest.AddDialogueVariable("user", "Amy");

        string input = "Who ate jamon? {user} ate jamon.";

        string expected = "Who ate jamon? Amy ate jamon.";
        string actual = DialogueManifest.ReplaceTokensIn(input);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void ReplaceTokenSingleRightBeforePunctuation() {
        DialogueManifest.ClearDialogueVariables();
        DialogueManifest.AddDialogueVariable("user", "Amy");

        string input = "Who ate jamon? {user}.";

        string expected = "Who ate jamon? Amy.";
        string actual = DialogueManifest.ReplaceTokensIn(input);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void ReplaceTokenMultipleWithOneInsideWord() {
        DialogueManifest.ClearDialogueVariables();
        DialogueManifest.AddDialogueVariable("food", "jamon");

        string input = "Amy, was the {food} super{food}ilicious?";

        string expected = "Amy, was the jamon superjamonilicious?";
        string actual = DialogueManifest.ReplaceTokensIn(input);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void ReplaceTokensTwoUnique() {
        DialogueManifest.ClearDialogueVariables();
        DialogueManifest.AddDialogueVariable("user", "Amy");
        DialogueManifest.AddDialogueVariable("food", "jamon");

        string input = "{user} ate {food}.";

        string expected = "Amy ate jamon.";
        string actual = DialogueManifest.ReplaceTokensIn(input);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void ReplaceTokensTwoAdjacent() {
        DialogueManifest.ClearDialogueVariables();
        DialogueManifest.AddDialogueVariable("user", "Amy");
        DialogueManifest.AddDialogueVariable("title", "Who Seeks Jamon");

        string input = "{user} {title} devoured jamon.";

        string expected = "Amy Who Seeks Jamon devoured jamon.";
        string actual = DialogueManifest.ReplaceTokensIn(input);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void ReplaceTokensOnlyTokensManyAdjacent() {
        DialogueManifest.ClearDialogueVariables();
        DialogueManifest.AddDialogueVariable("user", "Amy");
        DialogueManifest.AddDialogueVariable("title", "Who Seeks Jamon");
        DialogueManifest.AddDialogueVariable("action", "devoured");
        DialogueManifest.AddDialogueVariable("food", "jamon");

        string input = "{user} {title} {action} {food}";

        string expected = "Amy Who Seeks Jamon devoured jamon";
        string actual = DialogueManifest.ReplaceTokensIn(input);

        Assert.AreEqual(expected, actual);
    }
}