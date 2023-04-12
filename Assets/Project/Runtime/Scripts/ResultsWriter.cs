using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ResultsWriter : MonoBehaviour
{
    [Tooltip("The TextMeshProUGUI component to write to.")]
    [SerializeField] private TextMeshProUGUI text;
    
    private InferenceManager _im;
    private float[] _prevResults;

    private void Start()
    {
        _im = FindObjectOfType<InferenceManager>();
        //_im.CalculateInferenceResult();
    }

    private void Update()
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
