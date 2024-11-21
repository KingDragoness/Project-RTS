using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 class Debug : UnityEngine.Debug
{
    public static void DrawCircle(Vector3 center, float radius, Color color, Vector3 rotation, float numSegments = 40, float duration = 0.01f)
    {
        Quaternion rotQuaternion = Quaternion.AngleAxis(360.0f / numSegments, rotation);
        Vector3 vertexStart = new Vector3(radius, 0.0f);
        for (int i = 0; i < numSegments; i++)
        {
            Vector3 rotatedPoint = rotQuaternion * vertexStart;

            // Draw the segment, shifted by the center
            Debug.DrawLine(center + vertexStart, center + rotatedPoint, color, duration);

            vertexStart = rotatedPoint;
        }
    }
}
