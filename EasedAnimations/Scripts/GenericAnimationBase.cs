using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using static Easings;

public class IdContainer
{
    protected static int m_IdCounter;
    public int Id { get; protected set; }
    
    protected IdContainer()
    {
        Id = ++m_IdCounter;
    }
}

/// <summary>
/// The primary idea of this animation behaviour is that we never set the value directly, but instead we set the
/// start and end values and the behaviour will interpolate between them.
/// </summary>
/// <typeparam name="T"></typeparam>
public class GenericAnimationBase<T> : IdContainer, IDisposable
{
    private PlayableGraph m_PlayableGraph;
    private readonly ScriptPlayable<InterpolateBehaviour> m_InterpolatePlayable;
    private readonly InterpolateBehaviour m_PlayableBehaviour;

    private readonly GameObject m_Go;
    private readonly Func<T, T, float, T> m_LerpFunction;

    private T m_StartValue;
    private T m_EndValue;
    private T m_CurrentValue;

    public event Action OnAnimationFinishedEvent
    {
        add => m_PlayableBehaviour.OnAnimationFinishedEvent += value;
        remove => m_PlayableBehaviour.OnAnimationFinishedEvent -= value;
    }

    public event Action<T> OnAnimationUpdateEvent;

    public T StartValue
    {
        get => m_StartValue;
        set
        {
            m_StartValue = value;
            UpdateCurrentValue(m_PlayableBehaviour.EasedProgress);
        }
    }

    public T EndValue
    {
        get => m_EndValue;
        set
        {
            m_EndValue = value;
            UpdateCurrentValue(m_PlayableBehaviour.EasedProgress);
        }
    }

    public T CurrentValue
    {
        get => m_CurrentValue;
        private set
        {
            m_CurrentValue = value;
            OnAnimationUpdateEvent?.Invoke(m_CurrentValue);
        }
    }

    public EasingType EasingType
    {
        get => m_PlayableBehaviour.EasingType;
        set => m_PlayableBehaviour.EasingType = value;
    }

    public bool IsPlaying => m_InterpolatePlayable.GetPlayState() == PlayState.Playing;

    public float Duration
    {
        get => (float)m_InterpolatePlayable.GetDuration();
        set
        {
            //save initial state
            var initialState = m_InterpolatePlayable.GetPlayState();
            if (initialState == PlayState.Paused)
                m_InterpolatePlayable.Play();

            //set data
            m_InterpolatePlayable.SetDuration(value);
            m_PlayableGraph.Evaluate();

            //restore initial state
            if (initialState == PlayState.Paused)
                m_InterpolatePlayable.Pause();
        }
    }

    public float TimeLeft
    {
        get => Duration - Time;
        set => Time = Duration - value;
    }

    public float Time
    {
        get => (float)m_InterpolatePlayable.GetTime();
        set
        {
            //save initial state
            var initialState = m_InterpolatePlayable.GetPlayState();
            if (initialState == PlayState.Paused)
                m_InterpolatePlayable.Play();

            //set data
            var clampedTime = math.clamp(value, 0, m_InterpolatePlayable.GetDuration());
            m_InterpolatePlayable.SetTime(clampedTime);
            m_PlayableGraph.Evaluate();

            //restore initial state
            if (initialState == PlayState.Paused)
                m_InterpolatePlayable.Pause();
        }
    }

    public GenericAnimationBase(Func<T, T, float, T> lerpFunction)
    {
        //Prepare the graph
        m_PlayableGraph = PlayableGraph.Create();
        m_PlayableGraph.SetTimeUpdateMode(DirectorUpdateMode.UnscaledGameTime);
        m_PlayableGraph.Play();

        //prepare the interpolator
        m_InterpolatePlayable = ScriptPlayable<InterpolateBehaviour>.Create(m_PlayableGraph);
        m_InterpolatePlayable.SetDuration(1);
        m_InterpolatePlayable.Pause();
        m_InterpolatePlayable.SetTime(0);
        m_PlayableBehaviour = m_InterpolatePlayable.GetBehaviour();
        m_LerpFunction = lerpFunction;
        m_PlayableBehaviour.OnEasedProgressUpdateEvent += UpdateCurrentValue;

        //Connect animation to graph
        m_Go = new GameObject($"Animator {Id}", typeof(Animator))
        {
            hideFlags = HideFlags.HideInHierarchy
        };
        var animator = m_Go.GetComponent<Animator>();
        var playableOutput = AnimationPlayableOutput.Create(m_PlayableGraph, $"FloatAnimation {Id}", animator);
        playableOutput.SetSourcePlayable(m_InterpolatePlayable);
    }

    public void SetStartEnd(T start, T end)
    {
        m_StartValue = start;
        m_EndValue = end;
        UpdateCurrentValue(m_PlayableBehaviour.EasedProgress);
    }

    public void Stop() => m_InterpolatePlayable.Pause();

    public void Play() => m_InterpolatePlayable.Play();

    public void PlayAnimation(T start, T end, float duration, EasingType easingType, float startFrom)
    {
        Time = startFrom;
        PlayAnimation(start, end, duration, easingType);
    }

    public void PlayAnimation(T start, T end, float duration, EasingType easingType)
    {
        EasingType = easingType;
        PlayAnimation(start, end, duration);
    }

    public void PlayAnimation(T start, T end, float duration)
    {
        Duration = duration;
        PlayAnimation(start, end);
    }

    public void PlayAnimation(T start, T end)
    {
        m_StartValue = start;
        m_EndValue = end;
        CurrentValue = start;
        m_InterpolatePlayable.SetTime(0);
        m_InterpolatePlayable.Play();
    }

    public void PlayAnimation(T end)
    {
        PlayAnimation(m_CurrentValue, end);
    }

    public void Dispose()
    {
        m_PlayableGraph.Destroy();
        UnityEngine.Object.Destroy(m_Go);
    }

    private void UpdateCurrentValue(float progress)
    {
        CurrentValue = m_LerpFunction(m_StartValue, m_EndValue, progress);
    }
}