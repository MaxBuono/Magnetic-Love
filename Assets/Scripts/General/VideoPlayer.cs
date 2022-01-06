using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayer : MonoBehaviour
{
    private UnityEngine.Video.VideoPlayer _videoPlayer;

    private void Awake()
    {
        _videoPlayer = GetComponent<UnityEngine.Video.VideoPlayer>();
        _videoPlayer.Prepare();
    }

    void Start()
    {
        // Start playback. This means the VideoPlayer may have to prepare (reserve
        // resources, pre-load a few frames, etc.). To better control the delays
        // associated with this preparation one can use _videoPlayer.Prepare() along with
        // its prepareCompleted event.
        _videoPlayer.Play();
    }
}
