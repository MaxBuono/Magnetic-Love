using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ClipBank
{
    public List<AudioClip> clips = new List<AudioClip>();
}

[CreateAssetMenu(fileName = "New Audio Collection")]
public class AudioCollection : ScriptableObject
{
    // Inspector Assigned
    [SerializeField] private string _mixerGroupName = string.Empty;
    [SerializeField] [Range(0.0f, 1.0f)] private float _volume = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] private float _spatialBlend = 1.0f;
    [SerializeField] [Range(0, 256)] private int _priority = 128;
    [SerializeField] private List<ClipBank> _clipBanks = new List<ClipBank>();

    // Properties
    public string MixerGroupName { get { return _mixerGroupName; } }
    public float Volume { get { return _volume; } }
    public float SpatialBlend { get { return _spatialBlend; } }
    public int Priority { get { return _priority; } }
    public int BankCount { get { return _clipBanks.Count; } }

    // Allow to get a clipBank as we are accessing an array
    // and then get a random clip from that bank
    public AudioClip this[int i]
    {
        get
        {
            if (_clipBanks == null || BankCount <= i) return null;
            if (_clipBanks[i].clips.Count == 0) return null;

            List<AudioClip> clipList = _clipBanks[i].clips;
            return clipList[Random.Range(0, clipList.Count)];
        }
    }

    // Same as above but always get the first bank
    public AudioClip AudioClip
    {
        get
        {
            if (_clipBanks == null || BankCount == 0) return null;
            if (_clipBanks[0].clips.Count == 0) return null;

            List<AudioClip> clipList = _clipBanks[0].clips;
            return clipList[Random.Range(0, clipList.Count)];
        }
    }
}
