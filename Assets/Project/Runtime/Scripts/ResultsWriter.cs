using System.Linq;
using TMPro;
using UnityEngine;

public class ResultsWriter : MonoBehaviour
{
    [Tooltip("The TextMeshProUGUI component to write to")]
    [SerializeField] private TextMeshProUGUI text;
    
    private InferenceManager _im;

    private void Start()
    {
        _im = FindObjectOfType<InferenceManager>();
        // _im.CalculateInferenceResult();
    }

    private void Update()
    {
        var results = _im.GetInferenceResult();
        if (results == null || results.Length == 0) return;
        
        results.ToList().Sort();  // TODO: will need changing once the final result is a confluence of both nl and gr

        var vocab = VocabularyProvider.GetVocabArray();
        var resultString = "";
        for (var i = 0; i < results.Length; i++)
        {
            var roundedResult = Mathf.Round(results[i] * 10_000) / 10_000f;

            resultString += $"{vocab[i]}: {roundedResult} ";
        }

        text.SetText(resultString);
    }
}
