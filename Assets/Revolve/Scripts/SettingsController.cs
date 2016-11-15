using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using FEDE.Utils.Extensions.Button;

namespace FEDE {
  namespace Revolve {
  
    public class SettingsController : SceneController {
      public Button displayModeButton;
      public Button postFXButton;
      public Button musicButton;
      public Button[] trapConfigButtons = new Button[3];
      public bool displayVRModeInSettings;
      
      private bool isProbablyTablet() {
        float screenHeightInInch = Screen.height / Screen.dpi;
        return screenHeightInInch >= 3.1;
      }
      
      void Start() {
        if (!displayVRModeInSettings && isProbablyTablet()) {
          displayModeButton.gameObject.SetActive(false);
        }
        else {
          displayModeButton.setText(Game.displayMode == Game.DisplayMode.Monoscopic ? "VR STEREO DISPLAY: OFF" : "VR STEREO DISPLAY: ON");        
        }
        
        postFXButton.setText(PlayerCamera.postFXEnabled ? "VISUAL EFFECTS: ON" : "VISUAL EFFECTS: OFF");
        musicButton.setText(Game.currentGame.soundtrackEnabled ? "MUSIC: ON" : "MUSIC: OFF");
        updateTrapConfigButtons();
      }
      
      public void onBack() {
        transition("MainMenu");
      }
      
      public void onDisplayModeChange() {
        if (Game.displayMode == Game.DisplayMode.Monoscopic) {
          Game.displayMode = Game.DisplayMode.Stereoscopic;
        }
        else {
          Game.displayMode = Game.DisplayMode.Monoscopic;
        }
        
        displayModeButton.setText(Game.displayMode == Game.DisplayMode.Monoscopic ? "VR STEREO DISPLAY: OFF" : "VR STEREO DISPLAY: ON");
      }
      
      public void onPostFXChange() {
        if (PlayerCamera.postFXEnabled) {
          PlayerCamera.postFXEnabled = false;
        }
        else {
          PlayerCamera.postFXEnabled = true;
        }
        
        postFXButton.setText(PlayerCamera.postFXEnabled ? "VISUAL EFFECTS: ON" : "VISUAL EFFECTS: OFF");
      }
      
      public void onMusicChange() {
        Game.currentGame.soundtrackEnabled = !Game.currentGame.soundtrackEnabled;
        musicButton.setText(Game.currentGame.soundtrackEnabled ? "MUSIC: ON" : "MUSIC: OFF");
      }
      
      public void onTrapConfigurationChange(int trapConfiguration) {
        Game.currentGame.track.difficulty = (Track.Difficulty)trapConfiguration;
        updateTrapConfigButtons();
      }
      
      private void updateTrapConfigButtons() {
        Color onColor;
        Color.TryParseHexString("00FFDFFF", out onColor);
        
        Color offColor;
        Color.TryParseHexString("FF7900FF", out offColor);
        
        int selectedIndex = (int)Game.currentGame.track.difficulty;
        for (int i = 0; i < trapConfigButtons.Length; i++) {
          trapConfigButtons[i].getLabel().color = (i == selectedIndex ? onColor : offColor);
        }
      } 
    }

  }
}
