using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using FEDE.Revolve;

public class HUDButton : MonoBehaviour {

  private enum ProgressState : byte {
    Idle,
    Increasing,
    Decreasing
  }
  
  public BuildCircleMesh circle;
  private float increaseRate;
  private float decreaseRate;
  private ProgressState progressState = ProgressState.Idle;
  private float angle;
  public bool workInVRPauseMode;
  
  void Start() {
    circle.endAngle = 0f;
    increaseRate = 2f;
    decreaseRate = -3f;
  }
  
  public void onPointerEnter() {
    if (workInVRPauseMode && 
      Game.displayMode == Game.DisplayMode.Stereoscopic && 
      Game.currentGame.state == Game.State.Paused) {
      circle.endAngle = 360f;
      dispatchEventIfNeeded();
    }
    else {
      ProgressState previousState = progressState;
      progressState = ProgressState.Increasing;
      if (previousState == ProgressState.Idle) {
        StartCoroutine(updateButtonProgress()); 
      }
    }
  }
  
  public void onPointerExit() {
    if (Time.timeScale == 1f) {
      ProgressState previousState = progressState;
      progressState = ProgressState.Decreasing;
      if (previousState == ProgressState.Idle) {
        StartCoroutine(updateButtonProgress()); 
      }
    }
  }
  
  IEnumerator updateButtonProgress() {
    while (progressState != ProgressState.Idle) {
      if (progressState == ProgressState.Increasing) {
        angle += 360f * Time.deltaTime * increaseRate;
      }
      else if (progressState == ProgressState.Decreasing) {
        angle += 360f * Time.deltaTime * decreaseRate;
      }
      
      angle = Mathf.Clamp(angle, 0f, 360f);
      circle.endAngle = angle;
      
      if (circle.endAngle == 360f || circle.endAngle == 0f) {
        progressState = ProgressState.Idle;
      }      
      yield return null;
    }
    
    dispatchEventIfNeeded();
  }
  
  private void dispatchEventIfNeeded() {
    if (circle.endAngle == 360f && Cardboard.SDK.VRModeEnabled) {
      PointerEventData data = new PointerEventData(EventSystem.current);
      ExecuteEvents.Execute(this.gameObject, data, ExecuteEvents.submitHandler);
    }
  }
}
