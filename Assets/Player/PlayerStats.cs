using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    public int orders = 3;
    // public int orders 
    // {
    //     get
    //     {
    //         return _orders;
    //     }
    //     set
    //     {
    //         _orders = value;
    //         OrdersChanged(_orders);
    //     }
    // }

    // public event Action<int> OnOrdersChanged;
    // public void OrdersChanged(int amount)
    // {
    //     if (OnOrdersChanged != null) OnOrdersChanged(amount);
    // }

    public int deliveredAmount = 0;
    // public int deliveredAmount
    // {
    //     get
    //     {
    //         return _deliveredAmount;
    //     }
    //     set
    //     {
    //         _deliveredAmount = Mathf.Clamp(value, 0, _orders);
    //         DeliveredAmountChanged(_deliveredAmount);
    //     }
    // }

    // public event Action<int> OnDeliveredAmountChanged;
    // public void DeliveredAmountChanged(int amount)
    // {
    //     if (OnDeliveredAmountChanged != null) OnDeliveredAmountChanged(amount);
    // }

    int _foodbags = 0;
    public int foodbags
    {
        get
        {
            return _foodbags;
        }
        set
        {
            _foodbags = Mathf.Clamp(value, 0, orders);
            FoodbagsChanged(_foodbags);
        }
    }

    public event Action<int> OnFoodbagsChanged;
    public void FoodbagsChanged(int score)
    {
        if (OnFoodbagsChanged != null) OnFoodbagsChanged(score);
    }

    public bool restorePower = false;
    public float powerRecoverRate = 150f;
    public float maxPower = 100;
    float _power = 0;
    public float power 
    {
        get
        {
            return _power;
        }
        set
        {
            _power = Mathf.Clamp(value, 0, maxPower);
            PowerChanged(_power);
        }
    }

    public event Action<float> OnPowerChanged;
    public void PowerChanged(float amount)
    {
        if (OnPowerChanged != null) OnPowerChanged(amount);
    }

    public float maxStamina = 100;
    float _stamina = 100;
    public float stamina 
    {
        get
        {
            return _stamina;
        }
        set
        {
            _stamina = Mathf.Clamp(value, 0, maxStamina);
            StaminaChanged(_stamina);
        }
    }

    public event Action<float> OnStaminaChanged;
    public void StaminaChanged(float amount)
    {
        if (OnStaminaChanged != null) OnStaminaChanged(amount);
    }

    public void IncreaseDeliveredAmount()
    {
        deliveredAmount += 1;
    }

    public void ResetStats()
    {
        orders = 3;
        deliveredAmount = 0;
        this.stamina = maxStamina;
        this.power = 0;
    }

    public void SetPowerRestore(bool state)
    {
        restorePower = state;
    }
}
