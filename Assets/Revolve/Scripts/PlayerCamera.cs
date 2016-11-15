using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using System.Linq;
using System.Collections.Generic;
using System;
using FEDE.Revolve;

namespace FEDE {

  public class PlayerCamera : MonoBehaviour {
  
    public enum ViewPortType {
      ThirdPerson,
      FirstPerson
    }
    
    public static string OnPostFXStateChangeNotification = "OnPostFXStateChange";
    private static bool _postFXEnabled = true;
    private bool viewPortTypeChangeInProgress = false;
    private ViewPortType _viewPortType;
    public BuildCircleMesh gazeCursor;
    public Transform sun;
    public Transform vehicle;
    private Coroutine fadeCoroutine;
    private Action adaptBloom = () => {};
    private float initialBloomThreshold;
    private List<Bloom> blooms;
    private float initialCurveSaturation;
    private List<ColorCorrectionCurves> curves;
    
    void Start() {
      configureCamera();
      NotificationServices.addObserver(Game.OnDisplayModeChangedNotification, configureCamera);
      NotificationServices.addObserver(PlayerCamera.OnPostFXStateChangeNotification, configurePostFX);
    }
    
    void OnDestroy() {
      NotificationServices.removeObserver(Game.OnDisplayModeChangedNotification, configureCamera);
      NotificationServices.removeObserver(PlayerCamera.OnPostFXStateChangeNotification, configurePostFX);
    }
    
    public static bool postFXEnabled {
      get {
        return _postFXEnabled;
      }
    
      set {
        if (_postFXEnabled != value) {
          _postFXEnabled = value;
          NotificationServices.postNotification(OnPostFXStateChangeNotification, null);
        }
      }
    }
    
    private void configureCamera(Component sender = null) {
      Driver driver = FindObjectOfType<Driver>();
      if (driver != null) {
        bool gameIsVR = Game.displayMode == Game.DisplayMode.Stereoscopic;
        driver.vrTiltAsRoll = gameIsVR;
      }
      
      configureCameraRig();
      configurePostFX();
      initializeColorCurves();
      initializeBloomAdaptation();
      reset();
    }
    
    public ViewPortType viewPortType {
      get {
        return _viewPortType;
      }
      
      set {
        if (_viewPortType != value) {
          _viewPortType = value;
          if (!viewPortTypeChangeInProgress) {
            viewPortTypeChangeInProgress = true;
            StartCoroutine(switchViewPortType(_viewPortType));
          }
        }
      }
    }
    
    private IEnumerator switchViewPortType(ViewPortType viewPortType) {
      while (viewPortTypeChangeInProgress) {
        Vector3 position = transform.localPosition;
        if (viewPortType == ViewPortType.FirstPerson) {
          if (position.z < -0.7f) {
            position.z = Mathf.Min(position.z + 0.1f, -0.7f);
            position.y = Mathf.Min(position.y + 0.1f, -3.4f);
          }
          else {
            viewPortTypeChangeInProgress = false;
          }
        }
        else {
          if (position.z > -1.97f) {
            position.z = Mathf.Max(position.z - 0.1f, -1.97f);
            position.y = Mathf.Max(position.y - 0.1f, -3.67f);
          }
          else {
            viewPortTypeChangeInProgress = false;
          }
        }
        
        transform.localPosition = position;
        yield return null;
      }
    }      
      
    public void fade(bool toGray, Action completion = null) {
      if (fadeCoroutine != null) {
        StopCoroutine(fadeCoroutine);
      }
      
      fadeCoroutine = StartCoroutine(animateCurves(toGray, completion));
    }
   
