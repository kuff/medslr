using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoLoader : MonoBehaviour
{
    [Tooltip("The path in which to look for video files (relative to Assets/Resources/)")]
    public string videoFolderPath = "GUI/Videos";
    
    public List<VideoClip> VideoClips { get; private set; }

    private void Start()
    {
        LoadVideoFiles();
    }

    private void LoadVideoFiles()
    {
        VideoClips = new List<VideoClip>();

        // Load all videos from the specified folder
        var loadedVideos = Resources.LoadAll<VideoClip>(videoFolderPath);

        // Add the loaded videos to the videoClips list
        foreach (var videoClip in loadedVideos)
        {
            VideoClips.Add(videoClip);
        }

        if (VideoClips.Count == 0)
        {
            Debug.LogError("No video files found in the specified folder.");
        }
    }
}