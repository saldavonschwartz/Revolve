using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using System.Threading;
using FEDE.Utils;

namespace FEDE {
  namespace Revolve {
    
    public class Track : BaseBehaviour {
     
      [System.Serializable]
      public enum Difficulty {
        Easy,
        Medium,
        Hard
      }
     
      [System.Serializable]
      public class DispatchDescriptor {
        public UnityEngine.ThreadPriority backgroundLoadingPriority;
        
        public Mode mode;
        public enum Mode {
          AllAtOnce,
          Sequential
        }
        
        public PostProcessingMode postProcessingMode;
        public enum PostProcessingMode {
          MainThread,
          BackgroundThread
        }
        
        public int maxInstantiationsPerInvocation = 0;
        public bool clearCache;
      }
      
      [System.Serializable]
      public class PathItemDescriptor {
        public Transform root;
        public float probability;
        public bool isWayPoint;
        
        internal List<Transform> chunks = new List<Transform>();
        internal int chancesFromProbability {
          get {
            return Mathf.RoundToInt(1f / probability);
          }
        }
      }
      
      public DispatchDescriptor dispatchSetup;
      public int chunks = 10;
      public Action<int> onChunkLoaded;
      public int seed = 50;
      [SerializeField]
      private Difficulty
        _difficulty;
      public List<PathItemDescriptor> items = new List<PathItemDescriptor>();
      private PathItemDescriptor wayPoints;
      public List<PathItemDescriptor> traps;
      private System.Random probability;
      public bool initialized;
      public Path path;
      
      private Difficulty lastDificulty;
      
      
      public Difficulty difficulty {
        get {
          return _difficulty;
        }
      
        set {
          if (_difficulty != value) {
            _difficulty = value;
            switch (_difficulty) {
            case Difficulty.Easy:
              {
                traps[0].probability = 0.1f;
                traps[1].probability = 0.05f;
                break;
              }
              
            case Difficulty.Medium:
              {
                traps[0].probability = 0.25f;
                traps[1].probability = 0.1f;
                break;
              }
            
            case Difficulty.Hard:
              {
                traps[0].probability = 0.3f;
                traps[1].probability = 0.4f;
                break;
              } 
            }
          }
        }
      }
      
      void Awake() {
        logEnabled = false;
        wayPoints = items.Find((d) => d.isWayPoint);
        traps = items.Where<PathItemDescriptor>((d) => !d.isWayPoint).ToList();
      }
      
      public void initialize(Action<float> progress = null) {
        if (progress == null) {
          progress = (dummy) => {};
        }
        
        if (initialized && difficulty == lastDificulty) {
          progress(1f);
          Log("already initialized");
          return;
        }
        
        probability = new System.Random(seed);
        foreach (PathItemDescriptor item in items) {
          item.chunks.ForEach((c) => Destroy(c.gameObject));
          item.chunks.Clear();
        }
        
        Application.backgroundLoadingPriority = dispatchSetup.backgroundLoadingPriority;
        if (dispatchSetup.mode == DispatchDescriptor.Mode.Sequential) {
          onChunkLoaded += (chunkIdx) => {
            progress((chunks - chunkIdx) / (float)chunks);
            if (chunkIdx > 0) {
              StartCoroutine(loadChunk(chunkIdx - 1));
            }
            else {
              StaticBatchingUtility.Combine(wayPoints.root.gameObject);
              Loader.ReleaseAllAssetBundles();
              Loader.Dispose();
              onChunkLoaded = null;
              initialized = true;
              lastDificulty = difficulty;
            }
          };
          
          dispatch(DispatchMode.Foreground, () => StartCoroutine(loadChunk(9)));
        }
        else {
          onChunkLoaded += (chunkIdx) => {
            progress((chunks - chunkIdx) / (float)chunks);
            if (chunkIdx == 0) {
              StaticBatchingUtility.Combine(wayPoints.root.gameObject);
              Loader.ReleaseAllAssetBundles();
              Loader.Dispose();
              onChunkLoaded = null;
              initialized = true;
              lastDificulty = difficulty;
            }
          };
          
          dispatch(DispatchMode.Foreground, () => {
            for (int chunkIdx = chunks - 1; chunkIdx >= 0; chunkIdx--) {
              StartCoroutine(loadChunk(chunkIdx));
            }
          });
        }
      }
      
      private IEnumerator loadChunk(int chunkIdx) {
        Log("load chunk " + chunkIdx);
        int assetChunksLoaded = 0;
        for (int i = 0; i < items.Count; i++) {
          string assetBundlePath = Application.streamingAssetsPath + "/" + items[i].root.name.ToLower();
          Loader.LoadAssetBundleAsync(assetBundlePath, i, (bundle, i2) => {
            string assetName = string.Format("{0}-{1}", items[i2].root.name, chunkIdx);
            Loader.LoadAssetFromBundleAsync(bundle, assetName, i2, (assetRequest, i3) => {
              GameObject asset = assetRequest.allAssets[0] as GameObject;
              StartCoroutine(instantiateItemsFromAsset(asset, i3, () => assetChunksLoaded++));
            });
          });          
        }
        
        while (assetChunksLoaded < items.Count) {
          yield return null;
        }
        
        computeTrapsForChunk(chunks - chunkIdx - 1, () => {
          Log("done processing chunk " + chunkIdx);
          if (onChunkLoaded != null) {
            onChunkLoaded(chunkIdx);
          }
        });    
      }
      
