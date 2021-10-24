using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Door is an abstract class that implements the generic door
 * His childs inherite the OnTriggerEnter2D method with the correct CharacterColor Enum 
 */

public abstract class Door : MonoBehaviour
{
    public CharColor color;
    public GameObject resetter;

    //This coroutine is an example of an animation that starts when the player reachs the correct door
    IEnumerator fadeAway(Transform player)
    {
        for (float i = player.localScale.x; i > 0.05f; i -= 0.01f)
        {
            player.localScale = new Vector3(i,i,1f);
            yield return null;
        }

        player.localScale = new Vector3(1, 1, 1);
        resetter.GetComponent<ResetScene>().resetDefaultScene();
    }

    //Protected virtual method that is inherited by Door childs
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<CharacterData>().color == color)
            {
                Debug.Log("Level completed");
                StartCoroutine(fadeAway(other.transform));
            }
        }
    }
    
}
