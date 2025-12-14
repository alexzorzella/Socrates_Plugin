using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Codice.CM.Common.Tree.Partial;
using log4net.DateFormatter;

namespace SocratesDialogue {
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class SocVertModifier : MonoBehaviour {
        FancyText fancyText;

        TextMeshProUGUI textComponent;
        Vector3[] vertexPositions;

        float currentBetweenCharacterDelay;
        int totalVisibleCharacters;
        int counter = 0;

        DialogueSection currentSection;

        bool muted;
        static readonly Dictionary<string, MultiAudioSource> dialogueSfx = new();
        static MultiAudioSource currentDialogueSfx = null;

        float startedDisplayingLast = 0;
        const float scrollTime = 0.1F;
        const float minOffsetY = -12;
        
        void Awake() {
            GetComponents();
        }

        /// <summary>
        /// Caches the text component attached to the object.
        /// </summary>
        void GetComponents() {
            textComponent = GetComponent<TextMeshProUGUI>();
        }

        /// <summary>
        /// Sets the sound effect that plays when a new character is revealed.
        /// </summary>
        /// <param name="soundName"></param>
        public void SetDialogueSfx(string soundName) {
            if (!dialogueSfx.ContainsKey(soundName)) {
                dialogueSfx.Add(soundName, MultiAudioSource.FromResource(gameObject, soundName));
            }

            currentDialogueSfx = dialogueSfx[soundName];
        }
        
        /// <summary>
        /// Returns whether all the text has been displayed. Only returns true if there's visible text in the text element.
        /// </summary>
        /// <returns></returns>
        public bool TextHasBeenDisplayed() {
            return counter >= totalVisibleCharacters
                   && totalVisibleCharacters > 0;
        }

        /// <summary>
        /// Converts the passed raw text to fancy text and sets the text component's text fully,
        /// muted by default. Flagging it to scroll won't fully display it immediately and not
        /// muting it will make it play the current dialogue sound while the text scrolls.
        /// </summary>
        /// <param name="rawText"></param>
        /// <param name="scroll"></param>
        /// <param name="muted"></param>
        public void SetText(string rawText, bool scroll = false, bool muted = true) {
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

            for (var i = 0; i < totalVisibleCharacters; i++) {
                SetCharColor(
                    textComponent, i, 
                    OverrideAlpha(GetColorOfTopLeft(textComponent, i), true));
            }

            textComponent.color = new Color(color.r, color.g, color.b, 0);

            if (!scroll) {
                textComponent.color = OverrideAlpha(textComponent.color, false);
                
                if (currentDialogueSfx != null) {
                    currentDialogueSfx.Stop();
                }
                
                counter = textComponent.maxVisibleCharacters;
            }

            startedDisplayingLast = Time.timeSinceLevelLoad;
            
            fancyText.ClearDisplayTimes();
        }
        
        /// <summary>
        /// Clears the current text.
        /// </summary>
        public void ClearText() {
            SetText("");
        }

        void Update() {
            if (textComponent.text.Length <= 0) {
                return;
            }

            IncrementCharCounter();
            UpdateTextEmbellishes();
        }

        /// <summary>
        /// Increments the counter, resets the clock for the time left until the next character
        /// is displayed, executes parse actions that it's at or has passed that haven't been
        /// executed yet, and handles the state of the dialogue audio.
        /// </summary>
        void IncrementCharCounter() {
            // Return if the counter has exceeded the maximum number of characters after
            // muting the current sound effect, if one exists 
            if (counter >= totalVisibleCharacters) {
                if (!muted) {
                    if (currentDialogueSfx != null) {
                        currentDialogueSfx.Stop();
                    }
                }

                return;
            }

            // If the current delay between characters is less than or equal to zero,
            // the number of visible characters is increased, the sound effect is played
            // if it's not already
            if (currentBetweenCharacterDelay <= 0) {
                counter++;
                
                fancyText.LogDisplayTime(Time.timeSinceLevelLoad - startedDisplayingLast);

                // float actualTime = Time.timeSinceLevelLoad;
                // float expectedTime = fancyText.GetCharDisplayTime(counter);
                // float percentError = ((expectedTime - actualTime) / actualTime) * 100;
                //
                // Debug.Log(percentError);
                
                if (!muted) {
                    if (currentDialogueSfx != null) {
                        currentDialogueSfx.PlayOnlyIfDone();
                    }
                }

                // Reset the current delay
                currentBetweenCharacterDelay = SocraticAnnotation.displayDelayPerChar;

                // Executed unexecuted delays
                foreach (var parse in fancyText.GetAnnotationTokens()) {
                    bool isDelay = parse.GetRichTextType() == SocraticAnnotation.RichTextType.DELAY;
                    
                    if (isDelay) {
                        bool isUnexecutedAction = 
                            parse.IsOpener() && 
                            parse.GetStartCharIndex() <= counter &&
                            !parse.HasExecutedAction();
                        
                        if (isUnexecutedAction) {
                            if (currentDialogueSfx != null) {
                                currentDialogueSfx.Stop();
                            }

                            currentBetweenCharacterDelay = parse.GetDynamicValueAsFloat();
                            parse.ExecuteAction();

                            OnCharDelay();
                        } else if (parse.IsOpener() && parse.GetStartCharIndex() == counter - 1 &&
                                  parse.HasExecutedAction()) {
                            OnPostCharDelay();
                        }
                    }
                }
            }

            currentBetweenCharacterDelay -= Time.deltaTime;
        }

        /// <summary>
        /// Executes whenever a character delay beings.
        /// </summary>
        void OnCharDelay() {
            
        }

        /// <summary>
        /// Executes whenever a character delay ends.
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
        void SetCharColor(TextMeshProUGUI textComponent, int charIndex, Color32 color) {
            int meshIndex = textComponent.textInfo.characterInfo[charIndex].materialReferenceIndex;
            int vertexIndex = textComponent.textInfo.characterInfo[charIndex].vertexIndex;

            if (!textComponent.textInfo.characterInfo[charIndex].isVisible) {
                return;
            }

            Color32[] vertexColors = textComponent.textInfo.meshInfo[meshIndex].colors32;

            for (int v = 0; v < 4; v++) {
                int absVertexIndex = vertexIndex + v;
                vertexColors[absVertexIndex] = color;
            }

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
        /// Returns the color given with the alpha changed to 0 or 1. 
        /// </summary>
        /// <param name="color"></param>
        /// <param name="hidden"></param>
        /// <returns></returns>
        Color OverrideAlpha(Color color, bool hidden) {
            return new Color(color.r, color.g, color.b, hidden ? 0F : 1F);
        }

        /// <summary>
        /// Manages the visible characters and calls the rich text to be updated.
        /// </summary>
        void UpdateTextEmbellishes() {
            TMP_TextInfo textInfo = textComponent.textInfo;

            Vector3[] newVertexPositions = GetMaterialAtZero(textInfo).vertices;

            ApplyRichText(textInfo, newVertexPositions);

            int start = counter - 15; // Why 15?

            if (start < 0) start = 0;

            if (counter <= totalVisibleCharacters) {
                for (int i = start; i < counter; i++) {
                    SetCharColor(textComponent, i, 
                        OverrideAlpha(GetColorOfTopLeft(textComponent, i), false));
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
        /// Applies the rich text based on what the parse means.
        /// </summary>
        /// <param name="textInfo"></param>
        /// <param name="newVertexPositions"></param>
        void ApplyRichText(TMP_TextInfo textInfo, Vector3[] newVertexPositions) {
            ScrollInFromY(textInfo, vertexPositions, newVertexPositions);
            
            if (fancyText.GetAnnotationTokens() != null) {
                foreach (var parse in fancyText.GetAnnotationTokens()) {
                    if (parse.IsOpener()) {
                        switch (parse.GetRichTextType()) {
                            case SocraticAnnotation.RichTextType.WAVE:
                                ApplyRichTextWave(textInfo, parse, vertexPositions, newVertexPositions);
                                break;
                            case SocraticAnnotation.RichTextType.SHAKE:
                                ApplyRichTextShake(textInfo, parse, vertexPositions, newVertexPositions);
                                break;
                        }
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
        /// Makes the characters come from below. Still under construction.
        ///
        /// Known bugs:
        /// 1. The reveal time for the characters is beating the estimated time calculated by
        ///    fancyText.GetCharDisplayTime(i), so as the text scrolls, the effect diminishes.
        /// 2. Some characters jitter when animating and end up in the wrong location.
        /// </summary>
        /// <param name="textInfo"></param>
        /// <param name="vertexPositionsReadFrom"></param>
        /// <param name="vertexPositionsWriteTo"></param>
        void ScrollInFromY(
            TMP_TextInfo textInfo, 
            Vector3[] vertexPositionsReadFrom,
            Vector3[] vertexPositionsWriteTo) {
            if (counter >= textComponent.maxVisibleCharacters) {
                return;
            }
            
            for (int i = 0; i < textInfo.characterInfo.Length; i++) {
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                
                if (!textInfo.characterInfo[i].isVisible) {
                    //Debug.Log($"Vertex index is zero? {parse.startCharacterLocation}");
                    continue;
                }

                // Calculate the amount of time that passed since the dialogue started
                float timeSinceDialogueStarted = Time.timeSinceLevelLoad - startedDisplayingLast;
                
                // Cache the time that a character would first be displayed.
                // This is the amount of time it would take for the character to
                // appear after the dialogue first started.
                float charDisplayTimestamp = fancyText.GetCharDisplayTime(i);
                
                // Break if the time since the dialogue started has not reached that time yet.
                // This is safe because no other character after this one can be revealed
                // before this one is.
                if (timeSinceDialogueStarted < charDisplayTimestamp) {
                    break;
                }
                
                // Calculate how long the character has been displayed so far
                float timeCharHasBeenDisplayed = timeSinceDialogueStarted - charDisplayTimestamp;
                
                // Calculate the percentage of the way the character is supposed to be
                float percentageOfPathMoved = timeCharHasBeenDisplayed / scrollTime;
                
                // If the percentage is greater than one, there's no need to update its position
                if (percentageOfPathMoved > 1) {
                    continue;
                }
                
                // Clamp the percentage
                percentageOfPathMoved = Mathf.Clamp(percentageOfPathMoved, 0, 1);
                
                // Calculate the offset relative to the character's origin that it needs to be
                // using an easing function
                float offsetY = LeanTween.easeOutQuad(minOffsetY, 0, percentageOfPathMoved);
                // float offsetY = minOffsetY * (1 - percentageOfPathMoved);

                // Update the positions of all four vertices
                for (int v = 0; v < 4; v++) {
                    int absVertexIndex = vertexIndex + v;
                    vertexPositionsWriteTo[absVertexIndex].y = vertexPositionsReadFrom[absVertexIndex].y + offsetY;
                }
            }
        }

        /// <summary>
        /// Courtesy of TextMeshPro: I don't know much about this, but I know it works.
        /// </summary>
        /// <param name="vertexPositionsReadFrom"></param>
        /// <param name="textInfo"></param>
        /// <param name="token"></param>
        /// <param name="vertexPositionsWriteTo"></param>
        void ApplyRichTextShake(
            TMP_TextInfo textInfo,
            AnnotationToken token, 
            Vector3[] vertexPositionsReadFrom, 
            Vector3[] vertexPositionsWriteTo) {

            VertexAnim[] vertexAnim = new VertexAnim[1024];

            for (int i = 0; i < 1024; i++) {
                vertexAnim[i].angleRange = UnityEngine.Random.Range(10f, 25f);
                vertexAnim[i].speed = UnityEngine.Random.Range(1f, 3f);
            }

            for (int i = token.GetStartCharIndex(); i < token.GetLinkedToken().GetStartCharIndex(); i++) {
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

                Vector2 charMidBaseline = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

                Vector3 offset = charMidBaseline;

                Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                vertexPositionsWriteTo[vertexIndex + 0] = sourceVertices[vertexIndex + 0] - offset;
                vertexPositionsWriteTo[vertexIndex + 1] = sourceVertices[vertexIndex + 1] - offset;
                vertexPositionsWriteTo[vertexIndex + 2] = sourceVertices[vertexIndex + 2] - offset;
                vertexPositionsWriteTo[vertexIndex + 3] = sourceVertices[vertexIndex + 3] - offset;

                float angleMultiplier = 1.0F; // How much it rotates
                float curveScale = token.GetDynamicValueAsFloat(); // Noticeability
                float loopCount = 1.0F; // Don't know

                vertAnim.angle = Mathf.SmoothStep(-vertAnim.angleRange, vertAnim.angleRange,
                    Mathf.PingPong(loopCount / 25f * vertAnim.speed, 1f));
                Vector3 jitterOffset = new Vector3(Random.Range(-.25f, .25f),
                    Random.Range(-.25f, .25f), 0);
                Matrix4x4 matrix = Matrix4x4.TRS(jitterOffset * curveScale,
                    Quaternion.Euler(0, 0, Random.Range(-5f, 5f) * angleMultiplier), Vector3.one);

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
        void ApplyRichTextWave(
            TMP_TextInfo textInfo, 
            AnnotationToken token,
            Vector3[] vertexPositionsReadFrom,
            Vector3[] vertexPositionsWriteTo) {
            float waveSpeed = token.ContainsDynamicValue()
                ? token.GetDynamicValueAsFloat()
                : SocraticAnnotation.waveSpeed;

            for (int i = token.GetStartCharIndex(); i < token.GetLinkedToken().GetStartCharIndex(); i++) {
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                if (vertexIndex == 0 && i != 0) {
                    //Debug.Log($"Vertex index is zero? {parse.startCharacterLocation}");
                    continue;
                }

                float leftVerticesXPos = vertexPositionsReadFrom[vertexIndex + 0].x;
                float rightVerticesXPos = vertexPositionsReadFrom[vertexIndex + 2].x;

                float leftOffsetY = Mathf.Sin(
                    Time.timeSinceLevelLoad * waveSpeed +
                    leftVerticesXPos * SocraticAnnotation.waveFreqMultiplier) * SocraticAnnotation.waveAmplitude;

                float rightOffsetY = SocraticAnnotation.waveWarpTextVertices
                    ? Mathf.Sin(Time.timeSinceLevelLoad * waveSpeed +
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
}