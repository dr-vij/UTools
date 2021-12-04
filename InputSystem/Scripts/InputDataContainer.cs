using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViJTools
{
    public class InputDataContainer
    {
        private bool mIsPointerDownTriggered = false;
        private bool mIsDragTriggered = false;

        public Vector2 PointerDownPosition = Vector2.zero;
        public Vector2 PointerCurrentPosition = Vector2.zero;
        public Vector2 PointerPreviousPosition = Vector2.zero;

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
        }
    }
}