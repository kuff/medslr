using System.Linq;
using TMPro;
using UnityEngine;

public class ResultsWriter : MonoBehaviour
{
    [Tooltip("The TextMeshProUGUI component to write to.")]
    [SerializeField] private TextMeshProUGUI text;
    private ResultsManager _rm;

    private void Start()
    {
        _rm = FindObjectOfType<ResultsManager>();
    }

    public void PrintResults()
    {
        // Construct list text from sortedDictionary for the text UI element
        var (sortedDictionary, gestureResult, inferenceResult) = _rm.GetResultsRaw();
        var vocab = VocabularyProvider.GetVocabArray();
        var resultString = sortedDictionary.Aggregate("", (current, pair) => current + $"{pair.Key}: {pair.Value:F4}, {inferenceResult[vocab.ToList().IndexOf(pair.Key)]:F4}, {gestureResult[vocab.ToList().IndexOf(pair.Key)]:F4}\n");

        // Update UI element text
        text.SetText(resultString + $"\ninput: {InferenceManager.GetInputString()}");
    }
}
