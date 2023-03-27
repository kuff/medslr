using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    [SerializeField] private List<string> sentences;
    
    private int _activeIndex;  // The index of the currently active sentence
    private int _userIndex;    // The index in the active sentence which the user is attempting to replicate

    private int _userProgress;
    private int _totalProgress;

    private void Start()
    {
        _activeIndex = 0;
        _userIndex = 0;

        _userProgress = 0;
        _totalProgress = sentences.Sum(sentence => sentence.Length);
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
        if (HasConcluded()) return;
        
        _userIndex++;
        _userProgress++;
        if (_userIndex != GetActiveSentence().Length) return;
        
        _activeIndex++;
        _userIndex = 0;
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
