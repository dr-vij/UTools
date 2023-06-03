using UnityEngine;

public class Vector3Animation : GenericAnimationBase<Vector3>
{
    public Vector3Animation() : base(Vector3.Lerp)
    {
    }
}