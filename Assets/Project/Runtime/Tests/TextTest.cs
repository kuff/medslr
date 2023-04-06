using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TextTest
{
    private TextManager _textManager;
    private Scene _testScene;
    private List<string> _sentences;

    [SetUp]
    public void SetUp()
    {
        // Load InferenceTestScene
        EditorSceneManager.OpenScene("Assets/Scenes/InferenceTestScene.unity", OpenSceneMode.Single);
        _testScene = SceneManager.GetActiveScene();

        // Find test object
        var testObject = (GameObject)_testScene.GetRootGameObjects().GetValue(0);

        // Find test component
        _textManager = testObject.GetComponent<TextManager>();
        _textManager.Initialize();

        _sentences = new List<string>
        {
            "Wow! Tusindvis af fisk!",
            "Hvordan har du det?",
            "Jeg er mæt, ellers tak!",
            "Jeg har en storesøster.",
            "Jeg så en kat på taget."
        };
    }

    private static int GetNonAlphabeticCharacters(string sentence)
    {
        return sentence.Count(c => !char.IsLetter(c));
    }

    [Test]
    public void ActiveSentence_ChangesAfterCompletingPreviousSentence()
    {
        foreach (var sentence in _sentences)
        {
            for (var i = 0; i < sentence.Length - GetNonAlphabeticCharacters(sentence); i++)
            {
                Assert.AreEqual(sentence, _textManager.GetActiveSentence());
                _textManager.Step();
            }
        }
    }

    [Test]
    public void UserSentence_UpdatesCorrectlyWhenStepping()
    {
        // TODO: Update to test all sentences...
        // ReSharper disable once CommentTypo
        // NOTE: This logic is currently hardcoded for the sentence "Wow! Tusindvis af fisk!"
        Assert.AreEqual(string.Empty, _textManager.GetUserSentence());
        
        _textManager.Step();
        Assert.AreEqual(_sentences[0][0].ToString(), _textManager.GetUserSentence());

        for (var i = 0; i < _sentences[0].Length - GetNonAlphabeticCharacters(_sentences[0]) - 2; i++)
        {
            _textManager.Step();
        }

        Assert.AreEqual(_sentences[0][..^2], _textManager.GetUserSentence());
        
        _textManager.Step();
        Assert.AreEqual(string.Empty, _textManager.GetUserSentence());
    }

    [Test]
    public void TargetCharacter_UpdatesCorrectlyWhenStepping()
    {
        // TODO: Update to test all sentences...
        Assert.AreEqual(_sentences[0][0].ToString(), _textManager.GetTargetCharacter());

        _textManager.Step();

        Assert.AreEqual(_sentences[0][1].ToString(), _textManager.GetTargetCharacter());
    }

    [Test]
    public void Stepping_AdvancesSentencesAndResetsUserIndex()
    {
        foreach (var sentence in _sentences)
        {
            for (var i = 0; i < sentence.Length - GetNonAlphabeticCharacters(sentence); i++)
            {
                Assert.AreEqual(sentence, _textManager.GetActiveSentence());
                _textManager.Step();
            }
        }
    }

    [Test]
    public void ConclusionState_IsCorrectAfterSteppingThroughAllSentences()
    {
        var nonAlphabeticCharacters = _sentences.Sum(GetNonAlphabeticCharacters);
        for (var i = 0; i < _sentences.Sum(s => s.Length) - nonAlphabeticCharacters; i++)
        {
            Assert.IsFalse(_textManager.HasConcluded());
            _textManager.Step();
        }

        Assert.IsTrue(_textManager.HasConcluded());
        
        _textManager.Step();
        Assert.IsTrue(_textManager.HasConcluded());
    }

    [Test]
    public void ActiveSentenceNumber_UpdatesCorrectlyWhenStepping()
    {
        for (var j = 0; j < _sentences.Count; j++)
        {
            for (var i = 0; i < _sentences[j].Length - GetNonAlphabeticCharacters(_sentences[j]); i++)
            {
                Assert.AreEqual(j + 1, _textManager.GetActiveSentenceNumber());
                _textManager.Step();
            }
        }
    }

    [Test]
    public void TotalSentenceNumber_ReturnsCorrectValue()
    {
        Assert.AreEqual(_sentences.Count, _textManager.GetTotalSentenceNumber());
    }

    [Test]
    public void UserProgress_UpdatesCorrectlyWhenStepping()
    {
        var totalProgress = 0;
        foreach (var sentence in _sentences)
        {
            Assert.AreEqual(totalProgress, _textManager.GetUserProgress());
            totalProgress += sentence.Length;

            for (var i = 0; i < sentence.Length - GetNonAlphabeticCharacters(sentence); i++)
            {
                _textManager.Step();
            }

            Assert.AreEqual(totalProgress, _textManager.GetUserProgress());
        }
    }

    [Test]
    public void TotalProgress_ReturnsCorrectValue()
    {
        Assert.AreEqual(_sentences.Sum(s => s.Length), _textManager.GetTotalProgress());
    }
}
