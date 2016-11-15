using UnityEngine;
using System;
using System.Collections;
using FEDE.Utils;

namespace FEDE {
  namespace Revolve {

    public class Vehicle : MonoBehaviour {
      
      public class VehicleControl {
        public float thrust;
        public float roll;
      }
      
      public enum CSAMode {
        Manual,
        ColliderBounds
      }
      
      public static string OnHealthDecreasedNotification = "OnHealthDecreased";
      public static string OnHealthIncreasedNotification = "OnHealthIncreased";
      
      public float engineForce = 1000f;
      public float crossSectionalArea = 1f;
      public Collider vehicleBodyCollider;
      public CSAMode crossAreaMode = CSAMode.Manual;
      public float dragCoefficient = 0.1f;
      public float mass = 500f;
      public Renderer[] wings;
      public ParticleSystem sparks;
      public AudioSource destroySound;
      public readonly VehicleControl control = new VehicleControl();
      public ReflectionProbe reflections;
      public Transform stars;
      
      private float _health;
      private Path path;
      private float dragForce;
      private int laps;
      private float _totalDistanceTraveled;
      private float _speed;
      [SerializeField]
      private float
        _position;
      private float acceleration;
      private float rotation;  
//      private float Kmh;
      
      public float health {
        get {
          return _health;
        }
      
        set {
          if (_health > value) {
            _health = value;
            NotificationServices.postNotification(OnHealthDecreasedNotification, this);
            if (_health == 0f) {
              ImDead();
            }
          }
          else if (_health < value) {
            _health = value;
            NotificationServices.postNotification(OnHealthIncreasedNotification, this);
          }
        }
      }
      
      public float speed {
        get {
          return _speed;
        }
      }
      
      public float terminalSpeed {
        get {
          return Mathf.Pow(2f * engineForce * 0.0393f / dragCoefficient * Utils.Math.AirDensity * crossSectionalArea, 1f / 3f);
        }
      }
      
      
      public float totalDistanceTravelled {
        get {
          return _totalDistanceTraveled;
        }
      }
      
      public float position {
        get {
          return _position;
        }
      }
       
      public void ImDead() {
        destroySound.Play();
        dragCoefficient = 5f;
        control.thrust = 0f;
        control.roll = 0f; 
        GetComponent<Driver>().enabled = false;
        Rigidbody vehicleBody = GetComponentInChildren<Rigidbody>();
        vehicleBody.isKinematic = false;
        vehicleBody.velocity = Vector3.zero;
        vehicleBody.rotation = Quaternion.identity;
        vehicleBody.AddExplosionForce(40f, transform.position, 10f);
        sparks.Play();
      }
      
      void Awake() {
        sparks.Stop();
      }
      
      void Start() {
        path = FindObjectOfType<Path>();
      }
      
      void Update() {
        if (crossAreaMode == CSAMode.ColliderBounds) {
          Vector3 vehicleBounds = vehicleBodyCollider.bounds.extents;
          crossSectionalArea = vehicleBounds.x * vehicleBounds.y;
        }

        // Fd = .5 * Cd * rho * A * v^2
        dragForce = .5f * -dragCoefficient * Utils.Math.AirDensity * crossSectionalArea * _speed * _speed;
        
        // a = F/m = (engine - drag - friction) / m
        float friction = 30f * dragForce;
        acceleration = ((control.thrust * engineForce) + dragForce + friction) / mass;
        
        // v = v + at
        _speed += acceleration * Time.deltaTime;
        _speed = Mathf.Max(0f, _speed);
//        Kmh = _speed * 3.6f;
        
        // s = vt - 1/2at^2
        if (_speed != 0f) {
          float displacement = _speed * Time.deltaTime - (0.5f * acceleration * Time.deltaTime * Time.deltaTime);
          _position += displacement;
          _totalDistanceTraveled += displacement;
          if (_position >= path.length) {
            laps++;
            GetComponentInChildren<AnnouncementsUI>().announce(string.Format("LAP {0}", laps.ToString()), true);
          }
          
          _position %= path.length;
        }

        rotation += 360f * control.roll * Time.deltaTime;
        rotation = Utils.Math.WrapAround(rotation, 360f);
        
        Path.WayPoint w = path[_position];
        transform.forward = w.direction;
        transform.position = w.position;
        transform.Rotate(transform.InverseTransformDirection(w.direction) * rotation);
        
        reflections.transform.rotation = Quaternion.identity;
        stars.transform.rotation = Quaternion.identity;        
      }
    }

  }
}
