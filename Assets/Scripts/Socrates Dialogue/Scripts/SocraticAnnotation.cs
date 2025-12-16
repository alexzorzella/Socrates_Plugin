using System.Collections.Generic;
using UnityEngine;

namespace SocratesDialogue {
    public class SocraticAnnotation : MonoBehaviour {
        static SocraticAnnotation _i;

        void Start() {
            DontDestroyOnLoad(gameObject);
        }
	
        public static SocraticAnnotation i {
            get {
                if (_i == null) {
                    SocraticAnnotation x = Resources.Load<SocraticAnnotation>("SocraticAnnotation");

                    _i = Instantiate(x);
                }
                return _i;
            }
        }
        
        public enum RichTextType {
            WAVE,
            DELAY,
            SHAKE,
            GRADIENT,
            SOUND
        }
        
        public static readonly Dictionary<string, RichTextType> annotationTags = new() {
            { "wave", RichTextType.WAVE },
            { "delay", RichTextType.DELAY },
            { "shake", RichTextType.SHAKE },
            { "gradient", RichTextType.GRADIENT },
            { "sound", RichTextType.SOUND }
        };

        public const char richTextStart = '<';
        public const char richTextEnd = '>';

        public static readonly char[] minorPunctuation = { ',', '–' };
        public static readonly char[] majorPunctuation = { '.', '?', '!', '~', ':', ':', '(', ')', ';', '—' };

        public const char parseStartChar = '[';
        public const char parseEndChar = ']';
        public const char parseClosePairChar = '!';
        public const char parseValueSeparator = ',';

        [Range(0, 0.1F)]
        public float displayDelayPerChar = 0.01F;

        [Range(0, 1F)]
        public float minorPunctuationDisplayDelay = 0.25F;
        [Range(0, 1F)]
        public float majorPunctuationDisplayDelay = 0.35F;

        public bool waveWarpTextVertices = true;
        [Range(0, 0.1F)]
        public float waveFreqMultiplier = 0.025F;
        [Range(0, 20F)]
        public float waveAmplitude = 7F;
        [Range(0, 20F)]
        public float waveSpeed = 9F;
        
        public const string defaultSoundName = "dialogue";
        
        [Range(-20F, 20F)]
        public float gradientSpeed = 0.1F;
        [Range(0, 4000)]
        public float gradientWidth = 2000F;
    }
}