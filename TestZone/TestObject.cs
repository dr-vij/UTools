using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViJTools;

[RequireComponent(typeof(InteractionObject))]
public class TestObject : MonoBehaviour
{
    [SerializeField] private bool mSubscribeDrag = true;
    [SerializeField] private bool mSubscribePress = true;
    [SerializeField] private bool mSubscribeMove = true;
    [SerializeField] private bool mSubscribeGrab = true;


    private InteractionObject mInteractionObj;
    private Renderer mRenderer;
    private Vector3 mInitialScale;

    private Plane mPlane;
    private Vector3 mInitialPoint;
    private Vector3 mInitialPosition;
    private bool mIsGrabbed = false;

    private void Awake()
    {
        mInteractionObj = GetComponent<InteractionObject>();
        mInitialScale = transform.localScale;

        if (mSubscribeDrag)
            mInteractionObj.SubscribePointerDragEvent(OnDragStart, OnDrag, OnDragEnd);

        if (mSubscribeGrab)
            mInteractionObj.SubscribePointerGrabEvent(OnGrabStart, OnGrabEnd);

        if (mSubscribePress)
            mInteractionObj.SubscribePointerPressEvent(OnPress);

        if (mSubscribeMove)
            mInteractionObj.SubscribePointerMoveEvent(OnPointerMove);

        mRenderer = GetComponent<Renderer>();
    }

    private void OnPointerMove(object sender, PointerInteractionEventArgs args)
    {
        if (!mIsGrabbed)
        {
            var sph = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var pos = (Vector3)args.Position;
            pos.z = 5;
            sph.transform.position = Camera.main.ScreenToWorldPoint(pos);
            sph.transform.localScale = Vector3.one * 0.05f;
        }
    }

    private void OnPress(object sender, PointerInteractionEventArgs args)
    {
        mRenderer.material.color = Random.ColorHSV();
    }

    private void OnDragStart(object sender, PointerDragInteractionEventArgs args)
    {
        mInitialPosition = transform.position;
        var ray = Camera.main.ScreenPointToRay(args.PrevPosition);
        var colliders = GetComponentsInChildren<Collider>();
        var distance = float.PositiveInfinity;
        foreach (var collider in colliders)
        {
            if (collider.Raycast(ray, out var hit, float.PositiveInfinity))
            {
                if (hit.distance < distance)
                {
                    distance = hit.distance;
                    mInitialPoint = hit.point;
                    mPlane = new Plane(-Camera.main.transform.forward, hit.point);
                }
            }
        }
    }

    private void OnDrag(object sender, PointerDragInteractionEventArgs args)
    {
        var ray = Camera.main.ScreenPointToRay(args.Position);
        if (mPlane.Raycast(ray, out var enter))
        {
            var newPoint = ray.GetPoint(enter);
            var delta = newPoint - mInitialPoint;
            transform.position = mInitialPosition + delta;
        }
    }

    private void OnDragEnd(object sender, PointerDragInteractionEventArgs args)
    {
        var ray = Camera.main.ScreenPointToRay(args.Position);
        if (mPlane.Raycast(ray, out var enter))
        {
            var newPoint = ray.GetPoint(enter);
            var delta = newPoint - mInitialPoint;
            transform.position = mInitialPosition + delta;
        }
    }

    private void OnGrabStart(object sender, PointerInteractionEventArgs args)
    {
        mIsGrabbed = true;
        transform.localScale = mInitialScale * 1.2f;
    }

    private void OnGrabEnd(object sender, PointerInteractionEventArgs args)
    {
        mIsGrabbed = false;
        transform.localScale = mInitialScale;
    }
}
