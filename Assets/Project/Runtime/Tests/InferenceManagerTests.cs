using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InferenceManagerTests
{
    private static readonly string Vocab = VocabularyProvider.GetVocab();

    private Scene _testScene;
    
    private TextManager _tm;
    private InferenceManager _im;

    [SetUp]
    public void Setup()
    {
        // Load InferenceTestScene
        EditorSceneManager.OpenScene("Assets/Scenes/InferenceTestScene.unity", OpenSceneMode.Single);
        _testScene = SceneManager.GetActiveScene();

        // Find test object
        var testObject = (GameObject)_testScene.GetRootGameObjects().GetValue(0);

        // Find test components
        _tm = testObject.GetComponent<TextManager>();
        _im = testObject.GetComponent<InferenceManager>();
        _im.Initialize();
    }
    
//     [TearDown]
//     public void Teardown()
//     {
// #pragma warning disable CS0618
//         SceneManager.UnloadScene(_testScene);
// #pragma warning restore CS0618
//     }
    
    [Test]
    public void PerformsInferenceOnTextManagerInput()
    {
        while (!_tm.HasConcluded())
        {
            Debug.Log("\nResults:\n");
            var inferenceResult = _im.CalculateInferenceResult();
            var userSentence = _tm.GetUserSentence();
            Debug.Log(userSentence);
            var targetCharacter = _tm.GetTargetCharacter();
            for (var i = 0; i < userSentence.Length; i++) targetCharacter = i < 4 ? string.Concat(i, targetCharacter) : string.Concat(" ", targetCharacter);
            Debug.Log(targetCharacter + "\n");
            
            // for (var i = 0; i < Vocab.Length; i++) Debug.Log($"{Vocab[i]}: {inferenceResult[i]},");
            var results = inferenceResult.ToList().Take(inferenceResult.Length/* - 1*/).ToArray();
        
            // Construct the dictionary
            var vocab = VocabularyProvider.GetVocabArray();
            var dictionary = new Dictionary<char, float>();
            for (var i = 0; i < results.Length; i++) dictionary[vocab[i]] = results[i];

            // Sort the dictionary by values
            var sortedDictionary = dictionary.OrderBy(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value);
        
            // Construct list text from sortedDictionary for the text UI element
            var resultString = sortedDictionary.Aggregate("", (current, pair) => current + $"{pair.Key}: {pair.Value:F4}\n");
            Debug.Log(resultString);
            
            _tm.Step();
        }
    }
}
