using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Video;

[RequireComponent(typeof(VideoLoader))]
public class VideoManager : MonoBehaviour
{
    [Tooltip("The VideoPlayer on which to manage playback.")]
    [SerializeField] private VideoPlayer player;
    
    private VideoLoader _loader;
    private TextManager _tm;
    private char _prevCharacter;

    private void Start()
    {
        _loader = GetComponent<VideoLoader>();
        _tm = FindObjectOfType<TextManager>();
    }

    private void Update()
    {
        var targetString = _tm.GetTargetCharacter();
        if (targetString.Length == 0) return;

        var targetCharacter = targetString[0];
        if (targetCharacter == _prevCharacter) return;
        
        PlayClip(targetCharacter);
        _prevCharacter = targetCharacter;
    }

    private void PlayClip(char targetCharacter)
    {
        var vocab = VocabularyProvider.GetVocabArrayJustLetters().ToList();
        var clips = _loader.VideoClips;

        var targetIndex = vocab.IndexOf(char.ToLower(targetCharacter));
        PlayClip(clips[targetIndex]);
    }

    private void PlayClip(VideoClip clip)
    {
        player.clip = clip;
        player.Play();
    }
}
