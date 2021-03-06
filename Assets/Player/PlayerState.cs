using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour, IState
{
    [HideInInspector] public Player player;
    [HideInInspector] public FiniteStateMachine fsm;
    [HideInInspector] public MainInstances mainInstances;
    [HideInInspector] public EventsManager eventsManager;
    [HideInInspector] public PlayerStats playerStats;
    [HideInInspector] public Camera mainCamera;
    [HideInInspector] public Transform headPos;
    [HideInInspector] public CharacterController controller;
    [HideInInspector] public GameControls gameControls;
    [HideInInspector] public FoodGun foodGun;
    [HideInInspector] public Animator anim;

    private void Start() 
    {
        player = GetComponent<Player>();
        
        mainInstances = player.mainInstances;
        eventsManager = player.eventsManager;
        playerStats = player.playerStats;
        fsm = player.fsm;

        mainCamera = player.mainCamera;
        headPos = player.headPos;
        controller = player.controller;
        gameControls = player.gameControls;

        foodGun = player.foodGun;
        anim = player.anim;
    }
    
    public virtual void _Enter()
    {
        
    }

    public virtual void _Enter<T>(T msg)
    {
        
    }

    public virtual void _Exit()
    {
        
    }

    public virtual void _FixedUpdate()
    {
        
    }

    public virtual void _LateUpdate()
    {
        
    }

    public virtual void _Update()
    {
        
    }
}