    private IEnumerator animateCurves(bool toGray, Action completion = null) {
      bool done = curves.Count == 0;
      float step = 10f;
      while (!done) {          
        float newSaturation = Mathf.Lerp(curves[0].saturation, (toGray ? 0f : initialCurveSaturation), Time.deltaTime * step);
        if (toGray && newSaturation <= 0.2f) {
          newSaturation = 0.2f;
          done = true;
        }
        else if (!toGray && newSaturation >= initialCurveSaturation) {
          newSaturation = initialCurveSaturation;
          done = true;
        }
   
        curves.ForEach((fx) => fx.saturation = newSaturation);  
        yield return null;
      }
      
      if (completion != null) {
        completion();
      }
    }
    
    public void reset() {
      GazeInputModule gazeModule = FindObjectOfType<GazeInputModule>();
      if (Game.displayMode == Game.DisplayMode.Monoscopic) {
        FindObjectOfType<GazeInputModule>().enabled = false;
        Transform headTransform = FindObjectOfType<CardboardHead>().transform;
        headTransform.localPosition = Vector3.zero;
        headTransform.localRotation = Quaternion.identity;
      }
      else {
        FindObjectOfType<GazeInputModule>().enabled = true;
        Cardboard.SDK.Recenter();
      }
    }
    
    private void configureCameraRig() {
      bool gameIsVR = Game.displayMode == Game.DisplayMode.Stereoscopic;
      Cardboard.SDK.VRModeEnabled = gameIsVR;
        
      CardboardHead vrHead = GetComponentInChildren<CardboardHead>();
      vrHead.trackPosition = gameIsVR;
      vrHead.trackRotation = gameIsVR;
      
      if (gameIsVR && Application.platform == RuntimePlatform.IPhonePlayer) {
        StereoController stereoController = GetComponentInChildren<StereoController>();
        stereoController.directRender = SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2;
      }
    }
    
    private void configurePostFX(Component sender = null) {
      bool gameIsVR = Game.displayMode == Game.DisplayMode.Stereoscopic;
      CardboardHead vrHead = GetComponentInChildren<CardboardHead>();
      Camera monoCamera = vrHead.GetComponentInChildren<Camera>();
      List<PostEffectsBase> monoCamFXs = monoCamera.GetComponents<PostEffectsBase>().ToList<PostEffectsBase>();
      List<Camera> stereoCameras = monoCamera.GetComponentsInChildren<Camera>().Where((c) => c != monoCamera).ToList<Camera>();
      List<List<PostEffectsBase>> stereoCamFXs = new List<List<PostEffectsBase>>();
      stereoCamFXs.Add(stereoCameras[0].GetComponents<PostEffectsBase>().ToList<PostEffectsBase>());
      stereoCamFXs.Add(stereoCameras[1].GetComponents<PostEffectsBase>().ToList<PostEffectsBase>());     
      monoCamFXs.ForEach((fx) => fx.enabled = postFXEnabled && !gameIsVR);
      stereoCamFXs.ForEach((fxList) => fxList.ForEach((fx) => fx.enabled = postFXEnabled && gameIsVR));        
    }
    
    private void initializeColorCurves() {
      curves = GetComponentsInChildren<ColorCorrectionCurves>().Where((fx) => fx.enabled).ToList<ColorCorrectionCurves>();
      if (curves.Count > 0) {
        initialCurveSaturation = curves[0].saturation;
      }
    }
    
    private void initializeBloomAdaptation() {
      blooms = GetComponentsInChildren<Bloom>().Where((fx) => fx.enabled).ToList<Bloom>();
      if (blooms.Count > 0 && vehicle != null && sun != null) {
        initialBloomThreshold = blooms[0].bloomThreshold;
        adaptBloom = () => {
          float bloomFactor = Vector3.Dot(vehicle.forward, sun.forward);
          bloomFactor = ((bloomFactor * -1f) + 1f) / 2f;
          float finalThreshold = initialBloomThreshold + (initialBloomThreshold * bloomFactor);
          blooms.ForEach((fx) => fx.bloomThreshold = finalThreshold);
        };
      }
      else {
        adaptBloom = () => {};
      }     
    }
    
    void Update() {
      adaptBloom();
    }
  }
}
