using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UTools.Editor
{
    [InitializeOnLoad]
    public static class EditorMeshIntersections
    {
        private static MethodInfo IntersectMeshMethod;

        static EditorMeshIntersections()
        {
            var type_HandleUtility = typeof(UnityEditor.Editor).Assembly.GetTypes().First(t => t.Name == "HandleUtility");
            var methods = type_HandleUtility.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
            IntersectMeshMethod = type_HandleUtility.GetMethod("IntersectRayMesh", BindingFlags.Static | BindingFlags.NonPublic);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="meshFilter"></param>
        /// <param name="hit"></param>
        /// <returns></returns>
        public static bool RaycastMeshFilter2(Ray ray, MeshFilter meshFilter, out RaycastHit hit)
        {
            return RaycastMesh(ray, meshFilter.sharedMesh, meshFilter.transform.localToWorldMatrix, out hit);
        }

        /// <summary>
        /// Takes meshfilter and intersects it with given ray
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="meshFilter"></param>
        /// <param name="hit"></param>
        /// <returns></returns>
        public static bool RaycastMeshFilter(Ray ray, MeshFilter meshFilter, out RaycastHit hit)
        {
            var trans = meshFilter.transform;
            var newRayOrigin = trans.InverseTransformPoint(ray.origin);
            var newRayDirection = trans.InverseTransformDirection(ray.direction);
            var newRay = new Ray(newRayOrigin, newRayDirection);
            var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
            var result = RaycastMesh(newRay, meshFilter.sharedMesh, matrix, out hit);
            hit.point = trans.TransformPoint(hit.point);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="mesh"></param>
        /// <param name="matrix"></param>
        /// <param name="hit"></param>
        /// <returns></returns>
        public static bool RaycastMesh(Ray ray, Mesh mesh, Matrix4x4 matrix, out RaycastHit hit)
        {
            var parameters = new object[] { ray, mesh, matrix, null };
            bool result = (bool)IntersectMeshMethod.Invoke(null, parameters);
            hit = (RaycastHit)parameters[3];
            return result;
        }

        public static GameObject GetObjectUnder2dPosition(Vector2 position, bool selectPrefabRoot = false) => HandleUtility.PickGameObject(position, selectPrefabRoot);
    }
}