using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class SocraticVertexModifier : MonoBehaviour {
    int counter;

    float currentBetweenCharacterDelay;

    public Dialogue_Superclass.DialogueSection currentSection;

    bool muted;
    List<SocraticAnnotationParse> parses;
    TextMeshProUGUI textComponent;

    int totalVisibleCharacters;

    Vector3[] vertexPositions;

    void Start() {
        GetComponents();
    }

    void Update() {
        if (textComponent.text.Length <= 0) return;

        IncrementCharCounter(this);
        UpdateTextEmbellishes(textComponent, this);
    }
    //bool hasExecutedPostAction;

    internal TextMeshProUGUI TextComponent() {
        return textComponent;
    }

    /// <summary>
    ///     Returns whether all the text has been displayed. Only returns true if there's visible text in the text element.
    /// </summary>
    /// <returns></returns>
    internal bool TextHasBeenDisplayed() {
        return counter >= totalVisibleCharacters
               && totalVisibleCharacters > 0;
    }

    /// <summary>
    ///     Gets the text component attatched to the object.
    /// </summary>
    void GetComponents() {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    ///     Parses the text content inside of an input and sets the text as the parsed text.
    /// </summary>
    /// <param name="textContent"></param>
    /// <param name="textComponent"></param>
    /// <param name="vertexModifier"></param>
    public static void PrepareParsesAndSetText(
        string textContent,
        SocraticVertexModifier vertexModifier,
        bool displayFully = false,
        bool muted = true,
        Dialogue_Superclass.DialogueSection currentSection = null) {
        var textComponent = vertexModifier.GetComponent<TextMeshProUGUI>();

        vertexModifier.currentSection = currentSection;

        if (currentSection != null)
            if (currentSection.GetAction() != null)
                currentSection.TriggerAction(true);

        vertexModifier.muted = muted;
        vertexModifier.counter = 0;
        vertexModifier.currentBetweenCharacterDelay = 0;

        vertexModifier.parses = GetParses(textContent, 0);

        var parseOpeners = new List<SocraticAnnotationParse>();
        var parseClosers = new List<SocraticAnnotationParse>();

        foreach (var parse in vertexModifier.parses)
            if (parse.openingParse)
                parseOpeners.Add(parse);
            else
                parseClosers.Add(parse);

        foreach (var opener in parseOpeners)
        foreach (var closer in parseClosers)
            if (opener.richTextType == closer.richTextType && closer.linkedParse == null) {
                opener.linkedParse = closer;
                closer.linkedParse = opener;
                break;
            }

        var color = textComponent.color;

        //foreach (var item in GetPunctuationDelayParses(textContent, 0))
        //{
        //    vertexModifier.parses.Add(item);
        //}

        //TODO probably prone to break at some point
        textContent = SnipParses(textContent, vertexModifier);

        foreach (var item in GetPunctuationDelayParses(textContent, 0))
            //Debug.Log($"DON'T PANIC. LOCATION: {item.startCharacterLocation} {item.endCharacterLocation} val {item.dynamicValue}");
            vertexModifier.parses.Add(item);

        //textContent = SnipParses(textContent, vertexModifier);

        textComponent.text = textContent;

        textComponent.ForceMeshUpdate();

        var vertices = GetMaterialAtZero(textComponent.textInfo).vertices;
        vertexModifier.vertexPositions = new Vector3[vertices.Length];

        for (var v = 0; v < vertices.Length; v++) vertexModifier.vertexPositions[v] = vertices[v];

        vertexModifier.totalVisibleCharacters = textComponent.textInfo.characterCount;

        for (var i = 0; i < vertexModifier.totalVisibleCharacters; i++)
            SetColor(textComponent, i, ToggleAlpha(GetColorOfTopLeft(textComponent, i), true), i);

        textComponent.color = new Color(color.r, color.g, color.b, 0);

        if (displayFully) {
            var temp = Color.white;

            ColorUtility.TryParseHtmlString("#DDD0CC", out temp);

            textComponent.color = temp;
            // AudioManager.i.StopAllSourcesContains("dialogue", true);
            vertexModifier.counter = textComponent.maxVisibleCharacters;
        }
    }

    /// <summary>
    ///     Disects the text.
    /// </summary>
    /// <param name="contents"></param>
    /// <param name="vertexModifier"></param>
    /// <returns></returns>
    static string SnipParses(string contents, SocraticVertexModifier vertexModifier) {
        var totalCharactersSnipped = 0;

        foreach (var parsedSegment in vertexModifier.parses)
            //TODO work in progress
            if (parsedSegment.startCharacterLocation != parsedSegment.endCharacterLocation) {
                parsedSegment.startCharacterLocation -= totalCharactersSnipped;
                parsedSegment.endCharacterLocation -= totalCharactersSnipped;
                var start = parsedSegment.startCharacterLocation;
                var length = parsedSegment.endCharacterLocation - parsedSegment.startCharacterLocation + 1;
                contents = contents.Remove(start, length);
                totalCharactersSnipped += length;
            }

        return contents;
    }

    /// <summary>
    ///     Provides debug data breakdown for testing.
    /// </summary>
    /// <param name="sendInfoTo"></param>
    /// <param name="grabInfoFrom"></param>
    /// <param name="vertexMod"></param>
    static void UpdateDebugText(TextMeshProUGUI sendInfoTo, TextMeshProUGUI grabInfoFrom,
        SocraticVertexModifier vertexMod) {
        if (sendInfoTo == null) return;

        var debugParseInfo = "";

        foreach (var parse in vertexMod.parses)
            debugParseInfo +=
                $"{parse.startCharacterLocation}-{parse.endCharacterLocation} Opening: ({parse.openingParse})\n" +
                $"{parse.richTextType} Value: {parse.dynamicValue}\n";

        sendInfoTo.text = "" +
                          $"{(int)(1.0f / Time.deltaTime)} FPS\n" +
                          $"String Length: {grabInfoFrom.text.Length}\n" +
                          $"Display Counter: {vertexMod.counter}\n" +
                          $"Vertices: {grabInfoFrom.mesh.vertices.Length}\n" +
                          $"Character Count: {grabInfoFrom.textInfo.characterCount}\n" +
                          $"Line Count: {grabInfoFrom.textInfo.lineCount}\n" +
                          $"Word Count: {grabInfoFrom.textInfo.wordCount}\n" +
                          $"Parse Count: {vertexMod.parses.Count}\n" +
                          $"Total Visible Characters: {vertexMod.totalVisibleCharacters}\n" + debugParseInfo;
    }

    /// <summary>
    ///     Increments the counter and manages the volume of the sound being played.
    /// </summary>
    /// <param name="vertexModifier"></param>
    static void IncrementCharCounter(SocraticVertexModifier vertexModifier) {
        if (vertexModifier.counter >= vertexModifier.totalVisibleCharacters) {
            //        Debug.Log($"Finished display");
            if (vertexModifier.currentSection != null)
                //                Debug.Log($"Finished display and section is not null");
                if (!vertexModifier.currentSection.HasExecutedAction()) {
                    if (vertexModifier.currentSection.GetAction() != null)
                        //Debug.Log($"Should trigger once, hasExecutedPostAction: {vertexModifier.currentSection.HasExecutedAction()}");
                        vertexModifier.currentSection.TriggerAction(false);

                    vertexModifier.currentSection.SetActionExecution(true);
                }

            if (vertexModifier.currentSection != null && !vertexModifier.muted) {
                // AudioManager.i.StopAllSources(vertexModifier.currentSection.GetDialogueSound(), true);
            }
            else if (!vertexModifier.muted) {
                // AudioManager.i.StopAllSources("dialogue", true);
            }

            return;
        }

        if (vertexModifier.currentBetweenCharacterDelay <= 0) {
            vertexModifier.counter++;

            if (!vertexModifier.muted)
                if (vertexModifier.currentSection != null) {
                    // AudioManager.i.PlayOnlyIfDone(vertexModifier.currentSection.GetDialogueSound());
                }

            // AudioManager.i.PlayOnlyIfDone("dialogue");
            vertexModifier.currentBetweenCharacterDelay = SocraticAnnotation.display_textDelay;

            foreach (var parse in vertexModifier.parses)
                //if (parse.openingParse && 
                //    parse.startCharacterLocation == vertexModifier.counter && 
                //    parse.richTextType == SocraticAnnotation.RichTextType.WAVE)
                //{
                //    AudioManager.i.Play("wave_flourish");
                //}
                if (parse.richTextType == SocraticAnnotation.RichTextType.DELAY) {
                    if (parse.openingParse && parse.startCharacterLocation == vertexModifier.counter &&
                        !parse.executedAction) {
                        if (vertexModifier.currentSection != null) {
                            // AudioManager.i.StopAllSources(vertexModifier.currentSection.GetDialogueSound(), true);
                        }

                        // AudioManager.i.StopAllSources("dialogue", true);
                        vertexModifier.currentBetweenCharacterDelay = parse.GetDynamicValueAsFloat();
                        parse.executedAction = true;

                        OnCharDelay();
                    }
                    else if (parse.openingParse && parse.startCharacterLocation == vertexModifier.counter - 1 &&
                             parse.executedAction) {
                        OnPostCharDelay();
                    }
                }
        }

        vertexModifier.currentBetweenCharacterDelay -= Time.deltaTime;
        //vertexModifier.currentBetweenCharacterDelay -= Time.fixedDeltaTime;
    }

    /// <summary>
    ///     Occurs when the character delay beings.
    /// </summary>
    public static void OnCharDelay() {
    }

    /// <summary>
    ///     Occurs when the character delay finishes.
    /// </summary>
    public static void OnPostCharDelay() {
    }

    /// <summary>
    ///     Gets the material at index zero for a given textInfo.
    /// </summary>
    /// <param name="textInfo"></param>
    /// <returns></returns>
    static TMP_MeshInfo GetMaterialAtZero(TMP_TextInfo textInfo) {
        return textInfo.meshInfo[0];
    }

    /// <summary>
    ///     Sets all four vertices of a given character to a color.
    /// </summary>
    /// <param name="textComponent"></param>
    /// <param name="charIndex"></param>
    /// <param name="color"></param>
    /// <param name="i"></param>
    static void SetColor(TextMeshProUGUI textComponent, int charIndex, Color32 color, int i) {
        var meshIndex = textComponent.textInfo.characterInfo[charIndex].materialReferenceIndex;
        var vertexIndex = textComponent.textInfo.characterInfo[charIndex].vertexIndex;

        if (vertexIndex == 0 && i != 0) return;

        var vertexColors = textComponent.textInfo.meshInfo[meshIndex].colors32;
        vertexColors[vertexIndex + 0] = color;
        vertexColors[vertexIndex + 1] = color;
        vertexColors[vertexIndex + 2] = color;
        vertexColors[vertexIndex + 3] = color;

        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    /// <summary>
    ///     Gets color of top left vertex.
    /// </summary>
    /// <param name="textComponent"></param>
    /// <param name="charIndex"></param>
    /// <returns></returns>
    static Color GetColorOfTopLeft(TextMeshProUGUI textComponent, int charIndex) {
        var meshIndex = textComponent.textInfo.characterInfo[charIndex].materialReferenceIndex;
        var vertexIndex = textComponent.textInfo.characterInfo[charIndex].vertexIndex;

        var vertexColors = textComponent.textInfo.meshInfo[meshIndex].colors32;

        return vertexColors[vertexIndex + 0];
    }

    /// <summary>
    ///     Returns the color given with the alpha toggled on or off.
    /// </summary>
    /// <param name="color"></param>
    /// <param name="hidden"></param>
    /// <returns></returns>
    static Color ToggleAlpha(Color color, bool hidden) {
        return new Color(color.r, color.g, color.b, hidden ? 0F : 1F);
    }

    /// <summary>
    ///     Manages the visible characters and calls the rich text to be updated.
    /// </summary>
    /// <param name="textComponent"></param>
    /// <param name="vertexMod"></param>
    public static void UpdateTextEmbellishes(TextMeshProUGUI textComponent, SocraticVertexModifier vertexMod) {
        var textInfo = textComponent.textInfo;

        var newVertexPositions = GetMaterialAtZero(textInfo).vertices;

        ScrollInFromY(vertexMod, textInfo, newVertexPositions);

        ApplyRichText(textComponent, vertexMod, textInfo, newVertexPositions);

        //        int start = 0;
        var start = vertexMod.counter - 15;

        if (start < 0) start = 0;

        //if (vertexMod.counter >= 15)
        //{
        //    start = 15;
        //}

        if (vertexMod.counter <= vertexMod.totalVisibleCharacters)
            //            for (int i = vertexMod.counter - start; i < vertexMod.counter; i++)
            for (var i = start; i < vertexMod.counter; i++)
                SetColor(textComponent, i, ToggleAlpha(GetColorOfTopLeft(textComponent, i), false), i);

        var meshInfo = GetMaterialAtZero(textComponent.textInfo);

        textComponent.mesh.SetVertices(newVertexPositions);

        if (meshInfo.uvs0 != null) {
            textComponent.mesh.uv = FromVector4Arr(meshInfo.uvs0);
            textComponent.mesh.uv2 = meshInfo.uvs2;

            for (var i = 0; i < 1; i++) textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);    
        }
    }

    /// <summary>
    ///     Makes the characters come from below with a log function.
    /// </summary>
    /// <param name="vertexMod"></param>
    /// <param name="textInfo"></param>
    /// <param name="newVertexPositions"></param>
    static void ScrollInFromY(SocraticVertexModifier vertexMod, TMP_TextInfo textInfo,
        Vector3[] newVertexPositions) {
        //Under construction

        //Under construction
    }

    /// <summary>
    ///     Applies the rich text based on what the parse means.
    /// </summary>
    /// <param name="textComponent"></param>
    /// <param name="vertexMod"></param>
    /// <param name="textInfo"></param>
    /// <param name="newVertexPositions"></param>
    static void ApplyRichText(TextMeshProUGUI textComponent, SocraticVertexModifier vertexMod,
        TMP_TextInfo textInfo, Vector3[] newVertexPositions) {
        if (vertexMod.parses == null) return;

        foreach (var parse in vertexMod.parses)
            if (parse.openingParse)
                switch (parse.richTextType) {
                    case SocraticAnnotation.RichTextType.WAVE:
                        ApplyRichTextWave(textInfo, vertexMod.vertexPositions, parse, newVertexPositions);
                        break;
                    case SocraticAnnotation.RichTextType.COLOR:
                        ApplyRichTextColor(textComponent, parse);
                        break;
                    case SocraticAnnotation.RichTextType.SHAKE:
                        ApplyRichTextShake(textComponent, vertexMod.vertexPositions, parse, newVertexPositions);
                        break;
                }
    }

    /// <summary>
    ///     I don't know much about this but I know it works. Curtisy of TextMeshPro.
    /// </summary>
    /// <param name="textComponent"></param>
    /// <param name="vertexPositionsReadFrom"></param>
    /// <param name="parse"></param>
    /// <param name="vertexPositionsWriteTo"></param>
    static void ApplyRichTextShake(TextMeshProUGUI textComponent, Vector3[] vertexPositionsReadFrom,
        SocraticAnnotationParse parse, Vector3[] vertexPositionsWriteTo) {
        var textInfo = textComponent.textInfo;

        var vertexAnim = new VertexAnim[1024];

        for (var i = 0; i < 1024; i++) {
            vertexAnim[i].angleRange = Random.Range(10f, 25f);
            vertexAnim[i].speed = Random.Range(1f, 3f);
        }

        for (var i = parse.startCharacterLocation; i < parse.linkedParse.startCharacterLocation; i++) {
            var charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible)
                continue;

            var vertAnim = vertexAnim[i];

            var materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

            var vertexIndex = textInfo.characterInfo[i].vertexIndex;

            if (vertexIndex == 0 && i != 0) continue;

            var cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

            var sourceVertices = vertexPositionsReadFrom;

            Vector2 charMidBasline = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

            Vector3 offset = charMidBasline;

            var destinationVertices = textInfo.meshInfo[materialIndex].vertices;

            vertexPositionsWriteTo[vertexIndex + 0] = sourceVertices[vertexIndex + 0] - offset;
            vertexPositionsWriteTo[vertexIndex + 1] = sourceVertices[vertexIndex + 1] - offset;
            vertexPositionsWriteTo[vertexIndex + 2] = sourceVertices[vertexIndex + 2] - offset;
            vertexPositionsWriteTo[vertexIndex + 3] = sourceVertices[vertexIndex + 3] - offset;

            var AngleMultiplier = 1.0F; //how much it rotates
            var CurveScale = parse.GetDynamicValueAsFloat(); //noticability
            var loopCount = 1.0F; //don't know

            vertAnim.angle = Mathf.SmoothStep(-vertAnim.angleRange, vertAnim.angleRange,
                Mathf.PingPong(loopCount / 25f * vertAnim.speed, 1f));
            var jitterOffset = new Vector3(Random.Range(-.25f, .25f),
                Random.Range(-.25f, .25f), 0);
            var matrix = Matrix4x4.TRS(jitterOffset * CurveScale,
                Quaternion.Euler(0, 0, Random.Range(-5f, 5f) * AngleMultiplier), Vector3.one);

            vertexPositionsWriteTo[vertexIndex + 0] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 0]);
            vertexPositionsWriteTo[vertexIndex + 1] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 1]);
            vertexPositionsWriteTo[vertexIndex + 2] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 2]);
            vertexPositionsWriteTo[vertexIndex + 3] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 3]);

            vertexPositionsWriteTo[vertexIndex + 0] += offset;
            vertexPositionsWriteTo[vertexIndex + 1] += offset;
            vertexPositionsWriteTo[vertexIndex + 2] += offset;
            vertexPositionsWriteTo[vertexIndex + 3] += offset;
        }

        for (var i = 0; i < textInfo.meshInfo.Length; i++) {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }

    /// <summary>
    ///     Applies the rich text setting color.
    /// </summary>
    /// <param name="textComponent"></param>
    /// <param name="parse"></param>
    static void ApplyRichTextColor(TextMeshProUGUI textComponent, SocraticAnnotationParse parse) {
        for (var i = parse.startCharacterLocation; i < parse.linkedParse.startCharacterLocation; i++)
            SetColor(textComponent, i, parse.GetDynamicValueAsColor(GetColorOfTopLeft(textComponent, i).a), i);

        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    /// <summary>
    ///     Applies the rich text setting the wave positions.
    /// </summary>
    /// <param name="textInfo"></param>
    /// <param name="vertexPositionsReadFrom"></param>
    /// <param name="parse"></param>
    /// <param name="vertexPositionsWriteTo"></param>
    static void ApplyRichTextWave(TMP_TextInfo textInfo, Vector3[] vertexPositionsReadFrom,
        SocraticAnnotationParse parse, Vector3[] vertexPositionsWriteTo) {
        var wave_speed = parse.ContainsDynamicValue()
            ? parse.GetDynamicValueAsFloat()
            : SocraticAnnotation.wave_speed;

        for (var i = parse.startCharacterLocation; i < parse.linkedParse.startCharacterLocation; i++) {
            var vertexIndex = textInfo.characterInfo[i].vertexIndex;

            if (vertexIndex == 0 && i != 0)
                //Debug.Log($"Vertex index is zero? {parse.startCharacterLocation}");
                continue;

            var leftVerticesXPos = vertexPositionsReadFrom[vertexIndex + 0].x;
            var rightVerticesXPos = vertexPositionsReadFrom[vertexIndex + 2].x;

            var leftOffsetY = Mathf.Sin(
                Time.timeSinceLevelLoad * wave_speed +
                leftVerticesXPos * SocraticAnnotation.wave_freqMultiplier) * SocraticAnnotation.wave_amplitude;

            var rightOffsetY = SocraticAnnotation.wave_warpTextVerticies
                ? Mathf.Sin(Time.timeSinceLevelLoad * wave_speed +
                            rightVerticesXPos * SocraticAnnotation.wave_freqMultiplier) *
                  SocraticAnnotation.wave_amplitude
                : leftOffsetY;

            vertexPositionsWriteTo[vertexIndex + 0].y = vertexPositionsReadFrom[vertexIndex + 0].y + leftOffsetY;
            vertexPositionsWriteTo[vertexIndex + 1].y = vertexPositionsReadFrom[vertexIndex + 1].y + leftOffsetY;

            vertexPositionsWriteTo[vertexIndex + 2].y = vertexPositionsReadFrom[vertexIndex + 2].y + rightOffsetY;
            vertexPositionsWriteTo[vertexIndex + 3].y = vertexPositionsReadFrom[vertexIndex + 3].y + rightOffsetY;
        }
    }

    /// <summary>
    ///     Gets the parses and analyses them to determine their type.
    /// </summary>
    /// <param name="inputText"></param>
    /// <param name="startAt"></param>
    /// <param name="preFilled"></param>
    /// <returns></returns>
    internal static List<SocraticAnnotationParse> GetParses(string inputText, int startAt,
        List<SocraticAnnotationParse> preFilled = null) {
        var result = new List<SocraticAnnotationParse>();

        if (preFilled != null) result = preFilled;

        SocraticAnnotationParse parseProfile = null;

        for (var i = startAt; i < inputText.Length; i++)
            if (inputText[i] == SocraticAnnotation.parse_startParse) {
                parseProfile = new SocraticAnnotationParse();
                parseProfile.startCharacterLocation = i;

                var compareProfileTo = "";
                var dynamicValue = "";
                var readingDynamicValue = false;

                for (var f = i + 1; f < inputText.Length; f++) {
                    if (inputText[f] == SocraticAnnotation.parse_endParse) {
                        parseProfile.endCharacterLocation = f;

                        if (compareProfileTo.Contains(SocraticAnnotation.wave_parseInfo))
                            parseProfile.richTextType = SocraticAnnotation.RichTextType.WAVE;
                        else if (compareProfileTo.Contains(SocraticAnnotation.color_parseInfo))
                            parseProfile.richTextType = SocraticAnnotation.RichTextType.COLOR;
                        else if (compareProfileTo.Contains(SocraticAnnotation.delay_parseInfo))
                            parseProfile.richTextType = SocraticAnnotation.RichTextType.DELAY;
                        else if (compareProfileTo.Contains(SocraticAnnotation.shake_parseInfo))
                            parseProfile.richTextType = SocraticAnnotation.RichTextType.SHAKE;
                        else if (compareProfileTo.Contains(SocraticAnnotation.italic_parseInfo))
                            parseProfile.richTextType = SocraticAnnotation.RichTextType.ITALIC;
                        else
                            Debug.LogError($"'{compareProfileTo}' -- Parse section did not have a valid input.");

                        var contents = "";

                        for (var c = i; c < f; c++) contents += inputText[c];

                        if (contents.Contains(SocraticAnnotation.parse_endParsePair)) parseProfile.openingParse = false;

                        if (readingDynamicValue) parseProfile.dynamicValue = dynamicValue;

                        result.Add(parseProfile);

                        return GetParses(inputText, f, result);
                    }

                    compareProfileTo += inputText[f];

                    if (readingDynamicValue) dynamicValue += inputText[f];

                    if (inputText[f] == SocraticAnnotation.parse_inputValueIndicator) readingDynamicValue = true;
                }
            }

        if (result == null) Debug.LogError("The result for the list of parses is null.");

        return result;
    }

    internal static List<SocraticAnnotationParse> GetPunctuationDelayParses(string inputText, int startAt,
        List<SocraticAnnotationParse> preFilled = null) {
        var result = new List<SocraticAnnotationParse>();

        if (preFilled != null) result = preFilled;

        SocraticAnnotationParse parseProfile = null;

        for (var i = startAt; i < inputText.Length; i++)
            if (char.IsPunctuation(inputText[i]) && i < inputText.Length - 1) //TODO working
            {
                var c = inputText[i];

                var skip = false;

                if (i > 1)
                    if (inputText[i - 1] == 'r' && inputText[i - 2] == 'M') //TODO this fix is only temporary
                        skip = true;

                if (inputText[i + 1] == ' ' && !skip)
                    if (c == ',' ||
                        c == '.' ||
                        c == '?' ||
                        c == '!' ||
                        c == '~' ||
                        c == ':' ||
                        c == ':' ||
                        c == '(' ||
                        c == ')' ||
                        c == ';') {
                        parseProfile = new SocraticAnnotationParse();
                        parseProfile.startCharacterLocation = i + 1;
                        parseProfile.endCharacterLocation = i + 1;
                        parseProfile.richTextType = SocraticAnnotation.RichTextType.DELAY;
                        parseProfile.dynamicValue = c == ','
                            ? $"{SocraticAnnotation.display_minorPunctuationDelay}"
                            : $"{SocraticAnnotation.display_majorPunctuationDelay}";

                        result.Add(parseProfile);
                    }
            }

        if (result == null) Debug.LogError("The result for the list of parses is null.");

        return result;
    }

    public static Vector2[] FromVector4Arr(Vector4[] input) {
        var result = new Vector2[input.Length];

        for (var i = 0; i < input.Length; i++) result[i] = new Vector2(input[i].x, input[i].y);

        return result;
    }

    struct VertexAnim {
        public float angleRange;
        public float angle;
        public float speed;
    }
}