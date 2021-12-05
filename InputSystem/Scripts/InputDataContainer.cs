using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViJTools
{
    public class InputDataContainer
    {
        private bool mIsPointerDownTriggered = false;
        private bool mIsDragTriggered = false;

        public Vector2 PointerDownPosition { get; set; } = Vector2.zero;
        public Vector2 PointerCurrentPosition { get; set; } = Vector2.zero;
        public Vector2 PointerPreviousPosition { get; set; } = Vector2.zero;
        public Camera InteractionCamera { get; set; } = default;
        public InteractionObject InteractionObject { get; set; } = default;

        public Vector2 PointerCurrentDelta => PointerCurrentPosition - PointerPreviousPosition;

        public Vector2 PointerTotalDelta => PointerCurrentPosition - PointerDownPosition;

        public bool IsPointerDownTriggered => mIsPointerDownTriggered;

        public bool IsDragTriggered => mIsDragTriggered;

        public void TriggerDrag()
        {
            mIsDragTriggered = true;
        }

        public void StartInput()
        {
            if (!mIsPointerDownTriggered)
            {
                ResetInputData();
                mIsPointerDownTriggered = true;
            }
        }

        public void StopInput()
        {
            if (mIsPointerDownTriggered)
            {
                ResetInputData();
                mIsPointerDownTriggered = false;
            }
        }

        private void ResetInputData()
        {
            mIsDragTriggered = false;

            PointerDownPosition = Vector2.zero;
            PointerCurrentPosition = Vector2.zero;
            PointerPreviousPosition = Vector2.zero;

            InteractionObject = null;
            InteractionCamera = null;
        }
    }
}