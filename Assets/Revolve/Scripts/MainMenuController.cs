using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

namespace FEDE {
  namespace Revolve {
  
    public class MainMenuController : SceneController {
      public Animator textAnimator;
      public BuildCircleMesh progressIndicator;
      public Button postFXButton;
      public GameObject gazeCursor;
      public GazeInputModule gazeModule;
      
      public void onSettings() {
        transition("Settings");
      }
      
      public void onStartGame() {
        transition("Level1", () => {
          Game.currentGame.initialize((progress) => {
            progressIndicator.endAngle = 360f * progress;
            continueTransition = (progress == 1f);
          });
        });
      }
    }

  }
}

