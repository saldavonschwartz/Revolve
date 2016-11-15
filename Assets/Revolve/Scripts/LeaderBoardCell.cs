using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace FEDE {
  namespace Revolve {
  
    public class LeaderBoardCell : LayoutElement {
      private Player _player;
      public Player player {
        get {
          return _player;
        }
    
        set {
          _player = value;
          
          string labelText = null;
          if (_player != null) {
            labelText = string.Format("{0}m {1}", player.score, player.name.ToUpper());
          }
          GetComponentInChildren<Text>().text = labelText;
        }
      }
    }

  }
}
