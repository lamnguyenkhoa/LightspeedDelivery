using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public GameObject creditTextObj;
    private bool creditIsActive = false;

    public void StartGameButton()
    {
        SceneManager.LoadScene("CityTest");
    }

    public void CreditButton()
    {
        if (!creditIsActive)
        {
            creditTextObj.SetActive(true);
            creditIsActive = true;
        }
        else
        {
            creditTextObj.SetActive(false);
            creditIsActive = false;
        }
    }

    public void ExitGameButton()
    {
        Application.Quit();
    }
}