using UnityEngine;
using TMPro;
using System.Collections.Generic;
using NewSocratesDialogue;

[RequireComponent(typeof(TextMeshProUGUI))]
public class SocVertModifier : MonoBehaviour {
    FancyText fancyText;

    TextMeshProUGUI textComponent;
    Vector3[] vertexPositions;

    float currentBetweenCharacterDelay;
    int totalVisibleCharacters;
    int counter = 0;

    NewDialogueSection currentSection;

    bool muted;
    static Dictionary<string, MultiAudioSource> dialogueSfx = new();
    static MultiAudioSource currentDialogueSfx = null;

    public void SetDialogueSfx(string soundName) {
        if (!dialogueSfx.ContainsKey(soundName)) {
            dialogueSfx.Add(soundName, MultiAudioSource.FromResource(gameObject, soundName));
        }

        currentDialogueSfx = dialogueSfx[soundName];
    }

    TextMeshProUGUI TextComponent() {
        return textComponent;
    }

    /// <summary>
    /// Returns whether all the text has been displayed. Only returns true if there's visible text in the text element.
    /// </summary>
    /// <returns></returns>
    public bool TextHasBeenDisplayed() {
        return counter >= totalVisibleCharacters
               && totalVisibleCharacters > 0;
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
        string rawText,
        SocVertModifier vertexModifier,
        bool displayFully = false,
        bool muted = true) {
        var textComponent = vertexModifier.GetComponent<TextMeshProUGUI>();

        vertexModifier.muted = muted;
        vertexModifier.counter = 0;
        vertexModifier.currentBetweenCharacterDelay = 0;

        vertexModifier.fancyText = new FancyText(rawText);

        var color = textComponent.color;

        textComponent.text = vertexModifier.fancyText.ToString();

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
    /// Disects the text.
    /// </summary>
    /// <param name="contents"></param>
    /// <param name="vertexModifier"></param>
    /// <returns></returns>
    private static string SnipParses(string contents, SocVertModifier vertexModifier) {
        int totalCharactersSnipped = 0;

        foreach (var parsedSegment in vertexModifier.fancyText.GetAnnotationTokens()) {
            //TODO work in progress
            if (parsedSegment.startCharIndex != parsedSegment.endCharIndex) {
                parsedSegment.startCharIndex -= totalCharactersSnipped;
                parsedSegment.endCharIndex -= totalCharactersSnipped;
                int start = parsedSegment.startCharIndex;
                int length = (parsedSegment.endCharIndex - parsedSegment.startCharIndex) + 1;
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

        foreach (var parse in vertexMod.fancyText.GetAnnotationTokens()) {
            debugParseInfo +=
                $"{parse.startCharIndex}-{parse.endCharIndex} Opening: ({parse.opener})\n" +
                $"{parse.richTextType} Value: {parse.passedValue}\n";
        }

        sendInfoTo.text = $"" +
                          $"{(int)(1.0f / Time.deltaTime)} FPS\n" +
                          $"String Length: {grabInfoFrom.text.Length}\n" +
                          $"Display Counter: {vertexMod.counter}\n" +
                          $"Vertices: {grabInfoFrom.mesh.vertices.Length}\n" +
                          $"Character Count: {grabInfoFrom.textInfo.characterCount}\n" +
                          $"Line Count: {grabInfoFrom.textInfo.lineCount}\n" +
                          $"Word Count: {grabInfoFrom.textInfo.wordCount}\n" +
                          $"Parse Count: {vertexMod.fancyText.GetAnnotationTokens().Count}\n" +
                          $"Total Visible Characters: {vertexMod.totalVisibleCharacters}\n" + debugParseInfo;
    }

    /// <summary>
    /// Increments the counter and manages the volume of the sound being played.
    /// </summary>
    /// <param name="vertexModifier"></param>
    private static void IncrementCharCounter(SocVertModifier vertexModifier) {
        if (vertexModifier.counter >= vertexModifier.totalVisibleCharacters) {
            if (vertexModifier.currentSection != null && !vertexModifier.muted) {
                if (currentDialogueSfx != null) {
                    currentDialogueSfx.Stop();
                }
            }
            else if (!vertexModifier.muted) {
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
                }
                else {
                    if (currentDialogueSfx != null) {
                        currentDialogueSfx.PlayIfDone();
                    }
                }
            }

            vertexModifier.currentBetweenCharacterDelay = SocraticAnnotation.displayTextDelay;

            foreach (var parse in vertexModifier.fancyText.GetAnnotationTokens()) {
                //if (parse.openingParse && 
                //    parse.startCharacterLocation == vertexModifier.counter && 
                //    parse.richTextType == SocraticAnnotation.RichTextType.WAVE)
                //{

                //}

                if (parse.richTextType == SocraticAnnotation.RichTextType.DELAY) {
                    if (parse.opener && parse.startCharIndex == vertexModifier.counter &&
                        !parse.executedAction) {
                        if (vertexModifier.currentSection != null) {
                            if (currentDialogueSfx != null) {
                                currentDialogueSfx.Stop();
                            }
                        }
                        else {
                            if (currentDialogueSfx != null) {
                                currentDialogueSfx.Stop();
                            }
                        }

                        vertexModifier.currentBetweenCharacterDelay = parse.GetDynamicValueAsFloat();
                        parse.executedAction = true;

                        OnCharDelay();
                    }
                    else if (parse.opener && parse.startCharIndex == vertexModifier.counter - 1 &&
                             parse.executedAction) {
                        OnPostCharDelay();
                    }
                }
            }
        }

        vertexModifier.currentBetweenCharacterDelay -= Time.deltaTime;
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
        if (vertexMod.fancyText.GetAnnotationTokens() == null) {
            return;
        }

        foreach (var parse in vertexMod.fancyText.GetAnnotationTokens()) {
            if (parse.opener) {
                switch (parse.richTextType) {
                    case SocraticAnnotation.RichTextType.WAVE:
                        ApplyRichTextWave(textInfo, vertexMod.vertexPositions, parse, newVertexPositions);
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
    /// Courtesy of TextMeshPro: I don't know much about this, but I know it works.
    /// </summary>
    /// <param name="textComponent"></param>
    /// <param name="vertexPositionsReadFrom"></param>
    /// <param name="token"></param>
    /// <param name="vertexPositionsWriteTo"></param>
    private static void ApplyRichTextShake(TextMeshProUGUI textComponent, Vector3[] vertexPositionsReadFrom,
        AnnotationToken token, Vector3[] vertexPositionsWriteTo) {
        TMP_TextInfo textInfo = textComponent.textInfo;

        VertexAnim[] vertexAnim = new VertexAnim[1024];

        for (int i = 0; i < 1024; i++) {
            vertexAnim[i].angleRange = UnityEngine.Random.Range(10f, 25f);
            vertexAnim[i].speed = UnityEngine.Random.Range(1f, 3f);
        }

        for (int i = token.startCharIndex; i < token.linkedToken.startCharIndex; i++) {
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
            float CurveScale = token.GetDynamicValueAsFloat(); //noticability
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
    /// Applies the rich text setting the wave positions.
    /// </summary>
    /// <param name="textInfo"></param>
    /// <param name="vertexPositionsReadFrom"></param>
    /// <param name="token"></param>
    /// <param name="vertexPositionsWriteTo"></param>
    private static void ApplyRichTextWave(TMP_TextInfo textInfo, Vector3[] vertexPositionsReadFrom,
        AnnotationToken token, Vector3[] vertexPositionsWriteTo) {
        float wave_speed = token.ContainsDynamicValue()
            ? token.GetDynamicValueAsFloat()
            : SocraticAnnotation.waveSpeed;

        for (int i = token.startCharIndex; i < token.linkedToken.startCharIndex; i++) {
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

    public static Vector2[] FromVector4Arr(Vector4[] input) {
        Vector2[] result = new Vector2[input.Length];

        for (int i = 0; i < input.Length; i++) {
            result[i] = new Vector2(input[i].x, input[i].y);
        }

        return result;
    }
}