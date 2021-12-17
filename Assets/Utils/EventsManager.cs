using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Utils/EventsManager")]
public class EventsManager : ScriptableObject
{
    public event Action OnPlayerDashStarted;

    public void PlayerDashStarted()
    {
        if (OnPlayerDashStarted != null) OnPlayerDashStarted();
    }

    public event Action OnPlayerDashEnded;

    public void PlayerDashEnded()
    {
        if (OnPlayerDashEnded != null) OnPlayerDashEnded();
    }

    public event Action OnOrderReceived;

    public void OrderReceived()
    {
        if (OnOrderReceived != null) OnOrderReceived();
    }

    public event Action OnFoodDelivered;

    public void FoodDelivered()
    {
        if (OnFoodDelivered != null) OnFoodDelivered();
    }

    public event Action OnPlayerDied;

    public void PlayerDied()
    {
        if (OnPlayerDied != null) OnPlayerDied();
    }

    public event Action OnGamePaused;

    public void GamePaused()
    {
        if (OnGamePaused != null) OnGamePaused();
    }

    public event Action OnGameUnpaused;

    public void GameUnpaused()
    {
        if (OnGameUnpaused != null) OnGameUnpaused();
    }
}