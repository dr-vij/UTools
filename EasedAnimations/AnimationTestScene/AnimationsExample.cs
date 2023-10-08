using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UTools;

public class AnimationsExample : MonoBehaviour
{
    private readonly List<Vector3Animation> m_FloatAnimations = new ();

    [SerializeField] private Easings.EasingType m_EasingType;
    [SerializeField] private Material m_Material;

    private void Start()
    {
        var enums = (Easings.EasingType[])Enum.GetValues(typeof(Easings.EasingType));

        int rowCount = 3;
        int counter = 0;
        foreach (var enumType in enums)
        {
            int innerCounter = 0;
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.GetComponent<MeshRenderer>().sharedMaterial = m_Material;
            var basePosition = new Vector3(counter % rowCount, counter / rowCount, 0) * 2;
            
            var anim = new Vector3Animation();
            anim.EasingType = enumType;
            anim.PlayAnimation(basePosition, basePosition + Vector3.up);
            anim.OnAnimationFinishedEvent += () =>
            {
                innerCounter++;
                if (innerCounter % 2 == 1)
                    anim.PlayAnimation(basePosition);
                else
                    anim.PlayAnimation(basePosition + Vector3.up);
            };
            counter++;
            
            anim.OnAnimationUpdateEvent += vector =>
            {
                go.transform.position = vector;
                // Debug.Log("Update");
            };

            anim.Time = 0.5f;
            
            m_FloatAnimations.Add(anim);
        }
    }

    private void OnDestroy()
    {
        foreach (var anim in m_FloatAnimations)
            anim.Dispose();
    }
}