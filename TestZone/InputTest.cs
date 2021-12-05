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
        }
    }

    private void OnDragStart(object sender, PointerDragInteractionEventArgs args)
    {
        Debug.Log(args.HandleObject);
    }

    private void OnDrag(object sender, PointerDragInteractionEventArgs args)
    {
        Debug.Log(args.HandleObject);
    }

    private void OnDragEnd(object sender, PointerDragInteractionEventArgs args)
    {
        Debug.Log(args.HandleObject);
    }
}
