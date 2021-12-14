using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Utils/EventsMaganer")]
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
}
