using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTools.Input
{
    /// <summary>
    /// Simple object to contain information for drag inputs.
    /// </summary>
    public struct PointerInput
    {
        public bool Contact;

        /// <summary>
        /// ID of input type.
        /// </summary>
        public int InputId;

        /// <summary>
        /// Position of input device
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// Orientation of input pen.
        /// </summary>
        public Vector2? Tilt;

        /// <summary>
        /// Pressure of input device.
        /// </summary>
        public float? Pressure;

        /// <summary>
        /// Radius of input device.
        /// </summary>
        public Vector2? Radius;

        /// <summary>
        /// Twist of input device.
        /// </summary>
        public float? Twist;
    }
}