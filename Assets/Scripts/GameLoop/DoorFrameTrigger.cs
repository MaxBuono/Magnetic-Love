using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorFrameTrigger : MonoBehaviour
{
    public GameLoopSO gameLoop;
    public LayerMask layerMask;
    public GameObject resetter;
    private void OnTriggerEnter2D(Collider2D other)
    {  
        bool _stopLevel = false;
        if (gameLoop.levelRunning)
        {
            if (other.CompareTag("Player"))
            {
                int layerMaskInt = (int) Mathf.Log(layerMask.value, 2);
                if (layerMaskInt == other.gameObject.layer)
                    _stopLevel = true;   
                
                if (_stopLevel)
                {
                    Debug.Log("LEVEL COMPLETED");
                    gameLoop.levelRunning = false;
                    resetter.GetComponent<ResetScene>().resetDefaultScene();
                }
                
            }    
        }
    }
}
