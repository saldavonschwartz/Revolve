using UnityEngine;
using System.Collections;

namespace FEDE {
  namespace Revolve {
    
    public class Player {
      public static int Count = 0; 
      public string name;
      public long score = 0;
      public int id = -1;
      
      public Player(string name) {
        this.name = name;
        id = Count++;
      } 
    }
    
  }
}
