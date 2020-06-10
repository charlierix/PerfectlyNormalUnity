using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PerfectlyNormalUnity
{
    public static class Math2D
    {
        public static TransformsToFrom2D GetTransformTo2D(Plane plane)
        {
            Vector3 zUp = new Vector3(0, 0, 1);

            if (Math.Abs(Vector3.Dot(plane.normal.normalized, zUp)).IsNearValue(1))
            {
                // It's already 2D
                float distZ = plane.GetDistanceToPoint(new Vector3());      // this might be the same as plane.distance, but it's not clear whether plane.distance is signed
                return new TransformsToFrom2D()
                {
                    From3D_To2D = Matrix4x4.TRS(new Vector3(0, 0, -distZ), Quaternion.identity, Vector3.one),
                    From2D_BackTo3D = Matrix4x4.TRS(new Vector3(0, 0, distZ), Quaternion.identity, Vector3.one)
                };
            }

            // Don't bother with a double vector, just rotate the normal
            Quaternion rotation = Quaternion.FromToRotation(plane.normal, zUp);
            Vector3 rotatedXYPlane = rotation * plane.ClosestPointOnPlane(Vector3.zero);       // just need any point on the plane, then rotate so all points on the plane share the same z value

            Matrix4x4 transformTo2D = Matrix4x4.TRS(new Vector3(0, 0, -rotatedXYPlane.z), rotation, Vector3.one);
            Matrix4x4 transformTo3D = Matrix4x4.TRS(new Vector3(0, 0, rotatedXYPlane.z), Quaternion.Inverse(rotation), Vector3.one);

            return new TransformsToFrom2D()
            {
                From3D_To2D = transformTo2D,
                From2D_BackTo3D = transformTo3D,
            };
        }

        public static Vector2? GetIntersection_LineSegment_Circle(Vector2 lineStart, Vector2 lineEnd, Vector2 circleCenter, float radius)
        {
            float? percent = GetIntersection_LineSegment_Circle_percent(lineStart, lineEnd, circleCenter, radius);
            if (percent == null)
            {
                return null;
            }
            else
            {
                return lineStart + ((lineEnd - lineStart) * percent.Value);
            }
        }
        /// <summary>
        /// Got this here:
        /// http://stackoverflow.com/questions/1073336/circle-line-collision-detection
        /// </summary>
        public static float? GetIntersection_LineSegment_Circle_percent(Vector2 lineStart, Vector2 lineEnd, Vector2 circleCenter, float radius)
        {
            Vector2 lineDir = lineEnd - lineStart;

            Vector2 C = circleCenter;
            float r = radius;
            Vector2 E = lineStart;
            Vector2 d = lineDir;
            Vector2 f = E - C;

            Vector3 d3D = new Vector3(d.x, d.y, 0);
            Vector3 f3D = new Vector3(f.x, f.y, 0);

            float a = Vector3.Dot(d3D, d3D);
            float b = 2f * Vector3.Dot(f3D, d3D);
            float c = Vector3.Dot(f3D, f3D) - (r * r);

            float discriminant = (b * b) - (4 * a * c);
            if (discriminant < 0f)
            {
                // no intersection
                return null;
            }
            else
            {
                // ray didn't totally miss circle, so there is a solution to the equation.
                discriminant = (float)Math.Sqrt(discriminant);

                // either solution may be on or off the ray so need to test both
                float t1 = (-b + discriminant) / (2f * a);
                float t2 = (-b - discriminant) / (2f * a);

                if (t1 >= 0f && t1 <= 1f)
                {
                    // t1 solution on is ON THE RAY.
                    return t1;
                }
                else if (t1.IsNearZero())
                {
                    return 0f;
                }
                else if (t1.IsNearValue(1f))
                {
                    return 1f;
                }
                else
                {
                    // t1 solution "out of range" of ray
                    //return null;
                }

                if (t2 >= 0f && t2 <= 1f)
                {
                    // t2 solution on is ON THE RAY.
                    return t2;
                }
                else if (t2.IsNearZero())
                {
                    return 0f;
                }
                else if (t2.IsNearValue(1f))
                {
                    return 1f;
                }
                else
                {
                    // t2 solution "out of range" of ray
                }
            }

            return null;
        }
    }

    #region class: TransformsToFrom2D

    /// <summary>
    /// This holds a pair of transforms to take coplanar 3D points into 2D, then back from that 2D plane to the 3D's plane
    /// </summary>
    public class TransformsToFrom2D
    {
        /// <summary>
        /// Transform from 3D to 2D
        /// </summary>
        public Matrix4x4 From3D_To2D { get; set; }
        /// <summary>
        /// How to get a 2D back to 3D
        /// </summary>
        public Matrix4x4 From2D_BackTo3D { get; set; }
    }

    #endregion
}
