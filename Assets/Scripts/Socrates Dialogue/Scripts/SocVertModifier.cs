using UnityEngine;
using TMPro;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class SocVertModifier : MonoBehaviour {
    private TextMeshProUGUI textComponent;
    List<SocraticAnnotationParse> parses;

    private int totalVisibleCharacters;
    private int counter = 0;

    public DialogueSuperclass.DialogueSection currentSection;
    //bool hasExecutedPostAction;

    internal TextMeshProUGUI TextComponent() {
        return textComponent;
    }

    /// <summary>
    /// Returns whether all the text has been displayed. Only returns true if there's visible text in the text element.
    /// </summary>
    /// <returns></returns>
    internal bool TextHasBeenDisplayed() {
        return counter >= totalVisibleCharacters
               && totalVisibleCharacters > 0;
    }

    private float currentBetweenCharacterDelay;

    Vector3[] vertexPositions;

    bool muted;

    static Dictionary<string, MultiAudioSource> dialogueSfx = new();
    static MultiAudioSource currentDialogueSfx = null;

    public void SetDialogueSfx(string soundName) {
        if (!dialogueSfx.ContainsKey(soundName)) {
            dialogueSfx.Add(soundName, MultiAudioSource.FromResource(gameObject, soundName));
        }
        
        currentDialogueSfx = dialogueSfx[soundName];
    }
    
    void Start() {
        GetComponents();
    }

    /// <summary>
    /// Gets the text component attatched to the object.
    /// </summary>
    private void GetComponents() {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// Parses the text content inside of an input and sets the text as the parsed text.
    /// </summary>
    /// <param name="textContent"></param>
    /// <param name="textComponent"></param>
    /// <param name="vertexModifier"></param>
    public static void ParseAndSetText(
        string textContent,
        SocVertModifier vertexModifier,
        bool displayFully = false,
        bool muted = true,
        DialogueSuperclass.DialogueSection currentSection = null) {
        TextMeshProUGUI textComponent = vertexModifier.GetComponent<TextMeshProUGUI>();

        vertexModifier.currentSection = currentSection;

        if (currentSection != null) {
            if (currentSection.GetAction() != null) {
                currentSection.TriggerAction(true);
            }
        }

        vertexModifier.muted = muted;
        vertexModifier.counter = 0;
        vertexModifier.currentBetweenCharacterDelay = 0;

        vertexModifier.parses = GetParses(textContent, 0);

        List<SocraticAnnotationParse> parseOpeners = new List<SocraticAnnotationParse>();
        List<SocraticAnnotationParse> parseClosers = new List<SocraticAnnotationParse>();

        foreach (var parse in vertexModifier.parses) {
            if (parse.openingParse) {
                parseOpeners.Add(parse);
            } else {
                parseClosers.Add(parse);
            }
        }

        foreach (var opener in parseOpeners) {
            foreach (var closer in parseClosers) {
                if (opener.richTextType == closer.richTextType && closer.linkedParse == null) {
                    opener.linkedParse = closer;
                    closer.linkedParse = opener;
                    break;
                }
            }
        }

        Color color = textComponent.color;

        //foreach (var item in GetPunctuationDelayParses(textContent, 0))
        //{
        //    vertexModifier.parses.Add(item);
        //}

        //TODO probably prone to break at some point
        textContent = SnipParses(textContent, vertexModifier);

        foreach (var item in GetPunctuationDelayParses(textContent, 0)) {
            //Debug.Log($"DON'T PANIC. LOCATION: {item.startCharacterLocation} {item.endCharacterLocation} val {item.dynamicValue}");
            vertexModifier.parses.Add(item);
        }

        //textContent = SnipParses(textContent, vertexModifier);

        textComponent.text = textContent;

        textComponent.ForceMeshUpdate();

        Vector3[] vertices = GetMaterialAtZero(textComponent.textInfo).vertices;
        vertexModifier.vertexPositions = new Vector3[vertices.Length];

        for (int v = 0; v < vertices.Length; v++) {
            vertexModifier.vertexPositions[v] = vertices[v];
        }

        vertexModifier.totalVisibleCharacters = textComponent.textInfo.characterCount;

        for (int i = 0; i < vertexModifier.totalVisibleCharacters; i++) {
            SetColor(textComponent, i, ToggleAlpha(GetColorOfTopLeft(textComponent, i), true), i);
        }

        textComponent.color = new Color(color.r, color.g, color.b, 0);

        if (displayFully) {
            Color temp = Color.white;

            ColorUtility.TryParseHtmlString("#DDD0CC", out temp);

            textComponent.color = temp;
            // AudioManager.i.StopAllSourcesContains("dialogue", true);
            if (currentDialogueSfx != null) {
                currentDialogueSfx.Stop(); 
            }
            
            vertexModifier.counter = textComponent.maxVisibleCharacters;
        }
    }

    /// <summary>
    /// Disects the text.
    /// </summary>
    /// <param name="contents"></param>
    /// <param name="vertexModifier"></param>
    /// <returns></returns>
    private static string SnipParses(string contents, SocVertModifier vertexModifier) {
        int totalCharactersSnipped = 0;

        foreach (var parsedSegment in vertexModifier.parses) {
            //TODO work in progress
            if (parsedSegment.startCharacterLocation != parsedSegment.endCharacterLocation) {
                parsedSegment.startCharacterLocation -= totalCharactersSnipped;
                parsedSegment.endCharacterLocation -= totalCharactersSnipped;
                int start = parsedSegment.startCharacterLocation;
                int length = (parsedSegment.endCharacterLocation - parsedSegment.startCharacterLocation) + 1;
                contents = contents.Remove(start, length);
                totalCharactersSnipped += length;
            }
        }

        return contents;
    }

    private void Update() {
        if (textComponent.text.Length <= 0) {
            return;
        }

        IncrementCharCounter(this);
        UpdateTextEmbellishes(textComponent, this);
    }

    /// <summary>
    /// Provides debug data breakdown for testing.
    /// </summary>
    /// <param name="sendInfoTo"></param>
    /// <param name="grabInfoFrom"></param>
    /// <param name="vertexMod"></param>
    private static void UpdateDebugText(TextMeshProUGUI sendInfoTo, TextMeshProUGUI grabInfoFrom,
        SocVertModifier vertexMod) {
        if (sendInfoTo == null) {
            return;
        }

        string debugParseInfo = "";

        foreach (var parse in vertexMod.parses) {
            debugParseInfo +=
                $"{parse.startCharacterLocation}-{parse.endCharacterLocation} Opening: ({parse.openingParse})\n" +
                $"{parse.richTextType} Value: {parse.dynamicValue}\n";
        }

        sendInfoTo.text = $"" +
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
    /// Increments the counter and manages the volume of the sound being played.
    /// </summary>
    /// <param name="vertexModifier"></param>
    private static void IncrementCharCounter(SocVertModifier vertexModifier) {
        if (vertexModifier.counter >= vertexModifier.totalVisibleCharacters) {
            //        Debug.Log($"Finished display");
            if (vertexModifier.currentSection != null) {
                //                Debug.Log($"Finished display and section is not null");

                if (!vertexModifier.currentSection.HasExecutedAction()) {
                    if (vertexModifier.currentSection.GetAction() != null) {
                        //Debug.Log($"Should trigger once, hasExecutedPostAction: {vertexModifier.currentSection.HasExecutedAction()}");
                        vertexModifier.currentSection.TriggerAction(false);
                    }

                    vertexModifier.currentSection.SetActionExecution(true);
                }
            }

            if (vertexModifier.currentSection != null && !vertexModifier.muted) {
                // AudioManager.i.StopAllSources(vertexModifier.currentSection.GetDialogueSound(), true);
                if (currentDialogueSfx != null) {
                    currentDialogueSfx.Stop();
                }
            } else if (!vertexModifier.muted) {
                // AudioManager.i.StopAllSources("dialogue", true);
                if (currentDialogueSfx != null) {
                    currentDialogueSfx.Stop();
                }
            }

            return;
        }

        if (vertexModifier.currentBetweenCharacterDelay <= 0) {
            vertexModifier.counter++;

            if (!vertexModifier.muted) {
                if (vertexModifier.currentSection != null) {
                    if (currentDialogueSfx != null) {
                        currentDialogueSfx.PlayIfDone();
                    }
                } else {
                    if (currentDialogueSfx != null) {
                        currentDialogueSfx.PlayIfDone();
                    }
                }
            }

            vertexModifier.currentBetweenCharacterDelay = SocraticAnnotation.displayTextDelay;

            foreach (var parse in vertexModifier.parses) {
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
                            if (currentDialogueSfx != null) {
                                currentDialogueSfx.Stop();
                            }
                        } else {
                            if (currentDialogueSfx != null) {
                                currentDialogueSfx.Stop();
                            }
                        }

                        vertexModifier.currentBetweenCharacterDelay = parse.GetDynamicValueAsFloat();
                        parse.executedAction = true;

                        OnCharDelay();
                    } else if (parse.openingParse && parse.startCharacterLocation == vertexModifier.counter - 1 &&
                               parse.executedAction) {
                        OnPostCharDelay();
                    }
                }
            }
        }

        vertexModifier.currentBetweenCharacterDelay -= Time.deltaTime;
        //vertexModifier.currentBetweenCharacterDelay -= Time.fixedDeltaTime;
    }

    /// <summary>
    /// Occurs when the character delay beings.
    /// </summary>
    public static void OnCharDelay() {
    }

    /// <summary>
    /// Occurs when the character delay finishes.
    /// </summary>
    public static void OnPostCharDelay() {
    }

    /// <summary>
    /// Gets the material at index zero for a given textInfo.
    /// </summary>
    /// <param name="textInfo"></param>
    /// <returns></returns>
    private static TMP_MeshInfo GetMaterialAtZero(TMP_TextInfo textInfo) {
        return textInfo.meshInfo[0];
    }

    /// <summary>
    /// Sets all four vertices of a given character to a color.
    /// </summary>
    /// <param name="textComponent"></param>
    /// <param name="charIndex"></param>
    /// <param name="color"></param>
    /// <param name="i"></param>
    private static void SetColor(TextMeshProUGUI textComponent, int charIndex, Color32 color, int i) {
        int meshIndex = textComponent.textInfo.characterInfo[charIndex].materialReferenceIndex;
        int vertexIndex = textComponent.textInfo.characterInfo[charIndex].vertexIndex;

        if (vertexIndex == 0 && i != 0) {
            return;
        }

        Color32[] vertexColors = textComponent.textInfo.meshInfo[meshIndex].colors32;
        vertexColors[vertexIndex + 0] = color;
        vertexColors[vertexIndex + 1] = color;
        vertexColors[vertexIndex + 2] = color;
        vertexColors[vertexIndex + 3] = color;

        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    /// <summary>
    /// Gets color of top left vertex.
    /// </summary>
    /// <param name="textComponent"></param>
    /// <param name="charIndex"></param>
    /// <returns></returns>
    private static Color GetColorOfTopLeft(TextMeshProUGUI textComponent, int charIndex) {
        int meshIndex = textComponent.textInfo.characterInfo[charIndex].materialReferenceIndex;
        int vertexIndex = textComponent.textInfo.characterInfo[charIndex].vertexIndex;

        Color32[] vertexColors = textComponent.textInfo.meshInfo[meshIndex].colors32;

        return vertexColors[vertexIndex + 0];
    }

    /// <summary>
    /// Returns the color given with the alpha toggled on or off. 
    /// </summary>
    /// <param name="color"></param>
    /// <param name="hidden"></param>
    /// <returns></returns>
    private static Color ToggleAlpha(Color color, bool hidden) {
        return new Color(color.r, color.g, color.b, hidden ? 0F : 1F);
    }

    /// <summary>
    /// Manages the visible characters and calls the rich text to be updated.
    /// </summary>
    /// <param name="textComponent"></param>
    /// <param name="vertexMod"></param>
    public static void UpdateTextEmbellishes(TextMeshProUGUI textComponent, SocVertModifier vertexMod) {
        TMP_TextInfo textInfo = textComponent.textInfo;

        Vector3[] newVertexPositions = GetMaterialAtZero(textInfo).vertices;

        ScrollInFromY(vertexMod, textInfo, newVertexPositions);

        ApplyRichText(textComponent, vertexMod, textInfo, newVertexPositions);

        //        int start = 0;
        int start = vertexMod.counter - 15;

        if (start < 0) start = 0;

        //if (vertexMod.counter >= 15)
        //{
        //    start = 15;
        //}

        if (vertexMod.counter <= vertexMod.totalVisibleCharacters) {
            //            for (int i = vertexMod.counter - start; i < vertexMod.counter; i++)
            for (int i = start; i < vertexMod.counter; i++) {
                SetColor(textComponent, i, ToggleAlpha(GetColorOfTopLeft(textComponent, i), false), i);
            }
        }

        TMP_MeshInfo meshInfo = GetMaterialAtZero(textComponent.textInfo);

        textComponent.mesh.SetVertices(newVertexPositions);

        if (meshInfo.uvs0 != null) {
            textComponent.mesh.uv = FromVector4Arr(meshInfo.uvs0);
            textComponent.mesh.uv2 = meshInfo.uvs2;

            for (int i = 0; i < 1; i++) {
                textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
            
            textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }
    }

    /// <summary>
    /// Makes the characters come from below with a log function.
    /// </summary>
    /// <param name="vertexMod"></param>
    /// <param name="textInfo"></param>
    /// <param name="newVertexPositions"></param>
    private static void ScrollInFromY(SocVertModifier vertexMod, TMP_TextInfo textInfo,
        Vector3[] newVertexPositions) {
        //Under construction

        //Under construction
    }

    /// <summary>
    /// Applies the rich text based on what the parse means.
    /// </summary>
    /// <param name="textComponent"></param>
    /// <param name="vertexMod"></param>
    /// <param name="textInfo"></param>
    /// <param name="newVertexPositions"></param>
    private static void ApplyRichText(TextMeshProUGUI textComponent, SocVertModifier vertexMod,
        TMP_TextInfo textInfo, Vector3[] newVertexPositions) {
        if (vertexMod.parses == null) {
            return;
        }

        foreach (var parse in vertexMod.parses) {
            if (parse.openingParse) {
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
        }
    }

    private struct VertexAnim {
        public float angleRange;
        public float angle;
        public float speed;
    }

    /// <summary>
    /// I don't know much about this but I know it works. Curtisy of TextMeshPro.
    /// </summary>
    /// <param name="textComponent"></param>
    /// <param name="vertexPositionsReadFrom"></param>
    /// <param name="parse"></param>
    /// <param name="vertexPositionsWriteTo"></param>
    private static void ApplyRichTextShake(TextMeshProUGUI textComponent, Vector3[] vertexPositionsReadFrom,
        SocraticAnnotationParse parse, Vector3[] vertexPositionsWriteTo) {
        TMP_TextInfo textInfo = textComponent.textInfo;

        VertexAnim[] vertexAnim = new VertexAnim[1024];

        for (int i = 0; i < 1024; i++) {
            vertexAnim[i].angleRange = UnityEngine.Random.Range(10f, 25f);
            vertexAnim[i].speed = UnityEngine.Random.Range(1f, 3f);
        }

        for (int i = parse.startCharacterLocation; i < parse.linkedParse.startCharacterLocation; i++) {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible)
                continue;

            VertexAnim vertAnim = vertexAnim[i];

            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

            int vertexIndex = textInfo.characterInfo[i].vertexIndex;

            if (vertexIndex == 0 && i != 0) {
                continue;
            }

            TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

            Vector3[] sourceVertices = vertexPositionsReadFrom;

            Vector2 charMidBasline = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

            Vector3 offset = charMidBasline;

            Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

            vertexPositionsWriteTo[vertexIndex + 0] = sourceVertices[vertexIndex + 0] - offset;
            vertexPositionsWriteTo[vertexIndex + 1] = sourceVertices[vertexIndex + 1] - offset;
            vertexPositionsWriteTo[vertexIndex + 2] = sourceVertices[vertexIndex + 2] - offset;
            vertexPositionsWriteTo[vertexIndex + 3] = sourceVertices[vertexIndex + 3] - offset;

            float AngleMultiplier = 1.0F; //how much it rotates
            float CurveScale = parse.GetDynamicValueAsFloat(); //noticability
            float loopCount = 1.0F; //don't know

            vertAnim.angle = Mathf.SmoothStep(-vertAnim.angleRange, vertAnim.angleRange,
                Mathf.PingPong(loopCount / 25f * vertAnim.speed, 1f));
            Vector3 jitterOffset = new Vector3(UnityEngine.Random.Range(-.25f, .25f),
                UnityEngine.Random.Range(-.25f, .25f), 0);
            Matrix4x4 matrix = Matrix4x4.TRS(jitterOffset * CurveScale,
                Quaternion.Euler(0, 0, UnityEngine.Random.Range(-5f, 5f) * AngleMultiplier), Vector3.one);

            vertexPositionsWriteTo[vertexIndex + 0] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 0]);
            vertexPositionsWriteTo[vertexIndex + 1] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 1]);
            vertexPositionsWriteTo[vertexIndex + 2] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 2]);
            vertexPositionsWriteTo[vertexIndex + 3] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 3]);

            vertexPositionsWriteTo[vertexIndex + 0] += offset;
            vertexPositionsWriteTo[vertexIndex + 1] += offset;
            vertexPositionsWriteTo[vertexIndex + 2] += offset;
            vertexPositionsWriteTo[vertexIndex + 3] += offset;
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++) {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }

    /// <summary>
    /// Applies the rich text setting color.
    /// </summary>
    /// <param name="textComponent"></param>
    /// <param name="parse"></param>
    private static void ApplyRichTextColor(TextMeshProUGUI textComponent, SocraticAnnotationParse parse) {
        for (int i = parse.startCharacterLocation; i < parse.linkedParse.startCharacterLocation; i++) {
            SetColor(textComponent, i, parse.GetDynamicValueAsColor(GetColorOfTopLeft(textComponent, i).a), i);
        }

        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    /// <summary>
    /// Applies the rich text setting the wave positions.
    /// </summary>
    /// <param name="textInfo"></param>
    /// <param name="vertexPositionsReadFrom"></param>
    /// <param name="parse"></param>
    /// <param name="vertexPositionsWriteTo"></param>
    private static void ApplyRichTextWave(TMP_TextInfo textInfo, Vector3[] vertexPositionsReadFrom,
        SocraticAnnotationParse parse, Vector3[] vertexPositionsWriteTo) {
        float wave_speed = parse.ContainsDynamicValue()
            ? parse.GetDynamicValueAsFloat()
            : SocraticAnnotation.waveSpeed;

        for (int i = parse.startCharacterLocation; i < parse.linkedParse.startCharacterLocation; i++) {
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;

            if (vertexIndex == 0 && i != 0) {
                //Debug.Log($"Vertex index is zero? {parse.startCharacterLocation}");
                continue;
            }

            float leftVerticesXPos = vertexPositionsReadFrom[vertexIndex + 0].x;
            float rightVerticesXPos = vertexPositionsReadFrom[vertexIndex + 2].x;

            float leftOffsetY = Mathf.Sin(
                Time.timeSinceLevelLoad * wave_speed +
                leftVerticesXPos * SocraticAnnotation.waveFreqMultiplier) * SocraticAnnotation.waveAmplitude;

            float rightOffsetY = SocraticAnnotation.waveWarpTextVertices
                ? Mathf.Sin(Time.timeSinceLevelLoad * wave_speed +
                            rightVerticesXPos * SocraticAnnotation.waveFreqMultiplier) *
                  SocraticAnnotation.waveAmplitude
                : leftOffsetY;

            vertexPositionsWriteTo[vertexIndex + 0].y = vertexPositionsReadFrom[vertexIndex + 0].y + leftOffsetY;
            vertexPositionsWriteTo[vertexIndex + 1].y = vertexPositionsReadFrom[vertexIndex + 1].y + leftOffsetY;

            vertexPositionsWriteTo[vertexIndex + 2].y = vertexPositionsReadFrom[vertexIndex + 2].y + rightOffsetY;
            vertexPositionsWriteTo[vertexIndex + 3].y = vertexPositionsReadFrom[vertexIndex + 3].y + rightOffsetY;
        }
    }

    /// <summary>
    /// Gets the parses and analyses them to determine their type.
    /// </summary>
    /// <param name="inputText"></param>
    /// <param name="startAt"></param>
    /// <param name="preFilled"></param>
    /// <returns></returns>
    static internal List<SocraticAnnotationParse> GetParses(string inputText, int startAt,
        List<SocraticAnnotationParse> preFilled = null) {
        List<SocraticAnnotationParse> result = new List<SocraticAnnotationParse>();

        if (preFilled != null) {
            result = preFilled;
        }

        SocraticAnnotationParse parseProfile = null;

        for (int i = startAt; i < inputText.Length; i++) {
            if (inputText[i] == SocraticAnnotation.parseStartParse) {
                parseProfile = new SocraticAnnotationParse();
                parseProfile.startCharacterLocation = i;

                string compareProfileTo = "";
                string dynamicValue = "";
                bool readingDynamicValue = false;

                for (int f = i + 1; f < inputText.Length; f++) {
                    if (inputText[f] == SocraticAnnotation.parseEndParse) {
                        parseProfile.endCharacterLocation = f;

                        if (compareProfileTo.Contains(SocraticAnnotation.waveParseInfo)) {
                            parseProfile.richTextType = SocraticAnnotation.RichTextType.WAVE;
                        } else if (compareProfileTo.Contains(SocraticAnnotation.colorParseInfo)) {
                            parseProfile.richTextType = SocraticAnnotation.RichTextType.COLOR;
                        } else if (compareProfileTo.Contains(SocraticAnnotation.delayParseInfo)) {
                            parseProfile.richTextType = SocraticAnnotation.RichTextType.DELAY;
                        } else if (compareProfileTo.Contains(SocraticAnnotation.shakeParseInfo)) {
                            parseProfile.richTextType = SocraticAnnotation.RichTextType.SHAKE;
                        } else if (compareProfileTo.Contains(SocraticAnnotation.italicParseInfo)) {
                            parseProfile.richTextType = SocraticAnnotation.RichTextType.ITALIC;
                        } else {
                            Debug.LogError($"'{compareProfileTo}' -- Parse section did not have a valid input.");
                        }

                        string contents = "";

                        for (int c = i; c < f; c++) {
                            contents += inputText[c];
                        }

                        if (contents.Contains(SocraticAnnotation.parseEndParsePair)) {
                            parseProfile.openingParse = false;
                        }

                        if (readingDynamicValue) {
                            parseProfile.dynamicValue = dynamicValue;
                        }

                        result.Add(parseProfile);

                        return GetParses(inputText, f, result);
                    } else {
                        compareProfileTo += inputText[f];

                        if (readingDynamicValue) {
                            dynamicValue += inputText[f];
                        }
                    }

                    if (inputText[f] == SocraticAnnotation.parseInputValueIndicator) {
                        readingDynamicValue = true;
                    }
                }
            }
        }

        if (result == null) {
            Debug.LogError($"The result for the list of parses is null.");
        }

        return result;
    }

    static internal List<SocraticAnnotationParse> GetPunctuationDelayParses(string inputText, int startAt,
        List<SocraticAnnotationParse> preFilled = null) {
        List<SocraticAnnotationParse> result = new List<SocraticAnnotationParse>();

        if (preFilled != null) {
            result = preFilled;
        }

        SocraticAnnotationParse parseProfile = null;

        for (int i = startAt; i < inputText.Length; i++) {
            if (char.IsPunctuation(inputText[i]) && i < inputText.Length - 1) //TODO working
            {
                char c = inputText[i];

                bool skip = false;

                if (i > 1) {
                    if (inputText[i - 1] == 'r' && inputText[i - 2] == 'M') //TODO this fix is only temporary
                    {
                        skip = true;
                    }
                }

                if (inputText[i + 1] == ' ' && !skip) {
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
                        parseProfile.dynamicValue = (c == ',')
                            ? $"{SocraticAnnotation.displayMinorPunctuationDelay}"
                            : $"{SocraticAnnotation.displayMajorPunctuationDelay}";

                        result.Add(parseProfile);
                    }
                }
            }
        }

        if (result == null) {
            Debug.LogError($"The result for the list of parses is null.");
        }

        return result;
    }

    public static Vector2[] FromVector4Arr(Vector4[] input) {
        Vector2[] result = new Vector2[input.Length];

        for (int i = 0; i < input.Length; i++) {
            result[i] = new Vector2(input[i].x, input[i].y);
        }

        return result;
    }
}