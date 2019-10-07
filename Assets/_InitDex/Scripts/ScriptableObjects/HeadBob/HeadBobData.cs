using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New HeadBob", menuName = "Camera/Headbob Data")]
public class HeadBobData : ScriptableObject
{
    public AnimationCurve headbobHorizontalCurve;
    public AnimationCurve headbobVerticalCurve;

    public float speedCurve;
    public float bobInterval;

    public float horizontalInfluence;
    public float verticalInfluence;

    public Vector3 DoHeadBob(float cycle)
    {
        float xHeadBobOffset = headbobHorizontalCurve.Evaluate(cycle) * horizontalInfluence;
        float yHeadBobOffset = headbobVerticalCurve.Evaluate(cycle) * verticalInfluence;

        return new Vector3(xHeadBobOffset, yHeadBobOffset, 0f);
    }
}
