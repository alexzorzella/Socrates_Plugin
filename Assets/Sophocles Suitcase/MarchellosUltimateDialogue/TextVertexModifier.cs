using UnityEngine;
using TMPro;
using System.Linq;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextVertexModifier : MonoBehaviour
{
    private MarchellosAnnotation annotation;
    private TextMeshProUGUI localText;

    void Awake()
    {
        annotation = FindObjectOfType<MarchellosAnnotation>();
        localText = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        localText.ForceMeshUpdate();
    }

    private void Update()
    {
        AnimateWavePositions(annotation, localText);
    }

    private static TMP_MeshInfo GetMaterialAtZero(TMP_TextInfo textInfo)
    {
        return textInfo.meshInfo[0];
    }

    public static void AnimateWavePositions(MarchellosAnnotation annotation, TextMeshProUGUI textComponent)
    {

        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;

        string parsedText = textComponent.GetParsedText();

        List<int> indexes = FindWaveIndexes(parsedText, annotation);

        List<Tuple<int, int>> pairs = PairUpAnnotations(indexes);

        Vector3[] newVertexPositions = GetMaterialAtZero(textInfo).vertices;

        foreach (var pair in pairs)
        {
            for (int i = pair.Item1; i < pair.Item2 + 1; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                float leftVerticesXPos = newVertexPositions[vertexIndex + 0].x;
                float rightVerticesXPos = newVertexPositions[vertexIndex + 2].x;

                float leftOffsetY = Mathf.Sin(
                    Time.timeSinceLevelLoad * annotation.wave_speed +
                    leftVerticesXPos * annotation.wave_freqMultiplier) * annotation.wave_amplitude;

                float rightOffsetY = annotation.wave_warpTextVerticies
                    ? Mathf.Sin(Time.timeSinceLevelLoad * annotation.wave_speed +
                      rightVerticesXPos * annotation.wave_freqMultiplier) * annotation.wave_amplitude
                    : leftOffsetY;

                newVertexPositions[vertexIndex + 0].y += leftOffsetY;
                newVertexPositions[vertexIndex + 1].y += leftOffsetY;

                newVertexPositions[vertexIndex + 2].y += rightOffsetY;
                newVertexPositions[vertexIndex + 3].y += rightOffsetY;
            }
        }

        TMP_MeshInfo meshInfo = GetMaterialAtZero(textComponent.textInfo);

        textComponent.mesh.SetVertices(newVertexPositions);
        textComponent.mesh.uv = meshInfo.uvs0;
        textComponent.mesh.uv2 = meshInfo.uvs2;

        //SetAll(textComponent, Color.blue, annotation);

        for (int i = 0; i < 1; i++)
        {
            textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            textComponent.UpdateFontAsset();
            //textComponent.Update
        }
    }

    //Finds the indexes based on the given character in the MarchellosAnnotation script

    private static List<int> FindWaveIndexes(string inputText, MarchellosAnnotation annotation)
    {
        List<int> result = new List<int>();

        for (int i = 0; i < inputText.Length; i++)
        {
            if (inputText[i] == annotation.wave_textWaveAnnotationCharacter)
                result.Add(i);
        }

        return result;
    }

    //Pairs up all the annotations, leaves out an odd one
    private static List<Tuple<int, int>> PairUpAnnotations(List<int> indexes)
    {
        List<Tuple<int, int>> result = new List<Tuple<int, int>>();
        for (int i = 0; i < indexes.Count(); i += 2)
        {
            result.Add(new Tuple<int, int>(indexes[i], indexes[i + 1]));
        }
        return result;
    }
}