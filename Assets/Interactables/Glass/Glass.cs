using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glass : MonoBehaviour
{
    public EventsManager eventsManager;
    BoxCollider boxCollider;

    private void Awake() {
        boxCollider = GetComponent<BoxCollider>();
    }

    private void OnEnable() {
        eventsManager.OnPlayerDashStarted += DisableCollision;
        eventsManager.OnPlayerDashEnded += EnableCollision;
    }

    private void OnDisable() {
        eventsManager.OnPlayerDashStarted -= DisableCollision;
        eventsManager.OnPlayerDashEnded -= EnableCollision;
    }

    void DisableCollision()
    {
        boxCollider.enabled = false;
    }

    void EnableCollision()
    {
        boxCollider.enabled = true;
    }
}
