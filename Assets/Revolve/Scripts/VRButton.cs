using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;

namespace FEDE {
  namespace Revolve {
  
    public class VRButton : MonoBehaviour {

      public float actionDelay = 1f;
      private Coroutine delayedAction;
      public bool worksinPause = false;
      
      public void onPointerEnter() {
        if (Game.displayMode == Game.DisplayMode.Stereoscopic) {
          if (worksinPause && Game.currentGame.state == Game.State.Paused) {
            dispatchClickEvent();
          }
          else {
            delayedAction = StartCoroutine(performActionDelayed(actionDelay, dispatchClickEvent));
          }
        }
      }
  
      public void onPointerExit() {
        if (Game.displayMode == Game.DisplayMode.Stereoscopic && delayedAction != null) {
          endDelayedAction();
        }
      }
  
      private IEnumerator performActionDelayed(float delay, Action action) {
        float interval = 360f / delay;
        BuildCircleMesh gazeCursor = FindObjectOfType<PlayerCamera>().gazeCursor;    
        gazeCursor.endAngle = 360f;
        while (gazeCursor.endAngle > 0f && Game.displayMode == Game.DisplayMode.Stereoscopic) {
          gazeCursor.endAngle -= interval * Time.deltaTime;
          yield return null;
        }
    
        if (Game.displayMode == Game.DisplayMode.Stereoscopic) {
          action();
        }
        else {
          endDelayedAction();
        }
      }
  
      private void dispatchClickEvent() {
        PointerEventData data = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(this.gameObject, data, ExecuteEvents.submitHandler);
      }
  
      private void endDelayedAction() {
        StopCoroutine(delayedAction);
        delayedAction = null;
        BuildCircleMesh gazeCursor = FindObjectOfType<PlayerCamera>().gazeCursor;    
        gazeCursor.endAngle = Game.displayMode == Game.DisplayMode.Stereoscopic ? 360f : 0f;
      }
    }

  }
}
