using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace UTools.Input
{
    /// <summary>
    /// The input source of the screen: turns the mouse, the pen and the touches of the Input System into pointers of
    /// the registered cameras and hands them to the gesture recognizer. It leads one gesture at a time, the way
    /// FocusedAdmission admits pointers; a pointer that goes down over the UI is not led at all.
    /// </summary>
    public class InputManager : SingletonMonoBehaviour<InputManager>
    {
        public delegate void ScrollHandler(Vector2 scrollDelta, Vector2 pointerPosition);

        private const int MouseContactId = Helpers.LeftMouseInputId;

        /// <summary>
        /// How far, in pixels, a pointer travels before its press becomes a drag, unless DragThreshold says otherwise.
        /// </summary>
        [SerializeField] private int m_DragOrPressTriggerDistance;

        private InputActions m_Actions;
        private readonly List<Camera> m_Cameras = new();
        private readonly CameraTracer m_CameraTracer = new();
        private readonly GestureRecognizer m_Recognizer = new();
        private FocusedAdmission m_Admission;

        //Every contact that is down, by its id: the pointer of one that is led, null for one that is not - it went down
        //over the UI or was not admitted, and its moves and its lift are dropped.
        private readonly Dictionary<int, ScreenPointer> m_Contacts = new();
        private readonly Dictionary<int, InputDevice> m_ContactDevices = new();
        private readonly List<int> m_StaleContacts = new();
        private readonly List<PointerHit> m_Hits = new();
        private InputDevice m_ActiveDevice;

        //The mouse makes one contact: the button that started it; the others do nothing until it is over.
        private PointerButton? m_MouseButton;

        //A mouse or a pen that is not pressed hovers: the one seen last, where it was, and the pointer that stands for it.
        private InputDevice m_HoverDevice;
        private Vector2 m_HoverPosition;
        private ScreenPointer m_HoverPointer;

        private event ScrollHandler m_GlobalScrollEvent;

        public CameraTracer CameraTracer => m_CameraTracer;

        /// <summary>
        /// The recognizer the pointers of the screen go to. Another source may lead its pointers through it too.
        /// </summary>
        public GestureRecognizer Recognizer => m_Recognizer;

        /// <summary>
        /// How far, in pixels, a pointer of the kind travels before its press becomes a drag; null, and without the
        /// function every kind, is the distance set on the component.
        /// </summary>
        public Func<PointerKind, float?> DragThreshold { get; set; }

        public void RegisterCamera(Camera cam)
        {
            if (!m_Cameras.Contains(cam))
            {
                m_Cameras.Add(cam);
                if (!cam.TryGetComponent<InteractionObjectBase>(out _))
                    Debug.LogError("No interaction object for the camera");
                m_Cameras.Sort((cam1, cam2) => cam1.depth.CompareTo(cam2.depth));
            }
            else
            {
                Debug.LogWarning("Camera already registered");
            }
        }

        public IDisposable SubscribeGlobalScrollDelta(ScrollHandler scrollHandler)
        {
            m_GlobalScrollEvent += scrollHandler;
            return new DisposeAction(() => m_GlobalScrollEvent -= scrollHandler);
        }

        #region Contacts

        /// <summary>
        /// A contact goes down at a point of the screen: the same entry the devices come through, for input fed by
        /// hand. The id tells the contact from the others that are down.
        /// </summary>
        public void PointerDown(int id, PointerKind kind, PointerButton button, Vector2 position) => Down(id, kind, button, position, null);

        public void PointerMove(int id, Vector2 position)
        {
            if (!m_Contacts.TryGetValue(id, out var pointer) || pointer == null)
                return;

            pointer.MoveTo(position);
            m_Recognizer.Move(pointer);
        }

        public void PointerUp(int id, Vector2 position)
        {
            if (!m_Contacts.TryGetValue(id, out var pointer))
                return;

            if (pointer != null)
            {
                if (pointer.ScreenPosition != position)
                    PointerMove(id, position);
                m_Recognizer.Lift(pointer);
            }

            Forget(id, pointer);
        }

        private void Down(int id, PointerKind kind, PointerButton button, Vector2 position, InputDevice device)
        {
            DropStaleContacts();
            //A pointer that goes down hovers no more, at once and not a frame later.
            StopHovering();
            if (m_Contacts.ContainsKey(id))
                return;

            m_Contacts.Add(id, Admit(id, kind, button, position));
            if (device != null)
                m_ContactDevices.Add(id, device);
        }

        //The pointer of the contact if it is led from now on, or null.
        private ScreenPointer Admit(int id, PointerKind kind, PointerButton button, Vector2 position)
        {
            if (m_CameraTracer.IsOverUI(position) || !TryGetCamera(position, out var cam))
                return null;

            m_CameraTracer.TraceInteractionObjects(cam, position, m_Hits);
            var threshold = DragThreshold?.Invoke(kind) ?? m_DragOrPressTriggerDistance;
            return (ScreenPointer)m_Admission.Admit(kind, button, m_Hits, cam.GetComponent<InteractionObjectBase>(),
                (hits, fallback) => new ScreenPointer(id, kind, button, cam, position, threshold, hits, fallback));
        }

        private void Forget(int id, ScreenPointer pointer)
        {
            if (pointer != null)
            {
                m_Admission.Forget(pointer);
                m_Recognizer.Hover(pointer, null);
            }

            m_Contacts.Remove(id);
            m_ContactDevices.Remove(id);
            if (id == MouseContactId)
                m_MouseButton = null;
            if (m_Contacts.Count == 0)
                m_ActiveDevice = null;
        }

        //A contact whose device no longer holds it lost its lift on the way, and one whose device is gone will never
        //get it: it would stay down for ever and turn every next press into a second pointer. Its gesture ends where
        //it last was, without a press.
        private void DropStaleContacts()
        {
            m_StaleContacts.Clear();
            foreach (var pair in m_ContactDevices)
            {
                if (!IsDown(pair.Key, pair.Value))
                    m_StaleContacts.Add(pair.Key);
            }

            foreach (var id in m_StaleContacts)
            {
                var pointer = m_Contacts[id];
                if (pointer != null)
                    m_Recognizer.Cancel(pointer);
                Forget(id, pointer);
            }
        }

        private bool IsDown(int id, InputDevice device)
        {
            //A device that is gone holds nothing, and nothing of it can be read.
            if (!device.added)
                return false;

            switch (device)
            {
                case Touchscreen touchscreen:
                    foreach (var touch in touchscreen.touches)
                    {
                        if (touch.press.isPressed && touch.touchId.ReadValue() == id)
                            return true;
                    }

                    return false;
                case Mouse mouse:
                    return m_MouseButton switch
                    {
                        PointerButton.Secondary => mouse.rightButton.isPressed,
                        PointerButton.Middle => mouse.middleButton.isPressed,
                        _ => mouse.leftButton.isPressed,
                    };
                case Pen pen:
                    return pen.tip.isPressed;
                default:
                    return true;
            }
        }

        private bool TryGetCamera(Vector2 position, out Camera result)
        {
            if (m_Cameras.Count == 0)
                Debug.LogWarning("No traceable cameras. Did you forget to add one?");

            foreach (var cam in m_Cameras)
            {
                if (m_CameraTracer.CanBeTraced(cam, position))
                {
                    result = cam;
                    return true;
                }
            }

            result = null;
            return false;
        }

        #endregion

        #region Hover

        //What a pointer is over may change under a still pointer: the scene moves. Told once a frame, as long as the
        //pointer is there: a mouse or a pen until it is pressed, a finger while it is down and led.
        private void Update()
        {
            foreach (var pointer in m_Contacts.Values)
            {
                if (pointer != null && pointer.Kind == PointerKind.Touch)
                    m_Recognizer.Hover(pointer, TopAt(pointer.Camera, pointer.ScreenPosition));
            }

            if (!TryGetHoverKind(out var kind) || m_Contacts.Count != 0)
            {
                StopHovering();
                return;
            }

            if (m_CameraTracer.IsOverUI(m_HoverPosition) || !TryGetCamera(m_HoverPosition, out var cam))
            {
                StopHovering();
                return;
            }

            if (m_HoverPointer == null || m_HoverPointer.Kind != kind || m_HoverPointer.Camera != cam)
            {
                StopHovering();
                m_HoverPointer = new ScreenPointer(MouseContactId, kind, PointerButton.Primary, cam, m_HoverPosition, 0f, Array.Empty<PointerHit>(), null);
            }

            m_HoverPointer.MoveTo(m_HoverPosition);
            m_Recognizer.Hover(m_HoverPointer, TopAt(cam, m_HoverPosition));
        }

        //A mouse hovers wherever it is. A pen hovers only while it is near the screen: one taken away leaves the place
        //it was last seen at, and nothing is under it there.
        private bool TryGetHoverKind(out PointerKind kind)
        {
            kind = m_HoverDevice is Pen ? PointerKind.Pen : PointerKind.Mouse;
            return m_HoverDevice switch
            {
                null => false,
                Pen pen => pen.added && pen.inRange.isPressed,
                _ => m_HoverDevice.added,
            };
        }

        private void StopHovering()
        {
            if (m_HoverPointer == null)
                return;

            m_Recognizer.Hover(m_HoverPointer, null);
            m_HoverPointer = null;
        }

        private InteractionObjectBase TopAt(Camera cam, Vector2 position)
        {
            if (m_CameraTracer.IsOverUI(position))
                return null;

            m_CameraTracer.TraceInteractionObjects(cam, position, m_Hits);
            return m_Hits.Count != 0 ? m_Hits[0].Target : null;
        }

        #endregion

        #region Input System

        /// <summary>
        /// Create Actions and subscribe it's events
        /// </summary>
        private void OnEnable()
        {
            m_Admission ??= new FocusedAdmission(m_Recognizer);
            m_Actions = new InputActions();
            m_Actions.GestureActions.Enable();

            //Unified pointer events. mouse left button + pen + touch
            m_Actions.GestureActions.Pointer.performed += OnPointerEvent;
            m_Actions.GestureActions.Pointer.canceled += OnPointerEvent;

            //The other two buttons of the mouse are pointers too, and have no contact in the composite
            m_Actions.GestureActions.MouseRight.started += OnRightMouseButtonDownEvent;
            m_Actions.GestureActions.MouseRight.canceled += OnRightMouseButtonUpEvent;
            m_Actions.GestureActions.MouseMiddle.started += OnMiddleMouseButtonDownEvent;
            m_Actions.GestureActions.MouseMiddle.canceled += OnMiddleMouseButtonUpEvent;

            m_Actions.GestureActions.MousePosition.performed += OnMousePositionEvent;

            m_Actions.GestureActions.MouseScroll.performed += OnMouseScrollEvent;

            InputSystem.onDeviceChange += OnDeviceChange;
        }

        /// <summary>
        /// Unsubscribe from all events and dispose Actions
        /// </summary>
        private void OnDisable()
        {
            StopHovering();
            m_Actions.Disable();

            m_Actions.GestureActions.Pointer.performed -= OnPointerEvent;
            m_Actions.GestureActions.Pointer.canceled -= OnPointerEvent;

            m_Actions.GestureActions.MouseRight.started -= OnRightMouseButtonDownEvent;
            m_Actions.GestureActions.MouseRight.canceled -= OnRightMouseButtonUpEvent;
            m_Actions.GestureActions.MouseMiddle.started -= OnMiddleMouseButtonDownEvent;
            m_Actions.GestureActions.MouseMiddle.canceled -= OnMiddleMouseButtonUpEvent;

            m_Actions.GestureActions.MousePosition.performed -= OnMousePositionEvent;

            m_Actions.GestureActions.MouseScroll.performed -= OnMouseScrollEvent;

            InputSystem.onDeviceChange -= OnDeviceChange;
        }

        //A device that goes away - unplugged, out of power, out of reach - lifts nothing of what it held down.
        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Removed)
                DropStaleContacts();
        }

        private void OnPointerEvent(InputAction.CallbackContext context)
        {
            //I DO NOT ALLOW TO MIX DIFFERENT DEVICES IN THIS COMBINATION OF GESTURES
            if (!CanBeHandled(context))
                return;

            //The values are read from the device the event came from, not from the composite: with two devices of a kind
            //the composite takes each part from whichever of them has the larger value, a button of one at the place
            //of the other.
            switch (context.control.device)
            {
                case Touchscreen touchscreen when context.control.parent is TouchControl touch:
                    OnContact(touch.touchId.ReadValue(), PointerKind.Touch, touch.press.isPressed, touch.position.ReadValue(), touchscreen);
                    break;
                case Pen pen:
                    SeeHovering(pen, pen.position.ReadValue());
                    OnContact(Helpers.PenInputId, PointerKind.Pen, pen.tip.isPressed, pen.position.ReadValue(), pen);
                    break;
                case Mouse mouse:
                    //The left button comes here; the other two have no contact in the composite.
                    OnMouseButton(PointerButton.Primary, mouse.leftButton.isPressed, mouse.position.ReadValue(), mouse);
                    break;
            }
        }

        private void OnContact(int id, PointerKind kind, bool isDown, Vector2 position, InputDevice device)
        {
            if (!isDown)
                PointerUp(id, position);
            else if (m_Contacts.ContainsKey(id))
                PointerMove(id, position);
            else
            {
                m_ActiveDevice = device;
                Down(id, kind, PointerButton.Primary, position, device);
            }
        }

        private void OnMouseButton(PointerButton button, bool isDown, Vector2 position, InputDevice device)
        {
            //The contact of the mouse belongs to the button and the mouse that started it.
            if (m_MouseButton == button && IsMouseContactOf(device))
            {
                if (isDown)
                    PointerMove(MouseContactId, position);
                else
                    PointerUp(MouseContactId, position);
                return;
            }

            if (!isDown)
                return;

            //Another button or another mouse is remembered as down: unless it still is, its lift was lost.
            if (m_MouseButton != null)
                DropStaleContacts();
            if (m_MouseButton != null)
                return;

            m_ActiveDevice = device;
            m_MouseButton = button;
            Down(MouseContactId, PointerKind.Mouse, button, position, device);
        }

        private bool IsMouseContactOf(InputDevice device) => !m_ContactDevices.TryGetValue(MouseContactId, out var owner) || owner == device;

        private void OnRightMouseButtonDownEvent(InputAction.CallbackContext context) => OnMouseButtonEvent(context, PointerButton.Secondary, true);

        private void OnRightMouseButtonUpEvent(InputAction.CallbackContext context) => OnMouseButtonEvent(context, PointerButton.Secondary, false);

        private void OnMiddleMouseButtonDownEvent(InputAction.CallbackContext context) => OnMouseButtonEvent(context, PointerButton.Middle, true);

        private void OnMiddleMouseButtonUpEvent(InputAction.CallbackContext context) => OnMouseButtonEvent(context, PointerButton.Middle, false);

        private void OnMouseButtonEvent(InputAction.CallbackContext context, PointerButton button, bool isDown)
        {
            if (!CanBeHandled(context))
                return;

            //Where the mouse is now, from the device: a button may go down before the mouse has ever moved.
            var mouse = (Mouse)context.control.device;
            OnMouseButton(button, isDown, mouse.position.ReadValue(), mouse);
        }

        private void OnMousePositionEvent(InputAction.CallbackContext context)
        {
            //A pass-through action: it tells of every mouse that moves, and the place is read from that mouse.
            var mouse = (Mouse)context.control.device;
            var position = mouse.position.ReadValue();
            SeeHovering(mouse, position);
            //The left button moves with the pointer composite; the other two have only this.
            if (m_MouseButton is PointerButton.Secondary or PointerButton.Middle && IsMouseContactOf(mouse))
                PointerMove(MouseContactId, position);
        }

        private void SeeHovering(InputDevice device, Vector2 position)
        {
            m_HoverDevice = device;
            m_HoverPosition = position;
        }

        private void OnMouseScrollEvent(InputAction.CallbackContext context)
        {
            if (!CanBeHandled(context))
                return;

            var scrollData = context.ReadValue<Vector2>();
            var position = ((Mouse)context.control.device).position.ReadValue();
            if (!m_CameraTracer.IsOverUI(position))
                m_GlobalScrollEvent?.Invoke(scrollData, position);
        }

        /// <summary>
        /// Checks if action can be handled from this device. We do not allow to use pen/mouse and touch at the same time
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool CanBeHandled(InputAction.CallbackContext context)
        {
            //An action is cancelled as a device it is bound to is removed, and nothing of a device that is gone can be
            //read: what it held down ends in OnDeviceChange.
            var device = context.control.device;
            return device.added && (m_ActiveDevice == null || m_ActiveDevice.GetType() == device.GetType());
        }

        #endregion
    }
}
