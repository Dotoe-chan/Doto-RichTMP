using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Doto.RichTMP
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextMeshWobble : MonoBehaviour
    {
        public float speed = 1f;
        public float shakeRadius = 3f;

        private TextMeshProUGUI _textMeshPro;
        private CancellationToken _cancellationToken;
        private Vector3[][] _baseVertices;
        private float[] _phaseOffsets;
        private bool _isInitialized;
        private string _previousText = string.Empty;
        private int _previousCharacterCount;

        private void Start()
        {
            _textMeshPro = GetComponent<TextMeshProUGUI>();
            _cancellationToken = this.GetCancellationTokenOnDestroy();
            AnimateText().Forget();
        }

        private async UniTask AnimateText()
        {
            if (_textMeshPro == null)
                return;

            while (!_cancellationToken.IsCancellationRequested)
            {
                _textMeshPro.ForceMeshUpdate();
                TMP_TextInfo textInfo = _textMeshPro.textInfo;
                int characterCount = textInfo.characterCount;
                string currentText = _textMeshPro.text;

                bool textChanged =
                    currentText != _previousText || characterCount != _previousCharacterCount;

                if (!_isInitialized || textChanged)
                {
                    _baseVertices = new Vector3[textInfo.meshInfo.Length][];
                    for (int i = 0; i < textInfo.meshInfo.Length; i++)
                        _baseVertices[i] = (Vector3[])textInfo.meshInfo[i].vertices.Clone();

                    _phaseOffsets = Enumerable.Range(0, characterCount)
                        .Select(_ => Random.Range(0f, Mathf.PI * 2f))
                        .ToArray();

                    _isInitialized = true;
                    _previousText = currentText;
                    _previousCharacterCount = characterCount;
                }

                float time = Time.time * speed;

                for (int i = 0; i < characterCount; i++)
                {
                    if (i >= _phaseOffsets.Length || !textInfo.characterInfo[i].isVisible)
                        continue;

                    TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                    int materialIndex = charInfo.materialReferenceIndex;
                    int vertexIndex = charInfo.vertexIndex;
                    Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

                    if (materialIndex >= _baseVertices.Length ||
                        vertexIndex + 3 >= _baseVertices[materialIndex].Length ||
                        vertexIndex + 3 >= vertices.Length)
                    {
                        continue;
                    }

                    float phase = _phaseOffsets[i];
                    Vector3 offset = new(
                        Mathf.Sin(time + phase),
                        Mathf.Cos(time * 0.8f + phase),
                        0f
                    );
                    offset *= shakeRadius;

                    for (int j = 0; j < 4; j++)
                        vertices[vertexIndex + j] = _baseVertices[materialIndex][vertexIndex + j] + offset;
                }

                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
                    meshInfo.mesh.vertices = meshInfo.vertices;
                    _textMeshPro.UpdateGeometry(meshInfo.mesh, i);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, _cancellationToken);
            }
        }
    }
}
