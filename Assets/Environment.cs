using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    [SerializeField]
    private PlayerAgent[] yellowAgents;
    [SerializeField]
    private PlayerAgent[] blueAgents;
    private int yellowAlive;
    private int blueAlive;

    public void OnEnable()
    {
        yellowAlive = yellowAgents.Length;
        blueAlive = blueAgents.Length;
    }
    public void endAll()
    {
        foreach (PlayerAgent agents in yellowAgents)
        {
            agents.EndEpisode();
        }
        foreach (PlayerAgent agents in blueAgents)
        {
            agents.EndEpisode();
        }
        foreach (GameObject wall in GameObject.FindObjectsOfType<GameObject>())
        {
            if (wall.name == "Wall(Clone)" || wall.name == "BlueWall(Clone)" || wall.name == "Crash(Clone)")
            {
                Destroy(wall);
            }
        }
        yellowAlive = yellowAgents.Length;
        blueAlive = blueAgents.Length;
    }

    public void addBlueReward()
    {
        foreach (PlayerAgent agents in blueAgents)
        {
            agents.AddReward(+500f);
        }
    }

    public void addYellowReward()
    {
        foreach (PlayerAgent agents in yellowAgents)
        {
            agents.AddReward(+500f);
        }
    }

    public void yellowCrash()
    {
        yellowAlive--;
        if (yellowAlive == 0)
        {
            foreach (PlayerAgent agents in blueAgents)
            {
                agents.AddReward(+1000f);
            }
            endAll();
        }
    }
    public void blueCrash()
    {
        blueAlive--;
        if (blueAlive == 0)
        {
            foreach (PlayerAgent agents in yellowAgents)
            {
                agents.AddReward(+1000f);
            }
            endAll();
        }
    }
}
