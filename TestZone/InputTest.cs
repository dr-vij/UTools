using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ViJTools;

public class InputTest : MonoBehaviour
{
    private IDisposable mDragSubscribtion;
    private InteractionObject mCameraInteractionObject;
    private bool mIgnoreHandled = false;

    private void Awake()
    {
        InputManager.Instance.RegisterCamera(Camera.main);
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

    private void OnPress(object sender, PointerInteractionEventArgs args)
    {
        mIgnoreHandled = !mIgnoreHandled;
        Resubscribe();
    }

    private void OnDragStart(object sender, PointerDragInteractionEventArgs args)
    {
        transform.position += (Vector3)(args.Position - args.PrevPosition) / 100;
    }

    private void OnDrag(object sender, PointerDragInteractionEventArgs args)
    {
        transform.position += (Vector3)(args.Position - args.PrevPosition) / 100;
    }

    private void OnDragEnd(object sender, PointerDragInteractionEventArgs args)
    {
        transform.position += (Vector3)(args.Position - args.PrevPosition) / 100;
    }
}
