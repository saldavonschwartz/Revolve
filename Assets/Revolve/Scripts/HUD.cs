using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using FEDE.Utils.Extensions.Button;

namespace FEDE {
  namespace Revolve {

    public class HUD : SceneController {
      public Button backButton;
      public Button pauseButton;
      public Button viewButton;
      
      void Awake() {
        viewButton.onClick.AddListener(() => {
          if (Game.currentGame.state == Game.State.Ready || Game.currentGame.state == Game.State.Playing) {
            userInteractionEnabled = false;
            changeViewPoint();
            userInteractionEnabled = true;
          }
        });
        
        backButton.onClick.AddListener(() => {
          userInteractionEnabled = false;
          Game.currentGame.abort();
        });
        
        pauseButton.onClick.AddListener(() => {
          userInteractionEnabled = false;
          if (Game.currentGame.state == Game.State.Playing || Game.currentGame.state == Game.State.Ready) {
            pauseButton.setText("RESUME");
            Game.currentGame.pause();
          }
          else if (Game.currentGame.state == Game.State.Paused) {
            pauseButton.setText("PAUSE");
            Game.currentGame.play();
          }
          userInteractionEnabled = true;
        });
      }
      
      private void changeViewPoint() {
        PlayerCamera playerCamera = Game.currentGame.vehicle.GetComponent<Driver>().playerCamera;
        if (playerCamera.viewPortType == PlayerCamera.ViewPortType.FirstPerson) {
          viewButton.setText("3RD PERSON");
          playerCamera.viewPortType = PlayerCamera.ViewPortType.ThirdPerson;
        }
        else {
          viewButton.setText("1ST PERSON");
          playerCamera.viewPortType = PlayerCamera.ViewPortType.FirstPerson;
        }
      }
    }

  }
}