      IEnumerator instantiateItemsFromAsset(GameObject rootAsset, int itemIdx, Action completion) {
        if (dispatchSetup.maxInstantiationsPerInvocation <= 0) {
          GameObject newInstance = Instantiate<GameObject>(rootAsset);
          newInstance.name = rootAsset.name;
          newInstance.transform.parent = items[itemIdx].root;
          newInstance.transform.localRotation = rootAsset.transform.localRotation;
          items[itemIdx].chunks.Add(newInstance.transform);  
        }
        else {
          GameObject newInstance = new GameObject(rootAsset.name);
          newInstance.transform.parent = items[itemIdx].root;
          newInstance.transform.localRotation = rootAsset.transform.localRotation;
          items[itemIdx].chunks.Add(newInstance.transform);  
          
          Log("the asset has childs:" + rootAsset.transform.childCount);
          for (int instantiationCount = 0; instantiationCount < rootAsset.transform.childCount; instantiationCount++) {
            if (instantiationCount % dispatchSetup.maxInstantiationsPerInvocation == 0) {
              Log(newInstance.name + " processed up to " + instantiationCount);
              yield return null;
            }
            
            GameObject childAsset = rootAsset.transform.GetChild(instantiationCount).gameObject;
            GameObject newChild = Instantiate<GameObject>(childAsset);
            newChild.name = childAsset.name;
            newChild.transform.parent = newInstance.transform;
          }
        }
        
        completion();
      }
      
      // At each waypoint, we decide if we enable a trap:
      private void computeTrapsForChunk(int chunkIdx, Action completion) {
        bool trapEnabledAtIndex;
        int itemCount = wayPoints.chunks.Count > 0 ? wayPoints.chunks[chunkIdx].childCount : 100;
        int[] itemAllocations = new int[itemCount];
        
        Action computeTrapsAssignment = () => {
          for (int item = 0; item < itemAllocations.Length; item++) {
            trapEnabledAtIndex = false;
            for (int trapIdx = 0; trapIdx < traps.Count; trapIdx++) {
              PathItemDescriptor trap = traps[trapIdx];
              bool shouldEnableTrap = !trapEnabledAtIndex && probability.Next(0, trap.chancesFromProbability) == 0;
              if (shouldEnableTrap) {
                trapEnabledAtIndex = true;
                itemAllocations[item] = trapIdx;
              }
            }
            
            if (!trapEnabledAtIndex) {
              itemAllocations[item] = items.Count - 1;
            }
          }
        };
        
        if (dispatchSetup.postProcessingMode == DispatchDescriptor.PostProcessingMode.BackgroundThread) {
          dispatch(DispatchMode.Background, () => {
            computeTrapsAssignment();
            dispatch(DispatchMode.Foreground, () => {
              addTrapsForChunk(chunkIdx, itemAllocations, completion);
            });
          });
        }
        else {
          computeTrapsAssignment();
          addTrapsForChunk(chunkIdx, itemAllocations, completion);
        }
      }
      
      // Based on the allocations computed at 'computeTrapsForChunk', we set up
      // the actual game objects:
      private void addTrapsForChunk(int chunkIdx, int[] allocations, Action completion) {
        List<List<GameObject>> itemObjects = new List<List<GameObject>>();
        foreach (PathItemDescriptor descriptor in items) {
          itemObjects.Add(descriptor.chunks[chunkIdx].Cast<Transform>().Select((t) => t.gameObject).ToList<GameObject>());
        }
        
        for (int itemIdx = 0; itemIdx < allocations.Length; itemIdx++) {
          int itemType = allocations[itemIdx];
          for (int itemTypeIdx = 0; itemTypeIdx < items.Count; itemTypeIdx++) {
            if (itemTypeIdx == itemType) {
              GameObject item = itemObjects[itemType][itemIdx];
              item.SetActive(true);
              Renderer renderer = item.GetComponent<Renderer>();
              renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
              renderer.receiveShadows = false;
              renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
              renderer.useLightProbes = false;
              
#if UNITY_EDITOR
              renderer.material.shader = Shader.Find("Standard");
#endif
              if (itemType != items.Count - 1) {
                Trap trap = item.AddComponent<Trap>();
                trap.rotationSpeed *= probability.Next(1, (int)difficulty + 1); 
                if (trap.rotationSpeed == 1.5f) {
                  trap.rotationSpeed = 1.25f;
                }
              }
            }
            else {
              Destroy(items[itemTypeIdx].chunks[chunkIdx].GetChild(itemIdx).gameObject);
            }
          }
        }
        
        completion();
      }
    }
    
  }
}
