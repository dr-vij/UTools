using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ViJTools;

public class InputTest : MonoBehaviour
{
    [SerializeField] private List<InteractionObject> mObjects = default;

    private void Awake()
    {
        InputManager.Instance.RegisterCamera(Camera.main);
        foreach (var obj in mObjects)
        {
            obj.SubscribePointerDragEvent(OnDragStart, OnDrag, OnDragEnd);
            obj.SubscribePointerPressEvent(OnPress);
            obj.SubscribePointerGrabEvent(OnGrabStart, OnGrabEnd);
        }
    }

    private void OnPress(object sender, PointerInteractionEventArgs args)
    {
        Debug.Log("Press: " + args.HandleObject);
    }

    private void OnDragStart(object sender, PointerDragInteractionEventArgs args)
    {
        Debug.Log("Drag start: " + args.HandleObject);
    }

    private void OnDrag(object sender, PointerDragInteractionEventArgs args)
    {
        Debug.Log("Drag: " + args.HandleObject);
    }

    private void OnDragEnd(object sender, PointerDragInteractionEventArgs args)
    {
        Debug.Log("Drag end: " + args.HandleObject);
    }

    private void OnGrabStart(object sender, PointerInteractionEventArgs args)
    {
        Debug.Log("Grab start: " + args.HandleObject);
    }

    private void OnGrabEnd(object sender, PointerInteractionEventArgs args)
    {
        Debug.Log("Grab end: " + args.HandleObject);
    }
}
