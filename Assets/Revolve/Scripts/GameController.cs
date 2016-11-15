using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using UnityStandardAssets.ImageEffects;
using System;

namespace FEDE {
  namespace Revolve {

    public class GameController : SceneController {
      public AnnouncementsUI announcementsUI;
      public bool shouldStartGame = true;
      public Transform vehicle;
      
      void Awake() {
        Game.currentGame.vehicle = FindObjectOfType<Vehicle>();
        NotificationServices.addObserver(Game.OnStateChangedNotification, gameStateChanged);
      }
      
      protected override void Update() {
        base.Update();
        if (shouldStartGame) {
          shouldStartGame = false;
          StartCoroutine(announceGameStart());
        }
      }
      
      private IEnumerator announceGameStart() {
        if (Game.displayMode == Game.DisplayMode.Monoscopic) {
          announcementsUI.announce("HOLD DEVICE STRAIGHT");
          yield return new WaitForSeconds(1f);
        }
        announcementsUI.countDownToGame(() => Game.currentGame.play());
      }
      
      private void gameStateChanged(Component sender) {
        if (Game.currentGame.state == Game.State.Over) {
          FindObjectsOfType<Button>().ToList<Button>().ForEach((b) => Destroy(b.gameObject));
          NotificationServices.removeObserver(Game.OnStateChangedNotification, gameStateChanged);
          StartCoroutine(announceGameOver());
        }
        else if (Game.currentGame.state == Game.State.Aborted) {
          NotificationServices.removeObserver(Game.OnStateChangedNotification, gameStateChanged);
          Application.LoadLevelAsync("MainMenu");
        }
      }
      
      private IEnumerator announceGameOver() {
        PlayerCamera playerCamera = Game.currentGame.vehicle.GetComponent<Driver>().playerCamera;
        playerCamera.viewPortType = PlayerCamera.ViewPortType.ThirdPerson;
        AnnouncementsUI.sharedInstance.beat.Stop();
        AnnouncementsUI.sharedInstance.announce("GAME OVER");
        yield return new WaitForSeconds(1.5f);
        
        AsyncOperation reload = Application.LoadLevelAsync(Application.loadedLevel);
        reload.priority = 1000;
        reload.allowSceneActivation = false;
        yield return null;
        
        Game.currentGame.vehicle.GetComponent<Driver>().playerCamera.fade(true, () => {
          reload.allowSceneActivation = true;
        });
      }
    }

  }
}

