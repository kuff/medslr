using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ResultsManager : MonoBehaviour
{
    private InferenceManager _im;
    private GestureManager _gm;

    void Start()
    {
        _im = FindObjectOfType<InferenceManager>();
        _gm = FindObjectOfType<GestureManager>();
    }

    public Dictionary<char, float> GetResults()
    {
        return GetResultsRaw().Item1;
    }

    public (Dictionary<char, float>, List<float>, float[]) GetResultsRaw()
    {
        var inferenceResult = _im.GetInferenceResult();
        inferenceResult = inferenceResult.Take(inferenceResult.Length - 1).ToArray();  // Ignore result for " " character

        var gestureResult = _gm.GetGestureResult().ToList();
        var result = new List<float>(gestureResult);

        //Debug.Log($"im: {inferenceResult.Length}, gm: {gestureResult.Length}");

        // Define linear order for each results array
        var gestureRanked = gestureResult.OrderBy(x => x).ToList();

        // Dynamic weighting algorithm
        const int c = 12;
        var d = gestureRanked[0] / (gestureRanked[1] * 2) * c;  // <- * 2 For debugging
        //Debug.Log($"d: {d}, gestureRanked[0]: {gestureRanked[0]}, inferenceResult[gestureResult.IndexOf(gestureRanked[i])]: {inferenceResult[gestureResult.IndexOf(gestureRanked[0])]}");
        for (var i = 0; i < c; i++)
            result[gestureResult.IndexOf(gestureRanked[i])] = gestureRanked[i] - (Mathf.Max(inferenceResult[gestureResult.IndexOf(gestureRanked[i])], 0.4f) * d);

        // Construct the dictionary
        var vocab = VocabularyProvider.GetVocabArray();
        var dictionary = new Dictionary<char, float>();
        for (var i = 0; i < result.Count; i++)
            dictionary[vocab[i]] = result[i];

        // Sort the dictionary by values
        var sortedDictionary = dictionary
            .OrderBy(x => -x.Value)
            .ToDictionary(x => x.Key, x => x.Value);

        // Log result
        // TODO: Will need refactor if this method is called more than once per inference window
        var gestureString = new StringBuilder();
        var inferenceString = new StringBuilder();
        var resultString = new StringBuilder();
        for (var i = 0; i < gestureResult.Count; i++) gestureString.AppendFormat("{0} ", gestureResult[i]);
        HandLogger.Log(HandLogger.LogType.GestureResult, gestureString.ToString(), ignorePrevious: true);
        for (var i = 0; i < inferenceResult.Length; i++) inferenceString.AppendFormat("{0} ", inferenceResult[i]);
        HandLogger.Log(HandLogger.LogType.InferenceResult, inferenceString.ToString(), ignorePrevious: true);
        for (var i = 0; i < dictionary.Values.Count; i++) resultString.AppendFormat("{0} ", dictionary.Values.ToArray()[i]);
        HandLogger.Log(HandLogger.LogType.FusionResult, resultString.ToString(), ignorePrevious: true);

        return (sortedDictionary, gestureResult, inferenceResult);
    }
}
