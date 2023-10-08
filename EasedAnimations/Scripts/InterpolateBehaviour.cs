using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;
using static Easings;

public class InterpolateBehaviour : PlayableBehaviour
{
    private EasingType m_EasingType = EasingType.Linear;

    public EasingType EasingType
    {
        get => m_EasingType;
        set => m_EasingType = value;
    }

    public float EasedProgress { get; private set; }
    
    public float TimeProgress { get; private set; }

    public event Action<float> OnEasedProgressUpdateEvent;

    public event Action OnAnimationFinishedEvent;

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        base.PrepareFrame(playable, info);
        var time = playable.GetTime();
        var duration = playable.GetDuration();
        TimeProgress = math.clamp((float)(time / duration), 0f, 1f);
        EasedProgress = Interpolate(TimeProgress, m_EasingType);
        OnEasedProgressUpdateEvent?.Invoke(EasedProgress);

        if (time >= duration)
        {
            playable.Pause();
            OnAnimationFinishedEvent?.Invoke();
        }
    }
}