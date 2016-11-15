using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
  private static T _instance;
  private static object mutex = new object();
  private static bool applicationIsQuitting = false;
  
  public static T instance {
    get {
      if (applicationIsQuitting) {
        return null;
      }
      
      lock (mutex) {
        if (_instance == null) {
          _instance = FindObjectOfType<T>();
          DontDestroyOnLoad(_instance.gameObject);
        }
        
        return _instance;
      }
    }
  }
  
  public void OnDestroy() {
    applicationIsQuitting = true;
  }
}