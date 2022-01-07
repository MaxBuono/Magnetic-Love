using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level Properties")]
public class LevelProperties : ScriptableObject
{
    // Music
    public AudioClip music;
    public float timeToGoToZero = 1.3f;
    public float waitTime = 0.2f;
    public float timeToGetBackToMax = 1.5f;

    // Hint
    [TextArea] public string hint = "I'm a useless hint!";
}
