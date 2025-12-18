using NUnit.Framework;
using SocratesDialogue;
using UnityEngine;

public class SocraTSVTests {
    public class ParseModeTests {
        [Test]
        public void ParseModeSkipLineEmptyInput() {
            string line = "";

            DialogueParser.ParsingMode expected = DialogueParser.ParsingMode.SKIP_LINE;
            DialogueParser.ParsingMode actual = DialogueParser.ParsingModeFromLine(line);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ParseModeSkipLine() {
            string line = "\t\t\t\t\t";

            DialogueParser.ParsingMode expected = DialogueParser.ParsingMode.SKIP_LINE;
            DialogueParser.ParsingMode actual = DialogueParser.ParsingModeFromLine(line);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ParseModeTokenDefinition() {
            string line = "token\tvalue\t\t\t\t";

            DialogueParser.ParsingMode expected = DialogueParser.ParsingMode.TOKEN_DEF;
            DialogueParser.ParsingMode actual = DialogueParser.ParsingModeFromLine(line);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ParseModeTokenDefinitionCapitalized() {
            string line = "Token\tvalue\t\t\t\t";

            DialogueParser.ParsingMode expected = DialogueParser.ParsingMode.TOKEN_DEF;
            DialogueParser.ParsingMode actual = DialogueParser.ParsingModeFromLine(line);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ParseModeTokenDefinitionAndUnexpectedCells() {
            string line = "token\tI\treally\tlike\tcrepes\t";

            DialogueParser.ParsingMode expected = DialogueParser.ParsingMode.TOKEN_DEF;
            DialogueParser.ParsingMode actual = DialogueParser.ParsingModeFromLine(line);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ParseModeTagByCellExpectedValues() {
            string line = "name:Alex\tcontent:I love tests!\tsound:dialogue_0\tsoundbite:applause\t\t";

            DialogueParser.ParsingMode expected = DialogueParser.ParsingMode.TAG_BY_CELL;
            DialogueParser.ParsingMode actual = DialogueParser.ParsingModeFromLine(line);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ParseModeTagByCellFutureproofCells() {
            string line = "name:Alex\tcontent:I love tests!\tsound:dialogue_0\tsoundbite:applause\tanim:alex\t";

            DialogueParser.ParsingMode expected = DialogueParser.ParsingMode.TAG_BY_CELL;
            DialogueParser.ParsingMode actual = DialogueParser.ParsingModeFromLine(line);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ParseModeTagByCellUnexpectedCells() {
            string line =
                "name:Alex\tcontent:I love tests!\tsound:dialogue_0\tsoundbite:applause\tThey\tare\tquite\tdelicious\t\t\t\t";

            DialogueParser.ParsingMode expected = DialogueParser.ParsingMode.SKIP_LINE;
            DialogueParser.ParsingMode actual = DialogueParser.ParsingModeFromLine(line);

            Assert.AreEqual(expected, actual);
        }
    }

    public class TokenReplacementTests {
        [Test]
        public void ReplaceTokenSingle() {
            DialogueManifest.ClearTokenReplacements();
            DialogueManifest.AddTokenReplacement("user", "Amy");

            string input = "@user ate jamon.";

            string expected = "Amy ate jamon.";
            string actual = DialogueManifest.ReplaceTokensIn(input);
            
            Assert.AreEqual(expected, actual);
        }
        
        [Test]
        public void ReplaceTokenSingleRegularWordFirst() {
            DialogueManifest.ClearTokenReplacements();
            DialogueManifest.AddTokenReplacement("user", "Amy");

            string input = "Who ate jamon? @user ate jamon.";

            string expected = "Who ate jamon? Amy ate jamon.";
            string actual = DialogueManifest.ReplaceTokensIn(input);
            
            Assert.AreEqual(expected, actual);
        }
        
        [Test]
        public void ReplaceTokenSingleRightBeforePunctuation() {
            DialogueManifest.ClearTokenReplacements();
            DialogueManifest.AddTokenReplacement("user", "Amy");

            string input = "Who ate jamon? @user.";

            string expected = "Who ate jamon? Amy.";
            string actual = DialogueManifest.ReplaceTokensIn(input);
            
            Assert.AreEqual(expected, actual);
        }
        
        [Test]
        public void ReplaceTokensTwoUnique() {
            DialogueManifest.ClearTokenReplacements();
            DialogueManifest.AddTokenReplacement("user", "Amy");
            DialogueManifest.AddTokenReplacement("food", "jamon");

            string input = "@user ate @food.";

            string expected = "Amy ate jamon.";
            string actual = DialogueManifest.ReplaceTokensIn(input);
            
            Assert.AreEqual(expected, actual);
        }
        
        [Test]
        public void ReplaceTokensTwoAdjacent() {
            DialogueManifest.ClearTokenReplacements();
            DialogueManifest.AddTokenReplacement("user", "Amy");
            DialogueManifest.AddTokenReplacement("title", "Who Seeks Jamon");

            string input = "@user @title devoured jamon.";

            string expected = "Amy Who Seeks Jamon devoured jamon.";
            string actual = DialogueManifest.ReplaceTokensIn(input);
            
            Assert.AreEqual(expected, actual);
        }
        
        [Test]
        public void ReplaceTokensOnlyTokensManyAdjacent() {
            DialogueManifest.ClearTokenReplacements();
            DialogueManifest.AddTokenReplacement("user", "Amy");
            DialogueManifest.AddTokenReplacement("title", "Who Seeks Jamon");
            DialogueManifest.AddTokenReplacement("action", "devoured");
            DialogueManifest.AddTokenReplacement("food", "jamon");

            string input = "@user @title @action @food";

            string expected = "Amy Who Seeks Jamon devoured jamon";
            string actual = DialogueManifest.ReplaceTokensIn(input);
            
            Assert.AreEqual(expected, actual);
        }
    }
}