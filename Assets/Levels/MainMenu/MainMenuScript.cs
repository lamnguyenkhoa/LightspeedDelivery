using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public GameObject creditTextObj;
    public GameObject[] levelButtons;
    private bool creditIsActive = false;
    private bool levelSelectIsActive = false;

    public void StartGameButton()
    {
        if (!levelSelectIsActive)
        {
            levelSelectIsActive = true;
            foreach (GameObject levelButton in levelButtons)
            {
                levelButton.SetActive(true);
            }
        }
        else
        {
            levelSelectIsActive = false;
            foreach (GameObject levelButton in levelButtons)
            {
                levelButton.SetActive(false);
            }
        }

        if (creditIsActive)
        {
            creditTextObj.SetActive(false);
            creditIsActive = false;
        }
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

        if (levelSelectIsActive)
        {
            levelSelectIsActive = false;
            foreach (GameObject levelButton in levelButtons)
            {
                levelButton.SetActive(false);
            }
        }
    }

    public void TutorialLevelButton()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void CityLevelButton()
    {
        SceneManager.LoadScene("CityTest");
    }

    public void FreeplayLevelButton()
    {
        SceneManager.LoadScene("Freeplay");
    }

    public void ExitGameButton()
    {
        Application.Quit();
    }
}