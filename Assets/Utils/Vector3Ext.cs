using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Ext
{
    public static Vector3 ClampPlaneAxis(Vector3 motionVector, float clampValue)
    {
        Vector2 motionSupport = new Vector2(motionVector.x, motionVector.z);
        motionSupport = Vector2.ClampMagnitude(motionSupport, clampValue);
        return new Vector3(motionSupport.x, motionVector.y, motionSupport.y);
    }
}
