using NUnit.Framework;
using SocratesDialogue;
using UnityEngine;

public class SocraTSVTests {
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
        string line = "name:Alex\tcontent:I love tests!\tsound:dialogue_0\tsoundbite:applause\tThey\tare\tquite\tdelicious\t\t\t\t";

        DialogueParser.ParsingMode expected = DialogueParser.ParsingMode.SKIP_LINE;
        DialogueParser.ParsingMode actual = DialogueParser.ParsingModeFromLine(line);
        
        Assert.AreEqual(expected, actual);
    }
}