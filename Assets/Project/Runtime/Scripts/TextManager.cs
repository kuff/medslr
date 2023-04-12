using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    private static readonly string Vocab = VocabularyProvider.GetVocabJustLetters();
    
    [SerializeField] private List<string> sentences;
    
    private int _activeIndex;    // The index of the currently active sentence
    private int _userIndex;      // The index in the active sentence which the user is attempting to replicate

    private int _userProgress;   // The user's progress (in letters completed, including special characters)
    private int _totalProgress;  // The total progress (in letters to be completed across all sentences)

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        _activeIndex = 0;
        _userIndex = 0;

        _userProgress = 0;
        _totalProgress = sentences.SelectMany(s => s).Count(char.IsLetter);
    }

    public string GetActiveSentence()
    {
        return _activeIndex >= sentences.Count ? "" : sentences[_activeIndex];
    }

    public string GetUserSentence()
    {
        var activeSentence = GetActiveSentence();
        return activeSentence != "" ? activeSentence[.._userIndex] : "";
    }

    public string GetTargetCharacter()
    {
        var activeSentence = GetActiveSentence();
        return activeSentence == "" ? "" : activeSentence[_userIndex].ToString();
    }

    public void Step()
    {
        var stepProgress = true;
        while (true)
        {
            if (HasConcluded()) break;

            _userIndex++;
            if (stepProgress) _userProgress++;

            if (_userIndex == GetActiveSentence().Length)
            {
                _activeIndex++;
                _userIndex = 0;
                break;
            }

            if (Vocab.IndexOf(GetTargetCharacter().ToLower(), StringComparison.Ordinal) != -1) break;
            stepProgress = false;
        }
    }

    public bool HasConcluded()
    {
        return _activeIndex == sentences.Count;
    }

    public int GetActiveSentenceNumber()
    {
        return _activeIndex + 1;
    }

    public int GetTotalSentenceNumber()
    {
        return sentences.Count;
    }

    public int GetUserProgress()
    {
        return _userProgress;
    }

    public int GetTotalProgress()
    {
        return _totalProgress;
    }
}
