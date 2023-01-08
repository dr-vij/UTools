using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTools.Editor
{
    [Serializable]
    public class SceneViewSettings
    {
        private SceneViewPlane m_Plane;
        private List<Plane> m_Planes = new List<Plane>();

        public IReadOnlyList<Plane> ZoomPlanes => m_Planes;

        public SceneViewPlane SceneViewPlane
        {
            get { return m_Plane; }
            set
            {
                m_Plane = value;
                UplanePlanes();
            }
        }

        public SceneViewSettings()
        {
            SceneViewPlane = SceneViewPlane.YPlane;

            ScrollSensetivity = 0.5f;
            NearClip = 0.01f;
            FarClip = 1000;
        }

        private void UplanePlanes()
        {
            m_Planes.Clear();
            switch (m_Plane)
            {
                case SceneViewPlane.XPlane:
                    m_Planes.Add(new Plane(Vector3.right, Vector3.zero));
                    m_Planes.Add(new Plane(Vector3.left, Vector3.zero));
                    break;
                case SceneViewPlane.YPlane:
                    m_Planes.Add(new Plane(Vector3.up, Vector3.zero));
                    m_Planes.Add(new Plane(Vector3.down, Vector3.zero));
                    break;
                case SceneViewPlane.ZPlane:
                    m_Planes.Add(new Plane(Vector3.forward, Vector3.zero));
                    m_Planes.Add(new Plane(Vector3.back, Vector3.zero));
                    break;
            }
        }

        public float ScrollSensetivity;
        public float NearClip;
        public float FarClip;
    }
}