using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace geniikw.DataRenderer2D
{
    /// <summary>
    /// to draw joint, Define IEnumerable<Triple>
    /// </summary>
    public partial struct Spline 
    {
        public IEnumerable<Triple> TripleList
        {
            get
            {
                if (GetCount() < 2)
                    yield break;

                var mode = option.mode;
                var sr = option.startRatio;
                var er = option.endRatio;
                var color = option.color;
                            
                              
                var len = AllLength;
                var lenStart = sr * len;
                var lenEnd = er * len;
                var c = 0f;

                var pointF = Point.Zero;
                var firstTimeForF = true;
                var pointS = Point.Zero;
                var firstTimeForS = true;

                var index = 0;
                foreach(var p in TripleEnumerator())
                {                    
                    if (firstTimeForF)
                    {
                        firstTimeForF = false;
                        pointF = p;
                        continue;
                    }
                    if (firstTimeForS)
                    {
                        if (mode == LineOption.Mode.Loop && sr == 0f && er == 1f)
                            yield return new Triple(GetLastPoint(), GetFirstPoint(), p, color.Evaluate(0));

                        firstTimeForS = false;
                        pointS = p;
                        continue;
                    }
                    
                    c += CurveLength.Auto(pointF, pointS);
                    if (lenStart < c && c < lenEnd)
                    {
                        if (index == GetCount() - 1 && mode != LineOption.Mode.Loop)
                            break;
                        
                        yield return new Triple(pointF, pointS, p,color.Evaluate(c/len));
                    }
                    pointF = pointS;
                    pointS = p;
                    index++;
                    Debug.Log("This triple spline is being called");
                }
            }
        }

        public struct Triple
        {
            Point previous;
            Point target;
            Point next;
            Color color;

            public Triple(Point p, Point c, Point n, Color cl)
            {
                previous = p; target = c; next = n; color = cl;
            }

            public Vector3 ForwardDirection {
                get
                {
                    return Curve.AutoDirection(target, next, 0f);
                }
            }
            public Vector3 BackDirection
            {
                get
                {
                    return Curve.AutoDirection(previous, target, 1f);
                }
            }
            public Vector3 Position
            {
                get
                {
                    Debug.Log($"Returning a points positon at: {target.position}");
                    return target.position;
                }
            }
            public float CurrentWidth
            {
                get
                {
                    return target.width;
                }
            }
            public Color CurrentColor
            {
                get
                {
                    return color;
                }
            }
        }
    }
}