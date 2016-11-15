using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections;
using System;

public class TrackChunkImporter : AssetPostprocessor {
  static public int chunkSize = 100;
  static private bool shouldProcess = false;
  
  static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
    if (!shouldProcess) {
      return;
    }
    
    if (importedAssets.Length == 0) {
      return;
    }
    
    UnityEngine.Object model = AssetDatabase.LoadMainAssetAtPath(importedAssets[0]);
    if (model.name != "Level1") {
      return;
    }
    
    shouldProcess = false;
    GameObject temp = GameObject.Instantiate(model) as GameObject;
    
    Transform[] roots = new Transform[3] {
      temp.transform.Find("WayPointRoot"),
      temp.transform.Find("Trap90Root"),
      temp.transform.Find("Trap180Root")
    };
    
    Transform parent;
    List<Transform> tempChildren;
    int chunkCount = Mathf.FloorToInt(roots[0].childCount / (float)chunkSize);
    
    for (int chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++) {
      for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++) {
        parent = new GameObject().transform;
        parent.name = string.Format("{0}-{1}", roots[rootIndex].name, chunkIndex);
        tempChildren = new List<Transform>(roots[rootIndex].Cast<Transform>().ToList());
        tempChildren = tempChildren.Where((t) => t.childCount == 0).ToList<Transform>();
        tempChildren = tempChildren.GetRange(0, Mathf.Min(chunkSize, tempChildren.Count));
        tempChildren.ForEach((t) => t.parent = parent);
        parent.parent = roots[rootIndex];
      }
    }
    
    Transform path = temp.transform.Find("Path");
    path.gameObject.AddComponent<FEDE.Revolve.Path>();
    path.gameObject.AddComponent<FEDE.Revolve.Track>();
  }  
}

