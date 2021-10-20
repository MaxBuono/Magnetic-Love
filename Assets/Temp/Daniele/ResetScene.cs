using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetScene : MonoBehaviour
{
    public GameLoopSO gameLoop;
    
    //Scene component
    public GameObject red;
    public GameObject blue;
    public GameObject redblue;
    public GameObject bluered;
    
    private int playerInd = 0;
    
    public void resetDefaultScene()
    {
        playerInd++;
        if (playerInd > 3)
            playerInd = 0;
        //RED 
        red.transform.position = new Vector3(-1.5f,-3.5f);
        red.SetActive(false);
        //BLUE
        blue.transform.position = new Vector3(0.5f, -3.5f);
        blue.SetActive(false);
        //REDBLUE
        redblue.transform.position = new Vector3(-5.5f,-3.5f);
        redblue.SetActive(false);
        //BLUERED
        bluered.SetActive(false);
        bluered.transform.position = new Vector3(-8.5f, -3.5f);
        switch (playerInd)
        {
            case 0:
                red.SetActive(true);
                break;
            case 1:
                blue.SetActive(true);
                break;
            case 2:
                redblue.SetActive(true);
                break;
            case 3:
                bluered.SetActive(true);
                break;
        }
        
        gameLoop.levelRunning = true;
    }
}
