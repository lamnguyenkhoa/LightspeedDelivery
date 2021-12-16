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
    public Slider shootForceSlider;

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
        SetStamina(playerStats.stamina);
        SetPower(playerStats.power);

        SetDeliveredText();
        SetFoodbagText(playerStats.foodbags);

        SetShootForce(0);
        
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

        playerStats.OnShootForceChanged += SetShootForce;
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

        playerStats.OnShootForceChanged -= SetShootForce;
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

    void SetShootForce(float amount)
    {
        shootForceSlider.value = amount / playerStats.maxShootForce;
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
        eventsManager.GamePaused();
    }

    public void Unpause()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        eventsManager.GameUnpaused();
    }

    public void GoBackToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("MainMenu");
    }
}