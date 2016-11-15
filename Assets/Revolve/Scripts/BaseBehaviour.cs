
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;



namespace FEDE {

  public class BaseBehaviour : MonoBehaviour {
  
    public enum DispatchMode {
      Background,
      Foreground
    }
  
    protected bool logEnabled = true;
    public static bool GlobalLogEnabled = true;
    
    protected Queue<Action> syncQueue = new Queue<Action>();
    
    protected void dispatch(DispatchMode mode, Action action) {
      if (mode == DispatchMode.Foreground) {
        lock (syncQueue) {
          syncQueue.Enqueue(action);
        }
      }
      else {
        Thread backgroundThread = new Thread(() => action());
        backgroundThread.IsBackground = true;
        backgroundThread.Start();
      }
    }
    
    protected void dispatchOnce(ref bool done, Action action) {
      if (!done) {
        done = true;
        action();
      }
    }
    
    protected virtual void Update() {
      Queue<Action> dispatchQueue;
      lock (syncQueue) {
        dispatchQueue = new Queue<Action>(syncQueue);
        syncQueue.Clear();
      }
      
      while (dispatchQueue.Count > 0) {
        dispatchQueue.Dequeue()();
      }
    }
    
    protected void Log(object argument) {
      if (GlobalLogEnabled && logEnabled) {
        Debug.Log(argument);
      }  
    }
  }  
  
}
