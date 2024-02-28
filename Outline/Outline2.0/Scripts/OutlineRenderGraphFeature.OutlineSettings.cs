// using System;
// using UnityEngine;
//
// namespace UTools.Outline
// {
//     public partial class OutlineRenderGraphFeature
//     {
//         [Serializable]
//         public class OutlineSettings
//         {
//             [SerializeField] private Color m_Color = Color.white;
//             [SerializeField] [Range(1, 32)] private int m_OutlineWidth = 16;
//             [SerializeField] [Range(1, 32)] private int m_BlurWidth = 16;
//             [SerializeField] private LayerMask m_LayerMask = -1;
//
//             public Color Color => m_Color;
//             public int OutlineWidth => m_OutlineWidth;
//             public int BlurWidth => m_BlurWidth;
//             public LayerMask LayerMask => m_LayerMask;
//         }
//     }
// }