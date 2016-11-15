using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace FEDE {
 
  public static class NotificationServices {
    private static Dictionary<string, List<Action<Component>>> notifications = new Dictionary<string, List<Action<Component>>>();
  
    public static void addObserver(string notificationName, Action<Component> action) {
      List<Action<Component>> observers;
      if (!notifications.TryGetValue(notificationName, out observers)) {
        observers = new List<Action<Component>>();
        notifications[notificationName] = observers;
      }
    
      observers.Add(action);
    }
  
    public static void removeObserver(string notificationName, Action<Component> action) {
      List<Action<Component>> observers;
      if (notifications.TryGetValue(notificationName, out observers)) {
        observers.Remove(action);
      }
    }
  
    public static void postNotification(string notificationName, Component sender) {
      List<Action<Component>> observers;
      if (notifications.TryGetValue(notificationName, out observers)) {
        observers = new List<Action<Component>>(observers);
        foreach (Action<Component> notify in observers) {
          notify(sender);
        }
      }
    }
  }

}
