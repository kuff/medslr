using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoLoader : MonoBehaviour
{
    [Tooltip("The path in which to look for video files (relative to Assets/Resources/)")]
    public string videoFolderPath = "GUI/Videos";
    
    public VideoClip[] VideoClips { get; private set; }

    private void Start()
    {
        LoadVideoFiles();
    }

    private void LoadVideoFiles()
    {
        // Load all videos from the specified folder
        var loadedVideos = Resources.LoadAll<VideoClip>(videoFolderPath);
        VideoClips = new VideoClip[loadedVideos.Length];

        // Handle the incorrect ordering of æ, ø, and å in the file system
        for (var i = 0; i < VideoClips.Length; i++)
        {
            var targetIndex = loadedVideos[i].name switch
            {
                "æ" => i - 1,
                "ø" => i - 1,
                "å" => i + 2,
                _ => i,
            };
            VideoClips[targetIndex] = loadedVideos[i];  // Add the loaded videos to the videoClips list
        }

        // Warn in case the video weren't found
        if (VideoClips.Length == 0) Debug.LogError("No video files found in the specified folder.");
    }
}