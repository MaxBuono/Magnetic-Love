using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public void closeDoor()
    {
        gameObject.SetActive(true);
    }

    public void openDoor()
    {
        //StartCoroutine(openCoroutine(transform,new Vector3(1f, 0.1f, 1f), 0.2f));
        gameObject.SetActive(false);
    }

    IEnumerator closeCoroutine()
    {
        yield return 0;
    }

    IEnumerator openCoroutine(Transform transform, Vector3 upScale, float duration)
    {
        Vector3 initialScale = transform.localScale;
 
        for(float time = 0 ; time < duration * 2 ; time += Time.deltaTime)
        {
            float progress = Mathf.PingPong(time, duration) / duration;
            transform.localScale = Vector3.Lerp(initialScale, upScale, progress);
            yield return null;
        }
        transform.localScale = initialScale;
    }

}
