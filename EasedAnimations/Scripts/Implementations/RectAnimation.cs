using UnityEngine;

public class RectAnimation : GenericAnimationBase<Rect>
{
    private static Rect Lerp(Rect start, Rect end, float progress)
    {
        return new Rect(
            Vector2.Lerp(start.position, end.position, progress),
            Vector2.Lerp(start.size, end.size, progress));
    }

    public RectAnimation() : base(Lerp)
    {
    }
}