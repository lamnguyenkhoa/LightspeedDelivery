using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FoodGun : MonoBehaviour
{
    public EventsManager eventsManager;
    public MainInstances mainInstances;
    public PlayerStats playerStats;
    public GameObject foodGun;
    public FoodBag foodBag;
    private bool isCharging = false;

    public float minShootForce = 5;
    public float forceRate = 20;

    private GameControls gameControls;

    private void OnEnable()
    {
        UnpauseGun();
        eventsManager.OnGamePaused += PauseGun;
        eventsManager.OnGameUnpaused += UnpauseGun;
    }

    private void OnDisable()
    {
        eventsManager.OnGamePaused -= PauseGun;
        eventsManager.OnGameUnpaused -= UnpauseGun;
        PauseGun();
    }

    private void PauseGun()
    {
        DeactivateGun();
        gameControls.Disable();
    }

    private void UnpauseGun()
    {
        gameControls.Enable();
        ActivateGun();
    }

    public void ActivateGun()
    {
        gameControls.Player.Shoot.performed += ShootPerformed;
        gameControls.Player.Shoot.canceled += ShootCanceled;
    }

    public void DeactivateGun()
    {
        gameControls.Player.Shoot.performed -= ShootPerformed;
        gameControls.Player.Shoot.canceled -= ShootCanceled;
    }

    private void Awake()
    {
        gameControls = new GameControls();
        playerStats.foodbags = playerStats.orders;
    }

    private void Update()
    {
        if (isCharging)
        {
            playerStats.shootForce += forceRate * Time.deltaTime;
        }
    }

    private void Shoot()
    {
        AudioManager.instance.PlayFoodSplat();
        FoodBag newFoodBag = Instantiate(foodBag, foodGun.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
        newFoodBag.shootDirection = mainInstances.mainCamera.transform.forward;
        newFoodBag.shootForce = playerStats.shootForce;
        // not all momentum yet, only player "active" momentum
        newFoodBag.bonusFromPlayerMomentum = mainInstances.player.GetComponent<PlayerMotion>().finalMove;
        playerStats.foodbags -= 1;
    }

    private void ShootPerformed(InputAction.CallbackContext ctx)
    {
        if (playerStats.foodbags > 0) SetCharging();
    }

    private void ShootCanceled(InputAction.CallbackContext ctx)
    {
        if (playerStats.foodbags > 0 && playerStats.shootForce >= minShootForce)
        {
            Shoot();
            AudioManager.instance.PlayGunRepressurise();
        }
        UnsetCharging();
    }

    private void SetCharging()
    {
        isCharging = true;
    }

    private void UnsetCharging()
    {
        playerStats.shootForce = 0;
        isCharging = false;
    }
}