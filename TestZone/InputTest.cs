using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityTools;

public class InputTest : MonoBehaviour
{
    private IDisposable mDragSubscribtion;
    private InteractionObject mCameraInteractionObject;
    private bool mIgnoreHandled = false;

    [SerializeField] private Transform mCanvasRoot = default;
    [SerializeField] private Transform mPointerPreviewPrefab = default;

    private List<Transform> mGesturePointers = new List<Transform>();

    private void Awake()
    {
        InputManager.Instance.RegisterCamera(Camera.main);
        InputManager.Instance.CameraTracer.LayerSettings.SetLayers("Default");
        mCameraInteractionObject = Camera.main.GetComponent<InteractionObject>();

        mCameraInteractionObject.SubscribePointerPressEvent(OnPress);
        Resubscribe();
    }

    private void Resubscribe()
    {
        if (mDragSubscribtion != null)
            mDragSubscribtion.Dispose();

        mDragSubscribtion = mCameraInteractionObject.SubscribePointerDragEvent(OnDragStart, OnDrag, OnDragEnd, true, mIgnoreHandled);
    }

    private void Update()
    {
        var gestures = InputManager.Instance.ActiveGestures;
        var pointersCount = gestures.Sum(c => c.PointersCount);

        while (mGesturePointers.Count > pointersCount)
        {
            var pointerPreview = mGesturePointers[mGesturePointers.Count - 1];
            Destroy(pointerPreview.gameObject);
            mGesturePointers.RemoveAt(mGesturePointers.Count - 1);
        }

        while(mGesturePointers.Count < pointersCount)
        {
            mGesturePointers.Add(Instantiate(mPointerPreviewPrefab, mCanvasRoot));
        }

        var counter = 0;
        foreach (var gesture in gestures)
        {
            var pointers = gesture.Pointers;

            foreach (var pointer in pointers)
            {
                mGesturePointers[counter++].position = pointer.CurrentPosition;
            }
        }
    }

    private void OnPress(object sender, PointerInteractionEventArgs args)
    {
        mIgnoreHandled = !mIgnoreHandled;
        Resubscribe();
    }

    private void OnDragStart(object sender, PointerDragInteractionEventArgs args)
    {
        transform.position += (Vector3)(args.PointerPosition - args.PointerPrevPosition) / 100;
    }

    private void OnDrag(object sender, PointerDragInteractionEventArgs args)
    {
        transform.position += (Vector3)(args.PointerPosition - args.PointerPrevPosition) / 100;
    }

    private void OnDragEnd(object sender, PointerDragInteractionEventArgs args)
    {
        transform.position += (Vector3)(args.PointerPosition - args.PointerPrevPosition) / 100;
    }
}
