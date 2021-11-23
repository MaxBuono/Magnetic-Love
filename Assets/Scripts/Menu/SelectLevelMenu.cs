using MenuManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectLevelMenu : MonoBehaviour
{
    public void OnBackPressed()
    {
        print("LEVEL SELECT");
        MainMenu.Open();
    }
    
}
