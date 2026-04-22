using TMPro;
using UnityEngine;

namespace Doto.RichTMP
{
    [ExecuteAlways]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextMeshFirstCharacterScale : MonoBehaviour
    {
        [Min(0f)]
        public float scaleMultiplier = 1.5f;

        private TextMeshProUGUI _textMeshPro;
        private string _previousText = string.Empty;
        private int _previousCharacterCount = -1;

        private void Awake()
        {
            _textMeshPro = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            ApplyScale();
        }

        private void LateUpdate()
        {
            if (_textMeshPro == null)
                _textMeshPro = GetComponent<TextMeshProUGUI>();

            if (_textMeshPro == null)
                return;

            string currentText = _textMeshPro.text;
            int characterCount = _textMeshPro.textInfo.characterCount;
            if (currentText == _previousText && characterCount == _previousCharacterCount)
                return;

            ApplyScale();
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled)
                return;

            _textMeshPro = GetComponent<TextMeshProUGUI>();
            ApplyScale();
        }

        private void ApplyScale()
        {
            if (_textMeshPro == null)
                return;

            _textMeshPro.ForceMeshUpdate();
            TMP_TextInfo textInfo = _textMeshPro.textInfo;
            _previousText = _textMeshPro.text;
            _previousCharacterCount = textInfo.characterCount;

            if (textInfo.characterCount == 0)
                return;

            int visibleIndex = FindFirstVisibleCharacterIndex(textInfo);
            if (visibleIndex < 0)
                return;

            TMP_CharacterInfo charInfo = textInfo.characterInfo[visibleIndex];
            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            if (vertexIndex + 3 >= vertices.Length)
                return;

            // Pivot from the bottom-right area so extra scale grows upward and leftward.
            Vector3 pivot = (vertices[vertexIndex + 3] + vertices[vertexIndex + 2]) * 0.5f;

            for (int i = 0; i < 4; i++)
            {
                Vector3 offset = vertices[vertexIndex + i] - pivot;
                vertices[vertexIndex + i] = pivot + offset * scaleMultiplier;
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                _textMeshPro.UpdateGeometry(meshInfo.mesh, i);
            }
        }

        private static int FindFirstVisibleCharacterIndex(TMP_TextInfo textInfo)
        {
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (textInfo.characterInfo[i].isVisible)
                    return i;
            }

            return -1;
        }
    }
}
