using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public void startGame()
    {
        SceneManager.LoadScene(1);
    }

    public void mainMenu()
    {
        Debug.Log("masok");
        SceneManager.LoadScene(0);
    }
}
