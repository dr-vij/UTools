using System;
using UnityEngine;
using UTools.Input;

public class InputTest : MonoBehaviour
{
    private IDisposable m_DragSubscription;
    private InteractionObjectBase m_CameraInteractionObjectBase;
    private bool m_IgnoreHandled;

    private void Awake()
    {
        InputManager.Instance.RegisterCamera(Camera.main);
        InputManager.Instance.CameraTracer.LayerSettings.SetLayers("Default");
        m_CameraInteractionObjectBase = Camera.main.GetComponent<InteractionObjectBase>();

        m_CameraInteractionObjectBase.SubscribePointerPressEvent(OnPress);
        Resubscribe();
    }

    private void Resubscribe()
    {
        if (m_DragSubscription != null)
            m_DragSubscription.Dispose();

        m_DragSubscription = m_CameraInteractionObjectBase.SubscribePointerDragEvent(OnDragStart, OnDrag, OnDragEnd, true, m_IgnoreHandled);
    }

    private void OnPress(object sender, PointerInteractionEventArgs args)
    {
        m_IgnoreHandled = !m_IgnoreHandled;
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
