using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;

namespace UTools.Input
{
    public static class Helpers
    {
        public const int LeftMouseInputId = PointerInputModule.kMouseLeftId;
        public const int RightMouseInputId = PointerInputModule.kMouseRightId;
        public const int MiddleMouseInputId = PointerInputModule.kMouseMiddleId;
        public const int PenInputId = int.MinValue;
    }

    // What we do in PointerInputManager is to simply create a separate action for each input we need for PointerInput.
    // This here shows a possible alternative that sources all inputs as a single value using a composite. Has pros
    // and cons. Biggest pro is that all the controls actuate together and deliver one input value.
    //
    // NOTE: In PointerControls, we are binding mouse and pen separately from touch. If we didn't care about multitouch,
    //       we wouldn't have to to that but could rather just bind `<Pointer>/position` etc. However, to source each touch
    //       as its own separate PointerInput source, we need to have multiple PointerInputComposites.
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class PointerInputComposite : InputBindingComposite<PointerInput>
    {
        [InputControl(layout = "Button")] public int Contact;
        [InputControl(layout = "Vector2")] public int Position;
        [InputControl(layout = "Vector2")] public int Tilt;
        [InputControl(layout = "Vector2")] public int Radius;
        [InputControl(layout = "Axis")] public int Pressure;
        [InputControl(layout = "Axis")] public int Twist;
        [InputControl(layout = "Integer")] public int InputId;

        public override PointerInput ReadValue(ref InputBindingCompositeContext context)
        {
            //foreach(var control in  context.controls)
            //    Debug.Log(control.control.device.GetType());

            //Primary data
            var contact = context.ReadValueAsButton(Contact);
            var pointerId = context.ReadValue<int>(InputId); //Input ID does not work for mouse buttons
            var position = context.ReadValue<Vector2, Vector2MagnitudeComparer>(Position);

            //Secondary data
            var pressure = context.ReadValue<float>(Pressure);
            var radius = context.ReadValue<Vector2, Vector2MagnitudeComparer>(Radius);
            var tilt = context.ReadValue<Vector2, Vector2MagnitudeComparer>(Tilt);
            var twist = context.ReadValue<float>(Twist);

            return new PointerInput
            {
                Contact = contact,
                InputId = pointerId,
                Position = position,

                Tilt = tilt != default ? tilt : null,
                Pressure = pressure > 0 ? pressure : null,
                Radius = radius.sqrMagnitude > 0 ? radius : null,
                Twist = twist > 0 ? twist : null,
            };
        }

#if UNITY_EDITOR
        static PointerInputComposite() => Register();
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register() => InputSystem.RegisterBindingComposite<PointerInputComposite>();
    }
}