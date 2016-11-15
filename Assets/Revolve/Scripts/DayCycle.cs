using UnityEngine;
using System.Collections;

public class DayCycle : MonoBehaviour {
  public enum DayPhase {
    Day,
    Night
  }

  public float dayCycleSpeed = 1f;
  public bool animating = false;
  public Light sun;
  public ParticleSystem stars;
  public Gradient fogColor;
  public AnimationCurve fogIntensity;
  private float sunIntensityConstant;
  private float sunRotationDegrees;
  public float sunLightHorizon = -0.2f;
  public bool starsAlwaysVisible = false;
  
  void Start() {
    sunIntensityConstant = sun.intensity;
    if (starsAlwaysVisible) {
      stars.Play();
    }
  }
  
  void Update() {
    updateSun();
    updateAtmosphere();
  }

  private void updateSun() {
    if (animating) {
      sun.transform.Rotate(Vector3.right * step);
    }
    
    float sunPhaseOffset = Mathf.Clamp01((sunPhase - sunLightHorizon) / (1f - sunLightHorizon));
    sun.intensity = sunIntensityConstant * sunPhaseOffset;
  }
  
  private void updateAtmosphere() {
    float sunPhasePercent = sunRotationDegrees / 360f;
    sunRotationDegrees = (sunRotationDegrees + step) % 360f;
    RenderSettings.fogColor = fogColor.Evaluate(sunPhasePercent);
    if (!starsAlwaysVisible) {
      if (sun.intensity >= 0.3f) {
        if (stars.isPlaying) {
          stars.Stop();
        }
      }
      else {
        if (stars.isStopped) {
          stars.Play();
        }
      }
    }
  }

  public DayPhase dayPhase {
    get {
      return sunPhase > 0f ? DayPhase.Day : DayPhase.Night;
    }
  }

  private float sunPhase {
    get {
      return Vector3.Dot(sun.transform.forward, Vector3.down);
    }
  }


  private float step {
    get {
      return Time.deltaTime * 360f / dayCycleSpeed;
    }
  }
}
