using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FEDE {
  namespace Revolve {
  
    [ExecuteInEditMode]
    public class Path : MonoBehaviour {     
    
      public enum InterpolationMode {
        Realtime,
        Waypoints
      }
      
      public int integralSamples = 600;
      public int rootIterations = 20;
      public float errorTolerance = Mathf.Pow(10f, -7f);
      public int actualSamples = 0;
      public int wayPointCount = 10;
      public bool alwaysDrawGizmos = true;
      public InterpolationMode interpolationMode;
      
      private List<WayPoint> wayPoints;
      private List<float> arcLengths = new List<float>();

      public struct WayPoint {
        public Vector3 position;
        public Vector3 direction;
        
        public WayPoint(Vector3 position, Vector3 direction) {
          this.position = position;
          this.direction = direction;
        }
      }

      public WayPoint this[float displacement] {
        get {
          Vector3 newPosition;
          Vector3 newDirection;

          if (interpolationMode == InterpolationMode.Realtime) {
            float t = curveParameterFromDistance(displacement);
            newPosition = position(t);
            newDirection = direction(t);
          }
          else {
            float diff = displacement / length * wayPoints.Count;
            int nextWayPointIndex = Mathf.CeilToInt(diff);
            float segmentProgress = nextWayPointIndex - diff;
            int currentWayPointIndex = (nextWayPointIndex - 1);
            nextWayPointIndex = Utils.Math.WrapAround(nextWayPointIndex, wayPoints.Count);
            currentWayPointIndex = Utils.Math.WrapAround(currentWayPointIndex, wayPoints.Count);
            WayPoint currentWayPoint = wayPoints[currentWayPointIndex];
            WayPoint nextWayPoint = wayPoints[nextWayPointIndex];
            newPosition = Vector3.Lerp(currentWayPoint.position, nextWayPoint.position, 1f - segmentProgress);
            newDirection = Vector3.Lerp(currentWayPoint.direction, nextWayPoint.direction, 1f - segmentProgress);
          }

          return new WayPoint(newPosition, newDirection);
        }
      }

      [SerializeField]
      private float
        _length;

      public float length { 
        get {
          return _length;
        }

        private set {
          _length = value;
        }
      }

      void Start() {
        length = arcLength(1f);
        if (Application.isPlaying && interpolationMode == InterpolationMode.Waypoints) {
          wayPoints = new List<WayPoint>();
          float step = length / wayPointCount;
          interpolationMode = InterpolationMode.Realtime;
          for (float i = 0; i < wayPointCount; i ++) {
            WayPoint w = this[step * i];
            wayPoints.Add(w);
          }
                    
          interpolationMode = InterpolationMode.Waypoints;
        }
      }

      private Vector3 position(float t) {
        int i = 0;
        localCurveP0AndTFromGlobalT(ref t, out i);
        
        float oneMinusT = 1f - t;
        Vector3 t1 = oneMinusT * oneMinusT * oneMinusT * localPoint(i);
        Vector3 t2 = 3f * oneMinusT * oneMinusT * t * localPoint(i + 1);
        Vector3 t3 = 3f * oneMinusT * t * t * localPoint(i + 2);
        Vector3 t4 = t * t * t * localPoint((int)((i + 3) % pointCount));
        return  transform.TransformPoint(t1 + t2 + t3 + t4);
      }
      
      private Vector3 velocity(float t) {
        int i = 0;
        localCurveP0AndTFromGlobalT(ref t, out i);
        
        float oneMinusT = 1f - t;
        Vector3 t1 = 3f * oneMinusT * oneMinusT * (localPoint(i + 1) - localPoint(i));
        Vector3 t2 = 6f * oneMinusT * t * (localPoint(i + 2) - localPoint(i + 1));
        Vector3 t3 = 3f * t * t * (localPoint((int)((i + 3) % pointCount)) - localPoint(i + 2));
        return transform.TransformPoint(t1 + t2 + t3);
      }
      
      private Vector3 direction(float t) {
        return velocity(t).normalized;
      }
      
      private float speed(float t) {
        return velocity(t).magnitude;
      }
      
      private Vector3 localPoint(int i) {
        return transform.GetChild(i).localPosition;
      }
      
      private Vector3 worldPoint(int i) {
        return transform.GetChild(i).position;
      }
      
      private int pointCount {
        get {
          return transform.childCount;
        }
      }

      private void localCurveP0AndTFromGlobalT(ref float t, out int p0) {
        p0 = 0;
        if (t >= 1f) {
          t = 1f;
          p0 = pointCount - 4;
        }
        else {
          t = Mathf.Clamp01(t) * ((pointCount) / 3);
          p0 = (int)t;
          t -= p0;
          p0 *= 3;
        }
      }
      
      private float globalTFromPoint(int p) {
        return (p / 3f) / (pointCount / 3f);
      }
      
      private float arcLength(float t) {
        int p0 = 0;
        float localT = t;
        float result = 0f;
        float lowerBound = 0f;
        float upperBound = 0f;
        localCurveP0AndTFromGlobalT(ref localT, out p0);
        int targetCurveIdx = Mathf.CeilToInt(p0 / 3f);
        float evenSamples;
        for (int i = 0; i < targetCurveIdx; i++) {
          if (i == arcLengths.Count) {
            lowerBound = globalTFromPoint(3 * i);
            upperBound = globalTFromPoint(3 * (i + 1));
            evenSamples = integralSamples * (upperBound - lowerBound);
            evenSamples = evenSamples - evenSamples % 2;
            evenSamples = Mathf.Max(2f, evenSamples);
            actualSamples = (int)evenSamples;
            arcLengths.Add(Utils.Math.Integral(speed, lowerBound, upperBound, (uint)evenSamples));
          }
          
          result += arcLengths[i];
        }
        
        lowerBound = globalTFromPoint(3 * targetCurveIdx);
        upperBound = t;
        evenSamples = integralSamples * (upperBound - lowerBound);
        evenSamples = evenSamples - evenSamples % 2;
        evenSamples = Mathf.Max(2f, evenSamples);
        actualSamples = (int)evenSamples;
        result += Utils.Math.Integral(speed, lowerBound, upperBound, (uint)evenSamples);
        
        return result;
      }
      
      private float curveParameterFromDistance(float distance) {
        if (distance == 0f || distance == length) {
          return distance;
        }
        else {
          return Utils.Math.FindRoot((x) => arcLength(x) - distance, speed, distance / length, errorTolerance, (uint)rootIterations);
        }
      }

#if UNITY_EDITOR
      void OnDrawGizmos() {
        if (alwaysDrawGizmos) {
          for (int i = 0; i <= transform.childCount - 3; i += 3) {
            Vector3 p0 = worldPoint(i);
            Vector3 p1 = worldPoint(i + 1);
            Vector3 p2 = worldPoint(i + 2);
            Vector3 p3 = worldPoint((i + 3) % pointCount);
            Handles.DrawBezier(p0, p3, p1, p2, Color.cyan, null, 1f);
          }
          
          if (Application.isPlaying && interpolationMode == InterpolationMode.Waypoints) {
            foreach (WayPoint w in wayPoints) {
              Gizmos.DrawWireCube(w.position, Vector3.one * .1f);
            }
          }
        }
      }

      void OnDrawGizmosSelected() {
        if (!alwaysDrawGizmos) {
          for (int i = 0; i <= pointCount - 3; i += 3) {
            Vector3 p0 = worldPoint(i);
            Vector3 p1 = worldPoint(i + 1);
            Vector3 p2 = worldPoint(i + 2);
            Vector3 p3 = worldPoint((i + 3) % pointCount);
            Handles.DrawBezier(p0, p3, p1, p2, Color.cyan, null, 1f);
          }
          
          if (Application.isPlaying && interpolationMode == InterpolationMode.Waypoints) {
            foreach (WayPoint w in wayPoints) {
              Gizmos.DrawWireCube(w.position, Vector3.one * .1f);
            }
          }
        }
      }
#endif
    }

  }
}
