using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InGame : MonoBehaviour
{
    public EventsManager eventsManager;
    public PlayerStats playerStats;
    public TextMeshProUGUI deliveredText;
    public TextMeshProUGUI foodBagLeftText;
    public Slider staminaSlider;
    public Slider powerSlider;

    public GameObject pauseMenu;
    bool isPaused = false;
    GameControls gameControls;

    private void Awake() 
    {
        staminaSlider.maxValue = playerStats.maxStamina;
        powerSlider.maxValue = playerStats.maxPower;
        gameControls = new GameControls();
    }

    private void Start() 
    {
        Unpause();
    }

    private void OnEnable() 
    {
        gameControls.Enable();
        gameControls.Menu.Pause.performed += PausePerformed;

        playerStats.OnStaminaChanged += SetStamina;
        playerStats.OnPowerChanged += SetPower;
        playerStats.OnFoodbagsChanged += SetFoodbagText;

        eventsManager.OnFoodDelivered += SetDeliveredText;
        eventsManager.OnOrderReceived += SetDeliveredText;
    }

    private void OnDisable() 
    {
        gameControls.Disable();
        gameControls.Menu.Pause.performed -= PausePerformed;

        playerStats.OnStaminaChanged -= SetStamina;
        playerStats.OnPowerChanged -= SetPower;
        playerStats.OnFoodbagsChanged -= SetFoodbagText;

        eventsManager.OnFoodDelivered -= SetDeliveredText;
        eventsManager.OnOrderReceived -= SetDeliveredText;
    }

    void SetDeliveredText()
    {
        deliveredText.text = "Delivered: " + playerStats.deliveredAmount.ToString() + "/" + playerStats.orders.ToString();
    }

    void SetFoodbagText(int amount)
    {
        foodBagLeftText.text = "Food bags lefg: " + amount.ToString();
    }

    void SetStamina(float amount)
    {
        staminaSlider.value = amount;
    }

    void SetPower(float amount)
    {
        powerSlider.value = amount;
    }

    void PausePerformed(InputAction.CallbackContext ctx)
    {
        if (!isPaused)
        {
            Pause();
        }
        else
        {
            Unpause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Unpause()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void GoBackToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("MainMenu");
    }
}
