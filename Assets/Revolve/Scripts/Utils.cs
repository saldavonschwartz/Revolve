using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using UnityEngine.UI;

namespace FEDE {
  namespace Utils {
  
    public class Loader : MonoBehaviour {
      private Dictionary<string, object> assetBundles = new Dictionary<string, object>();
      private static Loader _SharedInstance;
      private static Loader SharedInstance {
        get {
          if (_SharedInstance == null) {
            _SharedInstance = new GameObject("Loader").AddComponent<Loader>();
            DontDestroyOnLoad(_SharedInstance);
          }
          
          return _SharedInstance;
        }
      }
      
      public static void LoadAssetFromBundleAsync(AssetBundle bundle, string assetName, int idx, Action<AssetBundleRequest, int> completion) {
        SharedInstance.StartCoroutine(SharedInstance.loadAssetFromBundleAsync(bundle, assetName, (assetRequest) => completion(assetRequest, idx)));
      }
      
      private IEnumerator loadAssetFromBundleAsync(AssetBundle bundle, string assetName, Action<AssetBundleRequest> completion) {
        AssetBundleRequest assetRequest;
        if (assetName != null) {
          assetRequest = bundle.LoadAssetAsync<GameObject>(assetName);
        }
        else {
          assetRequest = bundle.LoadAllAssetsAsync<GameObject>();
        }
        
        yield return assetRequest;
        completion(assetRequest);
      }
      
      public static void LoadAssetBundleAsync(string assetBundlePath, int idx, Action<AssetBundle, int> completion) {
        SharedInstance.StartCoroutine(SharedInstance.loadAssetBundleAsync(assetBundlePath, (assetBundle) => completion(assetBundle, idx)));
      }
      
      private IEnumerator loadAssetBundleAsync(string assetBundlePath, Action<AssetBundle> completion) {
        while (!Caching.ready) {
          yield return null;
        }
        
        string platformAssetBundlePath;
        if (Application.platform == RuntimePlatform.Android) {
          platformAssetBundlePath = assetBundlePath;
        }
        else {
          platformAssetBundlePath = "file://" + assetBundlePath;
        }
        
        object assetBundle;
        if (!SharedInstance.assetBundles.TryGetValue(platformAssetBundlePath, out assetBundle)) {
          assetBundle = WWW.LoadFromCacheOrDownload(platformAssetBundlePath, 1);
          SharedInstance.assetBundles[platformAssetBundlePath] = assetBundle;
          yield return assetBundle as WWW;
        }
        
        WWW request = assetBundle as WWW;
        if (request != null) {
          if (!request.isDone) {
            yield return request;
          }
          
          assetBundle = SharedInstance.assetBundles[platformAssetBundlePath];
          if (request == assetBundle) {
            assetBundle = request.assetBundle;
            SharedInstance.assetBundles[platformAssetBundlePath] = assetBundle;
            request.Dispose();
          }
        }
        
        completion(assetBundle as AssetBundle);
      }
      
      public static void Release(AssetBundle assetBundle) {
        string key = SharedInstance.assetBundles.FirstOrDefault((i) => i.Value == assetBundle).Key;
        SharedInstance.assetBundles.Remove(key);
        assetBundle.Unload(false);
        assetBundle = null;
      }
      
      public static void ReleaseAllAssetBundles() {
        foreach (AssetBundle assetBundle in SharedInstance.assetBundles.Values) {
          assetBundle.Unload(false);
        }
        
        SharedInstance.assetBundles.Clear();
      } 
      
      public static void Dispose() {
        DestroyImmediate(SharedInstance.gameObject);
      }
    }
    
    namespace Extensions {
    
      namespace Button {
        public static class ButtonExtensions {
          public static void setText(this UnityEngine.UI.Button self, string text) {
            self.GetComponentInChildren<Text>().text = text;
          }
        
          public static string getText(this UnityEngine.UI.Button self) {
            return self.GetComponentInChildren<Text>().text;
          }
        
          public static Text getLabel(this UnityEngine.UI.Button self) {
            return self.GetComponentInChildren<Text>();
          }
        }
      }
      
      public static class ComponentExtensions {
        public static void Clone<T>(this T self, T source) where T : Component {
          BindingFlags bindingFlags = 
            BindingFlags.Public | 
            BindingFlags.NonPublic | 
            BindingFlags.Instance | 
            BindingFlags.Default | 
            BindingFlags.DeclaredOnly;
        
          Type selfType = self.GetType();
          PropertyInfo[] properties = selfType.GetProperties(bindingFlags);
          foreach (PropertyInfo property in properties) {
            if (property.CanWrite) {
              try {
                property.SetValue(self, property.GetValue(source, null), null);
              }
              catch {
              } 
            }
          }
        
          FieldInfo[] fields = selfType.GetFields(bindingFlags);
          foreach (FieldInfo field in fields) {
            field.SetValue(self, field.GetValue(source));
          }
        }      
      }
    
      public static class GameObjectExtensions {
        public static T AddComponent<T>(this GameObject self, T source) where T : Component {
          T newComponent = self.AddComponent<T>();
          newComponent.Clone(source);
          return newComponent;
        }
      }
      
    }
  }
}

