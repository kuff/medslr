using JetBrains.Annotations;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Text;

[RequireComponent(typeof(HandProvider))]
[RequireComponent(typeof(TextManager))]
public class GestureManager : MonoBehaviour
{
    [SerializeField] private int boneCount;
    [SerializeField] private int captureInterval;
    [SerializeField] private int processInterval;

    private HandProvider _handProvider;
    private TextManager _tm;
    private OVRSkeleton _activeHand;

    private bool _captureGestures;
    private int _captureIndex;
    private Vector3[,] _boneCaptures;
    private Vector3 _handRootPosition;

    private float[] _result;
    private int _processCount;

    private void Start()
    {
        _handProvider = GetComponent<HandProvider>();
        _tm = GetComponent<TextManager>();

        _captureGestures = false;
        ResetCapture();
    }

    private void ResetCapture()
    {
        _boneCaptures = new Vector3[captureInterval, boneCount];
        _captureIndex = 0;
        _result = new float[TargetVectors.all.GetLength(0)];
    }

    private void Update()
    {
        if (!_captureGestures || _captureIndex == captureInterval) return;

        var bones = _activeHand.Bones;
        for (var i = 0; i < boneCount; i++)
            _boneCaptures[_captureIndex, i] = _handRootPosition - bones[i].Transform.position;

        _captureIndex++;
        _processCount++;

        if (_processCount > processInterval)
        {
            _processCount = 0;

            var (_, aggregateValues) = GetMeanAndAggregateResults();
            _result = GetDeltaValues(in aggregateValues);
        }
    }

    public void BeginGestureCapture()
    {
        ResetCapture();

        _activeHand = _handProvider.GetActiveHand();
        _handRootPosition = _activeHand.Bones[0].Transform.position;

        // Capture initial bone positions
        var positions = "";
        for (var i = 0; i < boneCount; i++)
        {
            var delta = _handRootPosition - _activeHand.Bones[i].Transform.position;
            positions += $"{delta.x} {delta.y} {delta.z} ";
        }
        HandLogger.Log(HandLogger.LogType.InitialBonePositions, positions, ignorePrevious: true);

        _captureGestures = true;
    }

    public void EndGestureCapture()
    {
        _captureGestures = false;

        var (_, aggregateValues) = GetMeanAndAggregateResults();
        _result = GetDeltaValues(in aggregateValues);

#if UNITY_EDITOR
        var gestureString = $"GESTURES target: {_tm.GetTargetCharacter()}";
        for (var i = 0; i < TargetVectors.all.GetLength(0); i++)
            gestureString += $", {VocabularyProvider.GetVocabArrayJustLetters()[i]}: {_result[i]}";
        Debug.Log(gestureString);
#endif

        var activeHandIndex = _activeHand.GetSkeletonType() == OVRSkeleton.SkeletonType.HandLeft ? 0 : 1;
        var sb = new StringBuilder();
        sb.Append(activeHandIndex);
        for (var i = 0; i < boneCount; i++)
            sb.AppendFormat(" {0} {1} {2};", aggregateValues[i].x, aggregateValues[i].y, aggregateValues[i].z);
        HandLogger.Log(HandLogger.LogType.HandCapture, sb.ToString(), ignorePrevious: true);
    }

    private float[] GetDeltaValues(in Vector3[] aggregateResults)
    {
        var vocabCount = TargetVectors.all.GetLength(0);
        var totalDeltas = new float[vocabCount];
        for (var i = 0; i < vocabCount; i++)
        {
            var boneDelta = GetBoneDeltaValues(in aggregateResults, in i).Sum();
            totalDeltas[i] = boneDelta;
        }
        return totalDeltas;
    }

    public List<float> GetBoneDeltaValues(in Vector3[] boneCaptures, in int vocabIndex)
    {
        var boneCount = TargetVectors.all.GetLength(1);
        var delta = new List<float>(boneCount);
        for (var i = 0; i < boneCount; i++)
        {
            var targetVector = new Vector3(TargetVectors.all[vocabIndex, i, 0],
                                           TargetVectors.all[vocabIndex, i, 1],
                                           TargetVectors.all[vocabIndex, i, 2]);
            delta.Add(Vector3.Distance(targetVector, boneCaptures[i]));
        }
        return delta;
    }

    [CanBeNull]
    public ref float[] GetGestureResult()
    {
        return ref _result;
    }

    public (Vector3[], Vector3[]) GetMeanAndAggregateResults()
    {
        // Calculate mean- and distance parameters
        var captureMaxLength = Mathf.Min(_boneCaptures.GetLength(0), captureInterval);
        var meanCaptures = new Vector3[boneCount];
        var aggregateCaptures = new Vector3[boneCount];
        for (var i = 0; i < boneCount; i++)
        {
            var aggregateForSpecificIndex = 0f;
            for (var j = 1; j < captureMaxLength; j++)
            {
                meanCaptures[i] += _boneCaptures[j, i];
                aggregateForSpecificIndex += Vector3.Distance(_boneCaptures[j - 1, i], _boneCaptures[j, i]);
            }

            meanCaptures[i] /= captureMaxLength;
            var deltaVector = _boneCaptures[0, i] - meanCaptures[i];
            var distanceVector = deltaVector.normalized * aggregateForSpecificIndex;
            aggregateCaptures[i] = distanceVector;
        }

        return (meanCaptures, aggregateCaptures);
    }
}