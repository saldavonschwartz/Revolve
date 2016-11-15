using UnityEngine;
using UnityStandardAssets.ImageEffects;
using System.Collections;

namespace FEDE {
  namespace Revolve {
  
    public class Trap : MonoBehaviour {
      public float rotationSpeed = 0.5f;
      public bool addComponents = true;
      public bool applyRotation = false;
      private float rotation = 0f;
            
      void OnBecameVisible() {
        applyRotation = true;
      }
      
      void OnBecameInvisible() {
        applyRotation = false;
      }
      
      void Start() {
        gameObject.layer = LayerMask.NameToLayer("Trap");
        if (addComponents) {
          setupColliderFromTemplate();
          setupRigidBody();
          setupAudioSourceFromTemplate(); 
        }        
      }
	
      void Update() {
        if (applyRotation) {
          transform.Rotate(Vector3.forward, (360f * Time.deltaTime * rotationSpeed) + rotation);
          rotation = 0f;
        }
        else {
          rotation += (360f * Time.deltaTime * rotationSpeed);
          rotation %= 360f;
        }
      }
  
      void OnTriggerEnter(Collider other) {
        Vehicle vehicle = other.transform.parent.GetComponent<Vehicle>();
        if (vehicle != null && Game.currentGame.state == Game.State.Playing) {
          vehicle.health = 0f;
        }
      }
  
      void setupColliderFromTemplate() {
        BoxCollider templateCollider = gameObject.GetComponentInParent<BoxCollider>();
        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        Vector3 colliderSize = templateCollider.size;
        Vector3 colliderCenter = templateCollider.center;
        colliderCenter.z = collider.center.z;
        collider.center = colliderCenter;
        collider.size = colliderSize;
        collider.isTrigger = true;
      }
  
      void setupRigidBody() {
        Rigidbody rigidBody = gameObject.AddComponent<Rigidbody>();
        rigidBody.isKinematic = true;
        rigidBody.useGravity = false;
        rigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
      }
  
      void setupAudioSourceFromTemplate() {
        AudioSource audioTemplate = gameObject.GetComponentInParent<AudioSource>();
        AudioSource audio = gameObject.AddComponent<AudioSource>();
        audio.clip = audioTemplate.clip;
        audio.spatialBlend = audioTemplate.spatialBlend;
        audio.panStereo = audioTemplate.panStereo;
        audio.loop = audioTemplate.loop;
        audio.pitch = audioTemplate.pitch;
        audio.maxDistance = audioTemplate.maxDistance;
        audio.dopplerLevel = audioTemplate.dopplerLevel;
        audio.outputAudioMixerGroup = audioTemplate.outputAudioMixerGroup;
        audio.Play();
      }
    }

  }
}
