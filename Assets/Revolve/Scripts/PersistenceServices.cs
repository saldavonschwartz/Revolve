using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System;

namespace FEDE {
  namespace Revolve {
    
    public class World {
      private enum OperationType {
        Load,
        Save
      }
      
      private static World sharedInstance;
      private static string persistentPath = Application.persistentDataPath + "/World.xml";
      private List<Player> players = new List<Player>();
      
      static World() {
        sharedInstance = new World();
      }
      
      public static Player NewPlayer(string name) {
        Player newPlayer = new Player(name);
        sharedInstance.players.Add(newPlayer);
        return newPlayer;
      }
      
      public static void Save(Action completion = null) {
        syncWithPersistentStore(OperationType.Save, completion);
      }
      
      public static void Load(Action completion = null) {
        syncWithPersistentStore(OperationType.Load, completion);
      }
      
      private static void syncWithPersistentStore(OperationType operationType, Action completion = null) {
        Thread t = new Thread(() => {
          FileMode mode = operationType == OperationType.Save ? FileMode.Create : FileMode.Open;
          FileStream stream = new FileStream(persistentPath, mode);
          XmlSerializer serializer = new XmlSerializer(typeof(World));        
          if (operationType == OperationType.Load) {
            sharedInstance = serializer.Deserialize(stream) as World;
          }
          else {
            serializer.Serialize(stream, sharedInstance);
          }
          
          if (completion != null) {
            completion();
          }
        });
        
        t.IsBackground = true;
        t.Start();
      }
    }
    
  }
}
