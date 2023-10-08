using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace UTools
{
    public class TextDebugger : SingletonMonoBehaviour<TextDebugger>
    {
        [SerializeField] private TextMeshProUGUI mText = default;
        [SerializeField] private Color mTextColor = Color.black;
        [SerializeField] private Color mErrorColor = Color.red;
        [SerializeField] private Color mWarningColor = Color.yellow;

        public void Awake() => mText.color = mTextColor;

        public void Log(object logText) => mText.text += logText + Environment.NewLine;

        public void LogWarning(object logText) => LogColored(logText, mWarningColor);

        public void LogError(object logText) => LogColored(logText, mErrorColor);

        public void LogColored(object logText, Color color) => mText.text += $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{logText}</color>{Environment.NewLine}";

        public void Clear() => mText.text = string.Empty;
    }
}