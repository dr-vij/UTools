using Unity.Mathematics;

public class FloatAnimation : GenericAnimationBase<float>
{
    public FloatAnimation() : base(math.lerp)
    {
    }
}