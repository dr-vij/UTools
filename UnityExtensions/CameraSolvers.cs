using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtilities
{
    /// <summary>
    /// Determines if the projection of the "newPoint" onto the "direction" vector is greater than the projection of the "originalPoint".
    /// </summary>
    /// <param name="original">The original point for comparison.</param>
    /// <param name="compared">The new point for comparison.</param>
    /// <param name="direction">The direction vector to project the points onto.</param>
    /// <returns>True if the projection of the "newPoint" is greater, false otherwise.</returns>
    public static bool IsProjectionGreater(Vector3 original, Vector3 compared, Vector3 direction)
    {
        return Vector3.Dot(direction, compared - original) > 0;
    }

    public static Bounds GetAllRendererBounds(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        return GetCombinedBounds(renderers);
    }

    public static Bounds GetCombinedBounds(Renderer[] renderers)
    {
        if (renderers == null || renderers.Length == 0)
            return new Bounds();
        var combinedBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            combinedBounds.Encapsulate(renderers[i].bounds);
        return combinedBounds;
    }

    public static List<Vector3> GetBoundsCorners(this Bounds bounds)
    {
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        return new List<Vector3>
        {
            min,
            new(min.x, min.y, max.z),
            new(min.x, max.y, min.z),
            new(min.x, max.y, max.z),
            new(max.x, min.y, min.z),
            new(max.x, min.y, max.z),
            new(max.x, max.y, min.z),
            max
        };
    }
}

public static class CameraSolver
{
    /// <summary>
    /// Calculates the desired position of the camera to fit the given points within the camera's view frustum.
    /// </summary>
    /// <param name="camera">The camera to adjust.</param>
    /// <param name="points">A list of points to fit within the camera's view.</param>
    /// <param name="leftGap">The left gap as a percentage of the screen width.</param>
    /// <param name="rightGap">The right gap as a percentage of the screen width.</param>
    /// <param name="topGap">The top gap as a percentage of the screen height.</param>
    /// <param name="bottomGap">The bottom gap as a percentage of the screen height.</param>
    /// <param name="calculatedCameraPosition">The resulting desired camera position.</param>
    /// <returns>True if a solution is found, false otherwise.</returns>
    public static bool CalculatePositionToFitPoints(Camera camera, List<Vector3> points,
        out Vector3 calculatedCameraPosition, float leftGap = 0f, float rightGap = 0f,
        float topGap = 0f, float bottomGap = 0f)
    {
        var transform = camera.transform;
        var forward = transform.forward;
        var right = transform.right;
        var up = transform.up;

        var verticalFovFactor = Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        var aspectRatio = camera.aspect;
        var leftProjectionFactor = (1 - 2 * leftGap) * aspectRatio * verticalFovFactor;
        var rightProjectionFactor = (1 - 2 * rightGap) * aspectRatio * verticalFovFactor;
        var topProjectionFactor = (1 - 2 * topGap) * verticalFovFactor;
        var bottomProjectionFactor = (1 - 2 * bottomGap) * verticalFovFactor;

        var rightBoundary = right - forward * rightProjectionFactor;
        var leftBoundary = -right - forward * leftProjectionFactor;
        var upperBoundary = up - forward * topProjectionFactor;
        var lowerBoundary = -up - forward * bottomProjectionFactor;

        if (points.Count > 0)
        {
            var extremeRightPoint = points[0];
            var extremeLeftPoint = points[0];
            var extremeTopPoint = points[0];
            var extremeBottomPoint = points[0];

            for (int i = 1; i < points.Count; i++)
            {
                if (MathUtilities.IsProjectionGreater(extremeRightPoint, points[i], rightBoundary))
                    extremeRightPoint = points[i];
                if (MathUtilities.IsProjectionGreater(extremeLeftPoint, points[i], leftBoundary))
                    extremeLeftPoint = points[i];
                if (MathUtilities.IsProjectionGreater(extremeTopPoint, points[i], upperBoundary))
                    extremeTopPoint = points[i];
                if (MathUtilities.IsProjectionGreater(extremeBottomPoint, points[i], lowerBoundary))
                    extremeBottomPoint = points[i];
            }

            var leftDirection = forward - right * leftProjectionFactor;
            var rightDirection = forward + right * rightProjectionFactor;
            var topDirection = forward + up * topProjectionFactor;
            var bottomDirection = forward - up * bottomProjectionFactor;

            if (TrySolveLinearCramer(leftDirection, up, rightDirection, extremeRightPoint - extremeLeftPoint,
                    out var leftRightSolution) &&
                TrySolveLinearCramer(bottomDirection, right, topDirection, extremeTopPoint - extremeBottomPoint,
                    out var bottomTopSolution))
            {
                var leftRightIntersection = extremeLeftPoint + leftDirection * leftRightSolution.x;
                var bottomTopIntersection = extremeBottomPoint + bottomDirection * bottomTopSolution.x;

                if (TrySolveLinearCramer(up, forward, right, bottomTopIntersection - leftRightIntersection,
                        out var frustumSolution))
                {
                    if (frustumSolution.y < 0)
                        calculatedCameraPosition = bottomTopIntersection - right * frustumSolution.z;
                    else
                        calculatedCameraPosition = leftRightIntersection + up * frustumSolution.x;
                    return true;
                }
            }
        }

        calculatedCameraPosition = Vector3.zero;
        return false;
    }

    /// <summary>
    /// Attempts to solve a system of linear equations with three variables using Cramer's Rule.
    /// </summary>
    /// <param name="a">The first coefficient vector.</param>
    /// <param name="b">The second coefficient vector.</param>
    /// <param name="c">The third coefficient vector.</param>
    /// <param name="d">The target vector.</param>
    /// <param name="result">The resulting coefficients for the three direction vectors if a solution is found.</param>
    /// <returns>True if a solution is found, false otherwise.</returns>
    private static bool TrySolveLinearCramer(Vector3 a, Vector3 b, Vector3 c, Vector3 d, out Vector3 result)
    {
        var determinant = CalculateDeterminant(a, b, c);

        if (determinant != 0)
        {
            var inverseDeterminant = 1 / determinant;
            result = new Vector3(CalculateDeterminant(d, b, c) * inverseDeterminant,
                CalculateDeterminant(a, d, c) * inverseDeterminant, CalculateDeterminant(a, b, d) * inverseDeterminant);
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    /// <summary>
    /// Calculates the determinant of a 3x3 matrix formed by three vectors.
    /// </summary>
    /// <param name="a">The first column vector of the matrix.</param>
    /// <param name="b">The second column vector of the matrix.</param>
    /// <param name="c">The third column vector of the matrix.</param>
    /// <returns>The determinant of the matrix.</returns>
    private static float CalculateDeterminant(Vector3 a, Vector3 b, Vector3 c)
    {
        return a.x * b.y * c.z + b.x * c.y * a.z + c.x * a.y * b.z - a.x * c.y * b.z - b.x * a.y * c.z -
               c.x * b.y * a.z;
    }
}