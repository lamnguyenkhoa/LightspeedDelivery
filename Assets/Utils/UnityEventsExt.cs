using UnityEngine;
using UnityEngine.Events;

[System.Serializable] public class UnityFloatEvent : UnityEvent<float> {}
[System.Serializable] public class UnityIntEvent : UnityEvent<int> {}
[System.Serializable] public class UnityObjectEvent : UnityEvent<GameObject> {}
[System.Serializable] public class UnityBoolEvent : UnityEvent<bool> {}
[System.Serializable] public class UnityCollision2DEvent : UnityEvent<Collision2D> {}
[System.Serializable] public class UnityStringEvent : UnityEvent<string> {}
