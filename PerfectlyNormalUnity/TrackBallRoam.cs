using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PerfectlyNormalUnity
{
    //TODO: Double tap shift key to toggle between orbit and 1st person look (reset the shift first tap if they click the mouse in between to reduce the chance of toggling while they are doing standard shift+right drag)

    /// <summary>
    /// This allows the user to move the camera around with left/right/middle drag, wheel mouse, keyboard.
    /// Attach this script to a camera
    /// </summary>
    /// <remarks>
    /// Attach this script to your camera
    /// </remarks>
    public class TrackBallRoam : MonoBehaviour
    {
        private float MouseSensitivity_Pan = 240f;
        private float MouseSensitivity_Orbit = 360f;
        private float MouseSensitivity_Wheel = 120f;

        private float OrbitRadius = 6;
        private float MaxOrbitRadius = 144f;

        private float MinAngle_Y = -90f;
        private float MaxAngle_Y = 90f;

        private float _eulerX;
        private float _eulerY;

        private bool _isLookAtPointSet = false;
        private Vector3 _lookAtPoint = new Vector3();

        private DebugRenderer3D _debug = null;
        private DebugItem _cameraRight = null;
        private DebugItem _cameraUp = null;

        private List<DebugItem> _orbitRayVisuals = new List<DebugItem>();

        void Start()
        {
            _debug = gameObject.AddComponent<DebugRenderer3D>();

            _cameraRight = _debug.AddLine_Pipe(new Vector3(), new Vector3(), .1f, Color.red);
            _cameraUp = _debug.AddLine_Pipe(new Vector3(), new Vector3(), .1f, Color.green);




            Vector3 angles = transform.eulerAngles;
            _eulerX = angles.y;
            _eulerY = angles.x;



        }

        void Update()
        {
            // Right Drag: Orbit
            // Shift + Right Drag : Orbit with ray for radius

            // Middle Drag: Pan
            // Shift + Middle Drag: Auto Pan        // might not need this, there will be inertia

            // Wheel: Zoom


            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            float mouseWheel = Input.mouseScrollDelta.y * MouseSensitivity_Wheel;

            bool isRightDown = Input.GetMouseButton(1);
            bool isMiddleDown = Input.GetMouseButton(2);

            bool isShiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);


            // Mouse wheel will act directly on position, it won't affect the velocity
            if (!Mathf.Approximately(mouseWheel, 0f))
            {
                transform.position += transform.forward * (mouseWheel * Time.deltaTime);
            }

            if (isMiddleDown)
            {
                transform.position -=
                    (transform.up * (mouseY * MouseSensitivity_Pan * Time.deltaTime)) +
                    (transform.right * (mouseX * MouseSensitivity_Pan * Time.deltaTime));
            }

            OrbitCamera(isRightDown, isShiftDown, mouseX, mouseY);

            DebugRenderer3D.AdjustLinePositions(_cameraRight, new Vector3(), transform.right);
            DebugRenderer3D.AdjustLinePositions(_cameraUp, new Vector3(), transform.up);
        }

        private void OrbitCamera_DEBUG(bool isRightDown, bool isShiftDown, float mouseX, float mouseY)
        {
            if (isRightDown)
            {
                _eulerX += mouseX * MouseSensitivity_Orbit * Time.deltaTime;     // the example multiplies this by orbit radius, but not Y
                _eulerY -= mouseY * MouseSensitivity_Orbit * Time.deltaTime;

                _eulerY = ClampAngle(_eulerY, MinAngle_Y, MaxAngle_Y);

                Quaternion rotation = Quaternion.Euler(_eulerY, _eulerX, 0);

                Vector3 lookAtPoint;
                float radius;
                if (_isLookAtPointSet)
                {
                    lookAtPoint = _lookAtPoint;
                    radius = (transform.position - lookAtPoint).magnitude;
                }
                else
                {
                    lookAtPoint = transform.position + (transform.forward * OrbitRadius);
                    radius = OrbitRadius;
                }

                if (isShiftDown && !_isLookAtPointSet)
                {
                    _debug.Remove(_orbitRayVisuals);
                    _orbitRayVisuals.Clear();

                    _orbitRayVisuals.Add(_debug.AddDot(transform.position, .09f, Color.white));
                    _orbitRayVisuals.Add(_debug.AddLine_Basic(transform.position, transform.position + (transform.forward * MaxOrbitRadius), .03f, Color.white));

                    Ray lookRay = new Ray(transform.position, transform.forward);

                    RaycastHit[] coneHits = UtilityUnity.ConeCastAll(lookRay, radius, MaxOrbitRadius, 12);

                    if (coneHits.Length > 0)
                    {
                        var coneHits2 = coneHits.
                            Select(o => new
                            {
                                hit = o,
                                lineIntersect = Math3D.GetClosestPoint_Line_Point(lookRay, o.point),        // this finds the closest point on the look ray, perpendicular to the ray
                                planeIntersect = Math3D.GetIntersection_Plane_Line(new Plane(lookRay.origin - o.point, o.point), lookRay),      // this finds a point on the look ray, perpendicular to the cone match ray
                            }).
                            Where(o => o.planeIntersect != null).       // it should never be null
                            Select(o => new
                            {
                                o.hit,
                                o.lineIntersect,
                                planeIntersect = o.planeIntersect.Value,
                                lineIntersectDist = (o.lineIntersect - lookRay.origin).magnitude,
                                planeIntersectDist = (o.planeIntersect.Value - lookRay.origin).magnitude,
                            }).
                            ToArray();



                        //TODO: This is unreliable, 
                        //var closest = coneHits.
                        //    OrderBy(o => o.distance).
                        //    First();




                        var closest2 = coneHits2.
                            OrderBy(o => o.planeIntersectDist).
                            First();






                        //TODO: Intersect closest.point with the ray coming straight out of the camera.  That should be the look at point



                        radius = closest2.planeIntersectDist;
                        lookAtPoint = closest2.planeIntersect;
                        _lookAtPoint = lookAtPoint;
                        _isLookAtPointSet = true;


                        foreach (var hit in coneHits2)
                        {
                            Color hitColor = hit.planeIntersectDist.IsNearValue(closest2.planeIntersectDist) ?
                                Color.green :
                                Color.red;

                            _orbitRayVisuals.Add(_debug.AddDot(hit.hit.point, .09f, hitColor));
                            _orbitRayVisuals.Add(_debug.AddLine_Basic(transform.position, hit.hit.point, .03f, hitColor));

                            _orbitRayVisuals.Add(_debug.AddDot(hit.planeIntersect, .06f, hitColor));
                            _orbitRayVisuals.Add(_debug.AddDot(hit.lineIntersect, .04f, hitColor));

                        }


                    }
                }

                Vector3 negRadius = new Vector3(0.0f, 0.0f, -radius);
                Vector3 position = rotation * negRadius + lookAtPoint;

                transform.rotation = rotation;
                transform.position = position;
            }
            else
            {
                _isLookAtPointSet = false;
            }
        }

        /// <summary>
        /// Orbits around a point that is OrbitRadius away.  If they are holding in shift, it will
        /// fire a cone ray and orbit around what they are looking at
        /// </summary>
        /// <remarks>
        /// Got this here
        /// http://wiki.unity3d.com/index.php?title=MouseOrbitImproved#Code_C.23
        /// </remarks>
        private void OrbitCamera(bool isRightDown, bool isShiftDown, float mouseX, float mouseY)
        {
            if (isRightDown)
            {
                _eulerX += mouseX * MouseSensitivity_Orbit * Time.deltaTime;     // the example multiplies this by orbit radius, but not Y
                _eulerY -= mouseY * MouseSensitivity_Orbit * Time.deltaTime;

                _eulerY = ClampAngle(_eulerY, MinAngle_Y, MaxAngle_Y);

                Quaternion rotation = Quaternion.Euler(_eulerY, _eulerX, 0);

                Vector3 lookAtPoint;
                float radius;
                if (_isLookAtPointSet)
                {
                    lookAtPoint = _lookAtPoint;
                    radius = (transform.position - lookAtPoint).magnitude;
                }
                else
                {
                    lookAtPoint = transform.position + (transform.forward * OrbitRadius);
                    radius = OrbitRadius;
                }

                if (isShiftDown && !_isLookAtPointSet)
                {
                    Ray lookRay = new Ray(transform.position, transform.forward);

                    var coneHits = UtilityUnity.ConeCastAll(lookRay, radius, MaxOrbitRadius, 12).
                        Select(o => new
                        {
                            hit = o,
                            //intersect = Math3D.GetClosestPoint_Line_Point(lookRay, o.point),        // this finds the closest point on the look ray, perpendicular to the look ray
                            intersect = Math3D.GetIntersection_Plane_Line(new Plane(lookRay.origin - o.point, o.point), lookRay),      // this finds a point on the look ray, perpendicular to the cone match ray (using this because it gives a better indication of what is "closest".  something that is closer to the camera, but higher angle from the look ray could project to be farther away than something that is lower angle, but slightly farther from the camera)
                        }).
                        Where(o => o.intersect != null).       // it should never be null
                        Select(o => new
                        {
                            o.hit,
                            intersect = o.intersect.Value,
                            distance = (o.intersect.Value - lookRay.origin).sqrMagnitude,
                        }).
                        ToArray();

                    if (coneHits.Length > 0)
                    {
                        var closest = coneHits.
                            OrderBy(o => o.distance).
                            First();

                        radius = (float)Math.Sqrt(closest.distance);
                        lookAtPoint = closest.intersect;
                        _lookAtPoint = lookAtPoint;
                        _isLookAtPointSet = true;
                    }
                }

                Vector3 negRadius = new Vector3(0.0f, 0.0f, -radius);
                Vector3 position = rotation * negRadius + lookAtPoint;

                transform.rotation = rotation;
                transform.position = position;
            }
            else
            {
                _isLookAtPointSet = false;
            }
        }

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f)
                angle += 360f;
            if (angle > 360f)
                angle -= 360f;
            return Mathf.Clamp(angle, min, max);
        }
    }
}
