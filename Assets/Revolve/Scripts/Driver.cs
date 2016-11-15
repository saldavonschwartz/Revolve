using UnityEngine;
using System.Collections;

namespace FEDE {
  namespace Revolve {
    public class Driver : MonoBehaviour {
      public PlayerCamera playerCamera;
      public Vehicle vehicle;
      public bool autoThrust = true;
      public float rollRange = 85f;
      public float yawRange = 25f;
      public float sensitivity = 1.2f;
      public bool vrTiltAsRoll = true;
      
      void OnEnable() {
        recenterCameraIfNeeded();
      }
      
      public void recenterCameraIfNeeded() {
        if (playerCamera != null) {
          playerCamera.transform.parent = transform;
          playerCamera.transform.localPosition = new Vector3(0f, -3.67f, -1.97f);
          vehicle.control.thrust = 0f;
          vehicle.control.roll = 0f;
          if (Game.displayMode == Game.DisplayMode.Monoscopic) { 
            Cardboard.SDK.Recenter();
          }
        }
      }
      
      void Update() {
        vehicle.control.thrust = autoThrust ? 1f : 0f;
        
        float angle;
        Vector3 axis;
        Cardboard.SDK.HeadPose.Orientation.ToAngleAxis(out angle, out axis);
        float roll = angle * axis.z;
        float yaw = angle * axis.y;
        
        if (!vrTiltAsRoll || Mathf.Abs(yaw) <= yawRange) {
          roll = Mathf.Clamp(roll / rollRange, -1f, 1f);
          vehicle.control.roll = roll * sensitivity;  
        }
      }
    }
    
  }
}
