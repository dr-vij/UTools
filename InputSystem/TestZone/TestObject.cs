using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UTools.Input;

[RequireComponent(typeof(InteractionObjectBase))]
public class TestObject : MonoBehaviour
{
    [SerializeField] private bool m_SubscribeDrag = true;
    [SerializeField] private bool m_SubscribePress = true;
    [SerializeField] private bool m_SubscribeMove = true;
    [SerializeField] private bool m_SubscribeGrab = true;
    [SerializeField] private bool m_SubscribeScale = true;

    private InteractionObjectBase m_InteractionObj;
    private Renderer m_Renderer;
    private Vector3 m_InitialScale;

    private Plane m_Plane;
    private Vector3 m_InitialPoint;
    private Vector3 m_InitialPosition;
    private bool m_IsGrabbed = false;

    private void Awake()
    {
        m_InteractionObj = GetComponent<InteractionObjectBase>();

        if (m_SubscribeDrag)
            m_InteractionObj.SubscribePointerDragEvent(OnDragStart, OnDrag, OnDragEnd);

        if (m_SubscribeGrab)
            m_InteractionObj.SubscribePointerGrabEvent(OnGrabStart, OnGrabEnd);

        if (m_SubscribePress)
            m_InteractionObj.SubscribePointerPressEvent(OnPress);

        if (m_SubscribeMove)
            m_InteractionObj.SubscribePointerMoveEvent(OnPointerMove);

        if (m_SubscribeScale)
            m_InteractionObj.SubscribeTwoPointersDragEnvent(OnTwoPointersDragStart, OnTwoPointersDrag, OnTwoPointersDragEnd);

        m_Renderer = GetComponent<Renderer>();
    }

    private void OnPointerMove(object sender, PointerInteractionEventArgs args)
    {
        if (!m_IsGrabbed)
        {
            var sph = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var pos = (Vector3)args.PointerPosition;
            pos.z = 5;
            sph.transform.position = Camera.main.ScreenToWorldPoint(pos);
            sph.transform.localScale = Vector3.one * 0.05f;
        }
    }

    private void OnPress(object sender, PointerInteractionEventArgs args)
    {
        m_Renderer.material.color = Random.ColorHSV();
    }

    private void OnDragStart(object sender, PointerDragInteractionEventArgs args)
    {
        ProcessInitialHit(args.PointerPrevPosition);
        ProcessDrag(args.PointerPosition);
    }

    private void OnDrag(object sender, PointerDragInteractionEventArgs args)
    {
        ProcessDrag(args.PointerPosition);
    }

    private void OnDragEnd(object sender, PointerDragInteractionEventArgs args)
    {
        ProcessDrag(args.PointerPosition);
    }

    private void OnTwoPointersDragStart(object sender, TwoPointersDragInteractionEventArgs args)
    {
        ProcessInitialHit(args.PointerCenterPrev);
        ProcessDrag(args.PointersCenter);
        ProcessScale(args);
    }

    private void OnTwoPointersDrag(object sender, TwoPointersDragInteractionEventArgs args)
    {
        ProcessDrag(args.PointersCenter);
        ProcessScale(args);
    }

    private void OnTwoPointersDragEnd(object sender, TwoPointersDragInteractionEventArgs args)
    {
        ProcessDrag(args.PointersCenter);
        ProcessScale(args);
    }

    private void OnGrabStart(object sender, PointerInteractionEventArgs args)
    {
        m_WasScaled = false;
        m_InitialScale = transform.localScale;
        m_IsGrabbed = true;
        transform.localScale = m_InitialScale * 1.05f;
    }

    private void OnGrabEnd(object sender, PointerInteractionEventArgs args)
    {
        m_IsGrabbed = false;
        if (!m_WasScaled)
            transform.localScale = m_InitialScale;
    }

    private bool m_WasScaled = false;

    private void ProcessScale(TwoPointersDragInteractionEventArgs args)
    {
        //Object scale:
        m_WasScaled = true;
        var disBefore = Vector2.Distance(args.PointerPrevPosition, args.SecondaryPointerPosition);
        var after = Vector2.Distance(args.PointerPosition, args.SecondaryPointerPrevPosition);
        var ratio = after / disBefore;
        transform.localScale *= ratio;
    }

    //Help funcs
    private void ProcessInitialHit(Vector2 position2d)
    {
        m_InitialPosition = transform.position;
        var ray = Camera.main.ScreenPointToRay(position2d);
        var colliders = GetComponentsInChildren<Collider>();
        var distance = float.PositiveInfinity;
        foreach (var collider in colliders)
        {
            if (collider.Raycast(ray, out var hit, float.PositiveInfinity))
            {
                if (hit.distance < distance)
                {
                    distance = hit.distance;
                    m_InitialPoint = hit.point;
                    m_Plane = new Plane(-Camera.main.transform.forward, hit.point);
                }
            }
        }
    }

    private void ProcessDrag(Vector2 newPosition2d)
    {
        var ray = Camera.main.ScreenPointToRay(newPosition2d);
        if (m_Plane.Raycast(ray, out var enter))
        {
            var newPoint = ray.GetPoint(enter);
            var delta = newPoint - m_InitialPoint;
            transform.position = m_InitialPosition + delta;
        }
    }
}
