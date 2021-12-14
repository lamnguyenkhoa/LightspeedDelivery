using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Utils/GameStats")]
public class GameStats : ScriptableObject
{
    public int foodOrders = 0;
    public int foodDelivered = 0;
}
