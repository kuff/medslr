using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ResultsWriter : MonoBehaviour
{
    [Tooltip("The TextMeshProUGUI component to write to.")]
    [SerializeField] private TextMeshProUGUI text;
    
    private InferenceManager _im;
    private GestureManager _gm;
    private float[] _prevResults;

    private void Start()
    {
        _im = FindObjectOfType<InferenceManager>();
        _gm = FindObjectOfType<GestureManager>();
    }

    private void Update()
    {
        //OldUpdate();
        PrintResults();
    }

    private void PrintResults()
    {
        var inferenceResult = _im.GetInferenceResult();
        var gestureResult = _gm.GetGestureResult()?.ToList();

        if (inferenceResult == _prevResults || inferenceResult == null || inferenceResult.Length == 0) return;
        _prevResults = inferenceResult;
        inferenceResult = inferenceResult.Take(inferenceResult.Length - 1).ToArray();  // Ignore result for " " character

        var result = new List<float>(gestureResult);

        //Debug.Log($"im: {inferenceResult.Length}, gm: {gestureResult.Length}");

        // Define linear order for each results array
        var gestureRanked = gestureResult.OrderBy(x => x).ToList();

        // Create array of aggregate descrete order values
        var c = 12;
        var d = gestureRanked[0] / gestureRanked[1] * c;
        Debug.Log($"d: {d}, gestureRanked[0]: {gestureRanked[0]}, inferenceResult[gestureResult.IndexOf(gestureRanked[i])]: {inferenceResult[gestureResult.IndexOf(gestureRanked[0])]}");
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

        // Construct list text from sortedDictionary for the text UI element
        var resultString = sortedDictionary.Aggregate("", (current, pair) => current + $"{pair.Key}: {pair.Value:F4}, {inferenceResult[vocab.ToList().IndexOf(pair.Key)]:F4}, {gestureResult[vocab.ToList().IndexOf(pair.Key)]:F4}\n");

        // Update UI element text
        text.SetText(resultString + $"\ninput: {InferenceManager.GetInputString()}");
    }

    private void OldUpdate()
    {
        var results = _im.GetInferenceResult();

        // Stop if the data is not new or undefined
        if (results == _prevResults || results == null || results.Length == 0) return;
        _prevResults = results;

        results = results.Take(results.Length - 1).ToArray();  // Ignore result for " " character

        // Construct the dictionary
        var vocab = VocabularyProvider.GetVocabArray();
        var dictionary = new Dictionary<char, float>();
        for (var i = 0; i < results.Length; i++) dictionary[vocab[i]] = results[i];

        // Sort the dictionary by values
        var sortedDictionary = dictionary.OrderBy(x => x.Value)
            .ToDictionary(x => x.Key, x => x.Value);

        // Construct list text from sortedDictionary for the text UI element
        var resultString = sortedDictionary.Aggregate("", (current, pair) => current + $"{pair.Key}: {pair.Value:F4}\n");

        // Update UI element text
        text.SetText(resultString + $"\ninput: {InferenceManager.GetInputString()}");
    }
}
