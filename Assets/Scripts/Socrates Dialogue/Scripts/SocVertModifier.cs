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

    void Awake() {
        GetComponents();
    }

    /// <summary>
    /// Gets the text component attached to the object.
    /// </summary>
    private void GetComponents() {
        textComponent = GetComponent<TextMeshProUGUI>();
    }
    
    /// <summary>
    /// Returns whether all the text has been displayed. Only returns true if there's visible text in the text element.
    /// </summary>
    /// <returns></returns>
    public bool TextHasBeenDisplayed() {
        return counter >= totalVisibleCharacters
               && totalVisibleCharacters > 0;
    }

    public void ClearText() {
        SetText("", true);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="rawText"></param>
    /// <param name="vertexModifier"></param>
    /// <param name="displayFully"></param>
    /// <param name="muted"></param>
    public void SetText(
        string rawText,
        bool displayFully = false,
        bool muted = true) {
        this.muted = muted;
        counter = 0;
        currentBetweenCharacterDelay = 0;

        fancyText = new FancyText(rawText);

        var color = textComponent.color;

        textComponent.text = fancyText.ToString();

        textComponent.ForceMeshUpdate();

        var vertices = GetMaterialAtZero(textComponent.textInfo).vertices;
        vertexPositions = new Vector3[vertices.Length];

        for (var v = 0; v < vertices.Length; v++) vertexPositions[v] = vertices[v];

        totalVisibleCharacters = textComponent.textInfo.characterCount;

        for (var i = 0; i < totalVisibleCharacters; i++)
            SetColor(textComponent, i, ToggleAlpha(GetColorOfTopLeft(textComponent, i), true), i);

        textComponent.color = new Color(color.r, color.g, color.b, 0);

        if (displayFully) {
            var temp = Color.white;

            ColorUtility.TryParseHtmlString("#DDD0CC", out temp);

            textComponent.color = temp;
            // AudioManager.i.StopAllSourcesContains("dialogue", true);
            counter = textComponent.maxVisibleCharacters;
        }
    }

    void Update() {
        if (textComponent.text.Length <= 0) {
            return;
        }

        IncrementCharCounter();
        UpdateTextEmbellishes();
    }

    /// <summary>
    /// Provides debug data breakdown for testing.
    /// </summary>
    /// <param name="sendInfoTo"></param>
    /// <param name="grabInfoFrom"></param>
    /// <param name="vertexMod"></param>
    static void UpdateDebugText(TextMeshProUGUI sendInfoTo, TextMeshProUGUI grabInfoFrom,
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
    void IncrementCharCounter() {
        if (counter >= totalVisibleCharacters) {
            if (currentSection != null && !muted) {
                if (currentDialogueSfx != null) {
                    currentDialogueSfx.Stop();
                }
            }
            else if (!muted) {
                if (currentDialogueSfx != null) {
                    currentDialogueSfx.Stop();
                }
            }

            return;
        }

        if (currentBetweenCharacterDelay <= 0) {
            counter++;

            if (!muted) {
                if (currentSection != null) {
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

            currentBetweenCharacterDelay = SocraticAnnotation.displayTextDelay;

            foreach (var parse in fancyText.GetAnnotationTokens()) {
                if (parse.richTextType == SocraticAnnotation.RichTextType.DELAY) {
                    if (parse.opener && parse.startCharIndex == counter &&
                        !parse.executedAction) {
                        if (currentSection != null) {
                            if (currentDialogueSfx != null) {
                                currentDialogueSfx.Stop();
                            }
                        }
                        else {
                            if (currentDialogueSfx != null) {
                                currentDialogueSfx.Stop();
                            }
                        }

                        currentBetweenCharacterDelay = parse.GetDynamicValueAsFloat();
                        parse.executedAction = true;

                        OnCharDelay();
                    }
                    else if (parse.opener && parse.startCharIndex == counter - 1 &&
                             parse.executedAction) {
                        OnPostCharDelay();
                    }
                }
            }
        }

        currentBetweenCharacterDelay -= Time.deltaTime;
    }

    /// <summary>
    /// Occurs when the character delay beings.
    /// </summary>
    void OnCharDelay() {
        
    }

    /// <summary>
    /// Occurs when the character delay finishes.
    /// </summary>
    void OnPostCharDelay() {
        
    }

    /// <summary>
    /// Gets the material at index zero for a given textInfo.
    /// </summary>
    /// <param name="textInfo"></param>
    /// <returns></returns>
    static TMP_MeshInfo GetMaterialAtZero(TMP_TextInfo textInfo) {
        return textInfo.meshInfo[0];
    }

    /// <summary>
    /// Sets all four vertices of a given character to a color.
    /// </summary>
    /// <param name="textComponent"></param>
    /// <param name="charIndex"></param>
    /// <param name="color"></param>
    /// <param name="i"></param>
    void SetColor(TextMeshProUGUI textComponent, int charIndex, Color32 color, int i) {
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
    Color GetColorOfTopLeft(TextMeshProUGUI textComponent, int charIndex) {
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
    Color ToggleAlpha(Color color, bool hidden) {
        return new Color(color.r, color.g, color.b, hidden ? 0F : 1F);
    }

    /// <summary>
    /// Manages the visible characters and calls the rich text to be updated.
    /// </summary>
    /// <param name="textComponent"></param>
    /// <param name="vertexMod"></param>
    void UpdateTextEmbellishes() {
        TMP_TextInfo textInfo = textComponent.textInfo;

        Vector3[] newVertexPositions = GetMaterialAtZero(textInfo).vertices;

        ScrollInFromY(textInfo, newVertexPositions);

        ApplyRichText(textComponent, textInfo, newVertexPositions);

        int start = counter - 15; // Why 15?

        if (start < 0) start = 0;

        if (counter <= totalVisibleCharacters) {
            for (int i = start; i < counter; i++) {
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
    static void ScrollInFromY(TMP_TextInfo textInfo, Vector3[] newVertexPositions) {
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
    void ApplyRichText(TextMeshProUGUI textComponent, TMP_TextInfo textInfo, Vector3[] newVertexPositions) {
        if (fancyText.GetAnnotationTokens() == null) {
            return;
        }

        foreach (var parse in fancyText.GetAnnotationTokens()) {
            if (parse.opener) {
                switch (parse.richTextType) {
                    case SocraticAnnotation.RichTextType.WAVE:
                        ApplyRichTextWave(textInfo, vertexPositions, parse, newVertexPositions);
                        break;
                    case SocraticAnnotation.RichTextType.SHAKE:
                        ApplyRichTextShake(vertexPositions, parse, newVertexPositions);
                        break;
                }
            }
        }
    }

    struct VertexAnim {
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
    void ApplyRichTextShake(Vector3[] vertexPositionsReadFrom,
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
    void ApplyRichTextWave(TMP_TextInfo textInfo, Vector3[] vertexPositionsReadFrom,
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

    static Vector2[] FromVector4Arr(Vector4[] input) {
        Vector2[] result = new Vector2[input.Length];

        for (int i = 0; i < input.Length; i++) {
            result[i] = new Vector2(input[i].x, input[i].y);
        }

        return result;
    }
}