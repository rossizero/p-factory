using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAdministration : MonoBehaviour
{


    /// <summary>
    /// Quits the game
    /// </summary>
    public void QuitTheGame()
    {
        Debug.LogError("Quitting Game...");
        Application.Quit();
    }

    /// <summary>
    /// Loads a specific game
    /// </summary>
    /// <param name="filepath">path to the savegame file</param>
    public void LoadGame(string filepath)
    {

    }

    /// <summary>
    /// Saves the game
    /// </summary>
    /// <param name="filepath">path to the savegame file</param>
    public void saveGame(string filepath)
    {

    }


}
