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
            var inferenceResult = _im.GetInferenceResult();
            var userSentence = _tm.GetUserSentence();
            Debug.Log(userSentence);
            var targetCharacter = _tm.GetTargetCharacter();
            for (var i = 0; i < userSentence.Length; i++) targetCharacter = i < 4 ? string.Concat(i, targetCharacter) : string.Concat(" ", targetCharacter);
            Debug.Log(targetCharacter + "\n");
            for (var i = 0; i < Vocab.Length; i++) Debug.Log($"{Vocab[i]}: {inferenceResult[i]},");
            
            _tm.Step();
        }
    }
}
