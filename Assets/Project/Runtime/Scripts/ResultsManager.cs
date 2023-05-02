using System.Collections.Generic;
using System.Linq;
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
        var c = 12;
        var d = gestureRanked[0] / gestureRanked[1] * c;
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

        return (sortedDictionary, gestureResult, inferenceResult);
    }
}
