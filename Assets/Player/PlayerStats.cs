using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    // int _deliveredAmount = 0;
    // public int deliveredAmount
    // {
    //     get
    //     {
    //         return _deliveredAmount;
    //     }
    //     set
    //     {
    //         _deliveredAmount = value;
    //     }
    // }

    // public UnityIntEvent OnDeliveredAmountChanged;
    // public void DeliveredAmountChanged(int score)
    // {
    //     OnDeliveredAmountChanged?.Invoke(score);
    // }

    public int deliveredAmount = 0;

    public void IncreaseDeliveredAmount()
    {
        deliveredAmount += 1;
    }
}
