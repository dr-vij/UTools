using UnityEngine;

public class ColorAnimation : GenericAnimationBase<Color>
{
    public ColorAnimation() : base(Color.Lerp)
    {
    }
}