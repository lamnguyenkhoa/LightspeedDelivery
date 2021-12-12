using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public void StartGameButton()
    {
        SceneManager.LoadScene("CityTest");
    }

    public void CreditButton()
    {
    }

    public void ExitGameButton()
    {
        Application.Quit();
    }
}