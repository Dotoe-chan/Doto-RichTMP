using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Doto.RichTMP
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextMeshPikaBlink : MonoBehaviour
    {
        [TextArea]
        public string sourceText;

        [Min(0f)]
        public float blinkSpeed = 3f;

        private const string StartTag = "<Pika>";
        private const string EndTag = "</Pika>";

        private readonly List<PikaRange> _ranges = new();
        private readonly List<CharacterColorCache> _targets = new();

        private TextMeshProUGUI _textMeshPro;
        private string _lastSourceText = string.Empty;
        private string _parsedText = string.Empty;

        private void Awake()
        {
            _textMeshPro = GetComponent<TextMeshProUGUI>();
            if (string.IsNullOrEmpty(sourceText))
                sourceText = _textMeshPro.text;
        }

        private void OnEnable()
        {
            RefreshIfNeeded(force: true);
        }

        private void LateUpdate()
        {
            RefreshIfNeeded(force: false);
            ApplyBlink();
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled)
                return;

            _textMeshPro = GetComponent<TextMeshProUGUI>();
            RefreshIfNeeded(force: true);
            ApplyBlink();
        }

        public void SetText(string value)
        {
            sourceText = value;
            RefreshIfNeeded(force: true);
        }

        private void RefreshIfNeeded(bool force)
        {
            if (_textMeshPro == null)
                return;

            if (!force && sourceText == _lastSourceText)
                return;

            ParseSourceText(sourceText, out string visibleText);
            _parsedText = visibleText;
            _lastSourceText = sourceText;

            _textMeshPro.text = _parsedText;
            _textMeshPro.ForceMeshUpdate();
            CacheOriginalColors();
        }

        private void ParseSourceText(string rawText, out string visibleText)
        {
            _ranges.Clear();

            if (string.IsNullOrEmpty(rawText))
            {
                visibleText = string.Empty;
                return;
            }

            System.Text.StringBuilder builder = new();
            int visibleIndex = 0;
            int pikaDepth = 0;
            int currentRangeStart = -1;

            for (int i = 0; i < rawText.Length;)
            {
                if (MatchesAt(rawText, i, StartTag))
                {
                    if (pikaDepth == 0)
                        currentRangeStart = visibleIndex;

                    pikaDepth++;
                    i += StartTag.Length;
                    continue;
                }

                if (MatchesAt(rawText, i, EndTag))
                {
                    if (pikaDepth > 0)
                    {
                        pikaDepth--;
                        if (pikaDepth == 0 && currentRangeStart >= 0 && visibleIndex > currentRangeStart)
                        {
                            _ranges.Add(new PikaRange(currentRangeStart, visibleIndex));
                            currentRangeStart = -1;
                        }
                    }

                    i += EndTag.Length;
                    continue;
                }

                if (rawText[i] == '<')
                {
                    int tagEnd = rawText.IndexOf('>', i);
                    if (tagEnd >= 0)
                    {
                        builder.Append(rawText, i, tagEnd - i + 1);
                        i = tagEnd + 1;
                        continue;
                    }
                }

                builder.Append(rawText[i]);
                visibleIndex++;
                i++;
            }

            if (pikaDepth > 0 && currentRangeStart >= 0 && visibleIndex > currentRangeStart)
                _ranges.Add(new PikaRange(currentRangeStart, visibleIndex));

            visibleText = builder.ToString();
        }

        private void CacheOriginalColors()
        {
            _targets.Clear();

            TMP_TextInfo textInfo = _textMeshPro.textInfo;
            foreach (PikaRange range in _ranges)
            {
                for (int i = range.Start; i < range.End && i < textInfo.characterCount; i++)
                {
                    TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                    if (!charInfo.isVisible)
                        continue;

                    int materialIndex = charInfo.materialReferenceIndex;
                    int vertexIndex = charInfo.vertexIndex;
                    Color32[] colors = textInfo.meshInfo[materialIndex].colors32;
                    if (vertexIndex + 3 >= colors.Length)
                        continue;

                    _targets.Add(new CharacterColorCache(
                        materialIndex,
                        vertexIndex,
                        colors[vertexIndex],
                        colors[vertexIndex + 1],
                        colors[vertexIndex + 2],
                        colors[vertexIndex + 3]
                    ));
                }
            }
        }

        private void ApplyBlink()
        {
            if (_textMeshPro == null || _targets.Count == 0)
                return;

            TMP_TextInfo textInfo = _textMeshPro.textInfo;
            float t = (Mathf.Sin(Time.unscaledTime * blinkSpeed) + 1f) * 0.5f;

            foreach (CharacterColorCache target in _targets)
            {
                Color32[] colors = textInfo.meshInfo[target.MaterialIndex].colors32;
                if (target.VertexIndex + 3 >= colors.Length)
                    continue;

                colors[target.VertexIndex] = LerpToWhite(target.Color0, t);
                colors[target.VertexIndex + 1] = LerpToWhite(target.Color1, t);
                colors[target.VertexIndex + 2] = LerpToWhite(target.Color2, t);
                colors[target.VertexIndex + 3] = LerpToWhite(target.Color3, t);
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.colors32 = meshInfo.colors32;
                _textMeshPro.UpdateGeometry(meshInfo.mesh, i);
            }
        }

        private static bool MatchesAt(string text, int index, string value)
        {
            if (index + value.Length > text.Length)
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                if (text[index + i] != value[i])
                    return false;
            }

            return true;
        }

        private static Color32 LerpToWhite(Color32 original, float t)
        {
            return (Color32)Color.Lerp(original, Color.white, t);
        }

        private readonly struct PikaRange
        {
            public readonly int Start;
            public readonly int End;

            public PikaRange(int start, int end)
            {
                Start = start;
                End = end;
            }
        }

        private readonly struct CharacterColorCache
        {
            public readonly int MaterialIndex;
            public readonly int VertexIndex;
            public readonly Color32 Color0;
            public readonly Color32 Color1;
            public readonly Color32 Color2;
            public readonly Color32 Color3;

            public CharacterColorCache(
                int materialIndex,
                int vertexIndex,
                Color32 color0,
                Color32 color1,
                Color32 color2,
                Color32 color3)
            {
                MaterialIndex = materialIndex;
                VertexIndex = vertexIndex;
                Color0 = color0;
                Color1 = color1;
                Color2 = color2;
                Color3 = color3;
            }
        }
    }
}
