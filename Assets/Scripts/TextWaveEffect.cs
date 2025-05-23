using System.Collections;
using TMPro;
using UnityEngine;

public class TextWaveEffect : MonoBehaviour
{
    public TMP_Text tmpText;
    public float waveAmplitude;
    public float waveFrequency;
    public float waveSpeed;
    private TMP_TextInfo textInfo;

    void Start()
    {
        tmpText.ForceMeshUpdate();
        textInfo = tmpText.textInfo;
    }

    void Update()
    {
        tmpText.ForceMeshUpdate();
        textInfo = tmpText.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible)
                continue;

            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            Vector3 offset = WaveOffset(i, Time.time);
            for (int j = 0; j < 4; j++)
            {
                vertices[vertexIndex + j] += offset;
            }
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            tmpText.UpdateGeometry(meshInfo.mesh, i);
        }
    }

    Vector3 WaveOffset(int index, float time)
    {
        float offsetY = Mathf.Sin((time * waveSpeed) + (index * waveFrequency)) * waveAmplitude;
        return new Vector3(0, offsetY, 0);
    }
}