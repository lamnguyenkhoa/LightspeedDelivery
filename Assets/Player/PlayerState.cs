using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour, IState
{
    [HideInInspector] public Player player;
    
    private void Awake() 
    {
        player = GetComponent<Player>();    
    }
    
    public void _Enter()
    {
        
    }

    public void _Enter<T>(T msg)
    {
        
    }

    public void _Exit()
    {
        
    }

    public void _FixedUpdate()
    {
        
    }

    public void _LateUpdate()
    {
        
    }

    public void _Update()
    {
        
    }
}
