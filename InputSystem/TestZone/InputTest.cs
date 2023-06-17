using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UTools.Input;

public class InputTest : MonoBehaviour
{
    private IDisposable m_DragSubscription;
    private InteractionObjectBase m_CameraInteractionObjectBase;
    private bool m_IgnoreHandled;

    [SerializeField] private Transform m_CanvasRoot;
    [SerializeField] private Transform m_PointerPreviewPrefab;

    private List<Transform> m_GesturePointers = new();

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

    private void Update()
    {
        // var gestures = InputManager.Instance.ActiveGestures;
        // var pointerGestureAnalyzers = gestures as PointerGestureAnalyzer[] ?? gestures.ToArray();
        // var pointersCount = pointerGestureAnalyzers.Sum(c => c.PointersCount);
        //
        // while (m_GesturePointers.Count > pointersCount)
        // {
        //     var pointerPreview = m_GesturePointers[^1];
        //     Destroy(pointerPreview.gameObject);
        //     m_GesturePointers.RemoveAt(m_GesturePointers.Count - 1);
        // }
        //
        // while(m_GesturePointers.Count < pointersCount)
        // {
        //     m_GesturePointers.Add(Instantiate(m_PointerPreviewPrefab, m_CanvasRoot));
        // }
        //
        // var counter = 0;
        // foreach (var gesture in pointerGestureAnalyzers)
        // {
        //     var pointers = gesture.Pointers;
        //
        //     foreach (var pointer in pointers)
        //     {
        //         m_GesturePointers[counter++].position = pointer.CurrentPosition;
        //     }
        // }
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
