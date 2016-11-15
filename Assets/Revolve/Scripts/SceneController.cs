using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections.Generic;

namespace FEDE {
  namespace Revolve {
  
    public class SceneController : BaseBehaviour {
      public Animator transitionAnimator;
      protected bool continueTransition;
      private bool _userInteractionEnabled = true; 
      public bool userInteractionEnabled {
        get {
          return _userInteractionEnabled;
        }
        
        set {
          if (_userInteractionEnabled != value) {
            _userInteractionEnabled = value;
            List<Button> buttons = FindObjectsOfType<Button>().ToList<Button>();
            buttons.ForEach((b) => b.interactable = _userInteractionEnabled);
          }
        }
      }
      
      public void transition(string sceneName, Action completion = null) {
        userInteractionEnabled = false;
        GazeInputModule gazeModule = FindObjectOfType<GazeInputModule>();
        GameObject cursor = gazeModule.cursor;
        gazeModule.cursor = null;
        cursor.SetActive(false);
        StartCoroutine(_transition(sceneName, completion));
      }
      
      private IEnumerator _transition(string sceneName, Action completion = null) {
        transitionAnimator.Play("ZoomIn", -1, 0f);
        yield return new WaitForSeconds(1f);
        
        continueTransition = completion == null;
        if (!continueTransition) {
          completion();
        }
        
        while (!continueTransition) {
          yield return null;
        }
        
        StartCoroutine(loadScene(sceneName));
      }
  
      private IEnumerator loadScene(string sceneName) {
        AsyncOperation loading = Application.LoadLevelAsync(sceneName);
        yield return loading;
      }      
    }

  }
}
