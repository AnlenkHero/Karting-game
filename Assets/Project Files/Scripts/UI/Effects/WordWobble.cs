using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class WordWobble : MonoBehaviour
{
    [System.Serializable]
    public struct AnimationSettings
    {
        public float verticesOffset1;
        public float verticesOffset2;
        public float verticesOffset3;
        public float verticesOffset4;
        public float colorLength1;
        public float colorLength2;
        public float colorLength3;
        public float colorLength4;
        public float colorSpeed1;
        public float colorSpeed2;
        public float colorSpeed3;
        public float colorSpeed4;
        public float speedMultiplier;
        public float amplitudeMultiplier;
        public float riseDuration;
        public float fallDuration;
        public float delayBetweenLetters;
    }

    public enum WobbleMode
    {
        SinCos,
        Spiral,
        Random,
        Pulse,
        Wave,
        Jitter,
        QueueRise,
        OscillatingRotation,
        Ripple,
        Custom
    }

    public enum GradientDirection
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop,
        DiagonalLeftToRight,
        DiagonalRightToLeft,
        DiagonalTopToBottom,
        DiagonalBottomToTop
    }

    public enum GradientMode
    {
        Static,
        Animated
    }

    [SerializeField] private float updateInterval = 0.05f;

    [SerializeField] private WobbleMode wobbleMode = WobbleMode.SinCos;
    [SerializeField] private bool useParallel;
    [SerializeField] private bool usePerLetterGradient;


    [SerializeField] private GradientDirection gradientDirection = GradientDirection.LeftToRight;
    [SerializeField] private GradientMode gradientMode = GradientMode.Animated;

    [SerializeField] private AnimationSettings animationSettings = new AnimationSettings
    {
        verticesOffset1 = 3.5f,
        verticesOffset2 = 3f,
        verticesOffset3 = 1.5f,
        verticesOffset4 = 2f,
        colorLength1 = 1.2f,
        colorLength2 = 1.3f,
        colorLength3 = 1.4f,
        colorLength4 = 1.5f,
        colorSpeed1 = 0.0004f,
        colorSpeed2 = 0.0007f,
        colorSpeed3 = 0.0007f,
        colorSpeed4 = 0.0007f,
        speedMultiplier = 2f,
        amplitudeMultiplier = 10f,
        riseDuration = 0.5f,
        fallDuration = 0.5f,
        delayBetweenLetters = 0.2f
    };

    public Gradient rainbow;

    private TMP_Text _textMesh;
    private Mesh _mesh;
    private Vector3[] _vertices;
    private Color[] _colors;
    private TMP_TextInfo _textInfo;
    private Coroutine _animationCoroutine;

    private void Start()
    {
        _textMesh = GetComponent<TMP_Text>();
        _textMesh.ForceMeshUpdate();
        _textInfo = _textMesh.textInfo;

        _mesh = _textMesh.mesh;
        _vertices = _mesh.vertices;
        _colors = new Color[_vertices.Length];

        _animationCoroutine = StartCoroutine(AnimateText());
    }

    private IEnumerator AnimateText()
    {
        while (true)
        {
            _textMesh.ForceMeshUpdate();
            for (int i = 0; i < _textInfo.characterCount; i++)
            {
                var charInfo = _textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                float timeOffset = useParallel ? i * animationSettings.delayBetweenLetters : i;
                Vector3 offset = ApplyWobbleEffect(Time.time + timeOffset, i, wobbleMode);

                for (int j = 0; j < 4; ++j)
                {
                    int vertexIndex = charInfo.vertexIndex + j;
                    _vertices[vertexIndex] = _textInfo.meshInfo[charInfo.materialReferenceIndex].vertices[vertexIndex] +
                                             offset * GetOffsetByIndex(j);

                    if (usePerLetterGradient)
                    {
                        _colors[vertexIndex] = GetSmoothGradientColor(i, j);
                    }
                    else
                    {
                        if (gradientMode == GradientMode.Animated)
                        {
                            _colors[vertexIndex] = rainbow.Evaluate(Mathf.Repeat(
                                Time.time + _vertices[vertexIndex].x * GetColorSpeedByIndex(j),
                                GetColorLengthByIndex(j)));
                        }
                        else
                        {
                            _colors[vertexIndex] = rainbow.Evaluate(
                                _vertices[vertexIndex].x * GetColorSpeedByIndex(j) * GetColorLengthByIndex(j));
                        }
                    }
                }
            }

            _mesh.vertices = _vertices;
            _mesh.colors = _colors;
            _textMesh.canvasRenderer.SetMesh(_mesh);

            yield return new WaitForSeconds(updateInterval);
        }
    }

    private Vector3 ApplyWobbleEffect(float time, int index, WobbleMode mode)
    {
        switch (mode)
        {
            case WobbleMode.SinCos:
                return new Vector2(Mathf.Sin(time * animationSettings.speedMultiplier),
                    Mathf.Cos(time * animationSettings.speedMultiplier)) * animationSettings.amplitudeMultiplier;
            case WobbleMode.Spiral:
                return new Vector2(Mathf.Cos(time * animationSettings.speedMultiplier) * 2f,
                    Mathf.Sin(time * animationSettings.speedMultiplier) * 2f) * animationSettings.amplitudeMultiplier;
            case WobbleMode.Random:
                return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) *
                       animationSettings.amplitudeMultiplier;
            case WobbleMode.Pulse:
                return new Vector2(0,
                    Mathf.Sin(time * animationSettings.speedMultiplier) * animationSettings.amplitudeMultiplier);
            case WobbleMode.Wave:
                return new Vector2(Mathf.Sin(time * animationSettings.speedMultiplier),
                           Mathf.Sin(time * animationSettings.speedMultiplier + time * 2)) *
                       animationSettings.amplitudeMultiplier;
            case WobbleMode.Jitter:
                return new Vector2(Mathf.PerlinNoise(time * animationSettings.speedMultiplier, 0),
                           Mathf.PerlinNoise(0, time * animationSettings.speedMultiplier)) *
                       animationSettings.amplitudeMultiplier;
            case WobbleMode.QueueRise:
                return CalculateQueueRiseOffset(time, index);
            case WobbleMode.Ripple:
                return new Vector2(Mathf.Sin(time * animationSettings.speedMultiplier + index * 0.5f),
                           Mathf.Sin(time * animationSettings.speedMultiplier + index * 0.5f)) *
                       animationSettings.amplitudeMultiplier;
            case WobbleMode.OscillatingRotation:
                return CalculateOscillatingRotationOffset(time, index);
            case WobbleMode.Custom:
                return CustomWobble(time);
            default:
                return Vector2.zero;
        }
    }

    private Vector3 CalculateQueueRiseOffset(float time, int index)
    {
        float totalDuration = animationSettings.riseDuration + animationSettings.fallDuration;
        float startDelay = index * animationSettings.delayBetweenLetters;
        float elapsed = time - startDelay;

        if (elapsed < 0)
            return Vector3.zero;

        float phase = (elapsed % totalDuration) / totalDuration;

        if (phase < animationSettings.riseDuration / totalDuration)
        {
            float risePhase = phase / (animationSettings.riseDuration / totalDuration);
            return new Vector3(0, Mathf.Sin(risePhase * Mathf.PI) * animationSettings.amplitudeMultiplier, 0);
        }
        else
        {
            float fallPhase = (phase - animationSettings.riseDuration / totalDuration) /
                              (animationSettings.fallDuration / totalDuration);
            return new Vector3(0, -Mathf.Sin(fallPhase * Mathf.PI) * animationSettings.amplitudeMultiplier, 0);
        }
    }

    private Vector3 CustomWobble(float time)
    {
        // Custom wobble logic
        return new Vector2(Mathf.Sin(time), Mathf.Cos(time) * Mathf.Sin(time));
    }

    private Vector3 CalculateOscillatingRotationOffset(float time, int index)
    {
        float speed = animationSettings.speedMultiplier;
        float angle = Mathf.Sin(time * speed + index * animationSettings.delayBetweenLetters) * Mathf.PI * animationSettings.amplitudeMultiplier;
        return new Vector3(0, 0, angle);
    }

    private Color GetSmoothGradientColor(int characterIndex, int vertexIndex)
    {
        float positionInGradient = 0f;
        switch (gradientDirection)
        {
            case GradientDirection.LeftToRight:
                positionInGradient = (float)characterIndex / _textInfo.characterCount;
                break;
            case GradientDirection.RightToLeft:
                positionInGradient = 1f - (float)characterIndex / _textInfo.characterCount;
                break;
            case GradientDirection.TopToBottom:
                positionInGradient = (float)vertexIndex / 4;
                break;
            case GradientDirection.BottomToTop:
                positionInGradient = 1f - (float)vertexIndex / 4;
                break;
            case GradientDirection.DiagonalLeftToRight:
                positionInGradient = ((float)characterIndex / _textInfo.characterCount + (float)vertexIndex / 4) / 2f;
                break;
            case GradientDirection.DiagonalRightToLeft:
                positionInGradient =
                    (1f - (float)characterIndex / _textInfo.characterCount + (1f - (float)vertexIndex / 4)) / 2f;
                break;
            case GradientDirection.DiagonalTopToBottom:
                positionInGradient = ((float)vertexIndex / 4 + (float)characterIndex / _textInfo.characterCount) / 2f;
                break;
            case GradientDirection.DiagonalBottomToTop:
                positionInGradient =
                    (1f - (float)vertexIndex / 4 + (1f - (float)characterIndex / _textInfo.characterCount)) / 2f;
                break;
            default:
                positionInGradient = (float)characterIndex / _textInfo.characterCount;
                break;
        }

        if (gradientMode == GradientMode.Static)
        {
            return rainbow.Evaluate(positionInGradient);
        }
        else
        {
            return rainbow.Evaluate(Mathf.Repeat(Time.time + positionInGradient, 1f));
        }
    }

    private float GetOffsetByIndex(int index)
    {
        return index switch
        {
            0 => animationSettings.verticesOffset1,
            1 => animationSettings.verticesOffset2,
            2 => animationSettings.verticesOffset3,
            _ => animationSettings.verticesOffset4,
        };
    }

    private float GetColorSpeedByIndex(int index)
    {
        return index switch
        {
            0 => animationSettings.colorSpeed1,
            1 => animationSettings.colorSpeed2,
            2 => animationSettings.colorSpeed3,
            _ => animationSettings.colorSpeed4,
        };
    }

    private float GetColorLengthByIndex(int index)
    {
        return index switch
        {
            0 => animationSettings.colorLength1,
            1 => animationSettings.colorLength2,
            2 => animationSettings.colorLength3,
            _ => animationSettings.colorLength4,
        };
    }

    private void OnDisable()
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }
    }
    
}
