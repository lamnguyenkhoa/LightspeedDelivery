using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiniteStateMachine : MonoBehaviour
{
    [HideInInspector] public IState currentState;
    public string currentStateName;
    bool loaded = false;

    private void OnEnable() {
        if (!loaded) return;
        currentState = GetComponent<IState>();
        currentState._Enter();
        currentStateName = currentState.GetType().Name;
    }

    private void Start() {
        currentState = GetComponent<IState>();
        currentState._Enter();
        currentStateName = currentState.GetType().Name;
        loaded = true;
    }

    private void Update()
    {
        currentState._Update();
    }

    private void FixedUpdate()
    {
        currentState._FixedUpdate();
    }

    private void LateUpdate()
    {
        currentState._LateUpdate();
    }

    public void TransitionTo<T>() 
    {
        currentState._Exit();
        currentState = GetComponent<T>() as IState;
        currentState._Enter();
        currentStateName = currentState.GetType().Name;
    }

    public void TransitionTo<T, U>(U msg) 
    {
        currentState._Exit();
        currentState = GetComponent<T>() as IState;
        currentState._Enter(msg);
        currentStateName = currentState.GetType().Name;
    }
}
