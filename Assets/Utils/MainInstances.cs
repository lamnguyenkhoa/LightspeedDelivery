using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Utils/MainInstances")]
public class MainInstances : ScriptableObject
{
    [HideInInspector] public Player player;
    [HideInInspector] public Camera mainCamera;
}
