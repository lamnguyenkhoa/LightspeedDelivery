using UnityEngine;
using UnityEditor;

public class GLTFAnimLoop
{
    [MenuItem ("Assets/Loop = true")]
    static void LoopTrue () 
    {
        var clip = Selection.activeObject as AnimationClip;
        if (clip == null)
        {
            Debug.LogError("Select an animation clip to use this function.");
            return;
        }
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
    }

    [MenuItem ("Assets/Loop = false")]
    static void LoopFalse () 
    {
        var clip = Selection.activeObject as AnimationClip;
        if (clip == null)
        {
            Debug.LogError("Select an animation clip to use this function.");
            return;
        }
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

    }
}
