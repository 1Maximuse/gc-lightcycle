using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Environment : MonoBehaviour
{
    public bool isTraining;

    [SerializeField]
    private PlayerAgent yellowAgent;
    [SerializeField]
    private PlayerAgent blueAgent;

    public void endAll(string winner)
    {
        yellowAgent.EndEpisode();
        blueAgent.EndEpisode();

        foreach (GameObject wall in GameObject.FindObjectsOfType<GameObject>())
        {
            if (wall.name == "Wall(Clone)" || wall.name == "BlueWall(Clone)" || wall.name == "Crash(Clone)")
            {
                Destroy(wall);
            }
        }

        if (!isTraining) SceneManager.LoadScene(winner == "yellow" ? 2 : 3);
    }

    public void addBlueReward()
    {
        blueAgent.AddReward(+500f);
    }

    public void addYellowReward()
    {
        yellowAgent.AddReward(+500f);
    }

    public void yellowCrash()
    {
        endAll("blue");
    }
    public void blueCrash()
    {
        endAll("yellow");
    }
}
