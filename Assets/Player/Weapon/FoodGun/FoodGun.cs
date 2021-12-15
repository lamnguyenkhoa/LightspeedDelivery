using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FoodGun : MonoBehaviour
{
    public MainInstances mainInstances;
    public PlayerStats playerStats;
    public GameObject foodGun;
    public FoodBag foodBag;
    bool isCharging = false;

    public float minShootForce = 5;
    public float forceRate = 20;

    GameControls gameControls;

    private void OnEnable() {
        gameControls.Enable();
        ActivateGun();
    }

    private void OnDisable() {
        DeactivateGun();
        gameControls.Disable();
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

    private void Awake() {
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

    void Shoot()
    {
        AudioManager.instance.PlayFoodSplat();
        FoodBag newFoodBag = Instantiate(foodBag, foodGun.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
        newFoodBag.shootDirection = mainInstances.mainCamera.transform.forward;
        newFoodBag.shootForce = playerStats.shootForce;
        // not all momentum yet, only player "active" momentum
        newFoodBag.bonusFromPlayerMomentum = mainInstances.player.GetComponent<PlayerMotion>().finalMove;
        playerStats.foodbags -= 1;
    }

    void ShootPerformed(InputAction.CallbackContext ctx)
    {
        if (playerStats.foodbags > 0) SetCharging();
    }

    void ShootCanceled(InputAction.CallbackContext ctx)
    {
        if (playerStats.foodbags > 0) Shoot();
        UnsetCharging();
    }

    void SetCharging()
    {
        isCharging = true;
    }

    void UnsetCharging()
    {
        playerStats.shootForce = 0;
        isCharging = false;
    }
}
