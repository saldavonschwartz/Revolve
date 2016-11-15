using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Audio;

namespace FEDE {
  namespace Revolve {
    
    public class Game : BaseBehaviour {
      
      public enum DisplayMode {
        Monoscopic,
        Stereoscopic,
      }
      
      public enum State {
        Uninitialized,
        Initializing, 
        Ready,
        Playing,
        Paused,
        Over,
        Aborted
      }
      
      public static string OnStateChangedNotification = "OnGameStateChanged";
      public static string OnDisplayModeChangedNotification = "OnDisplayModeChanged";
      
      private static DisplayMode _displayMode;
      private static Game _currentGame = null;
      private bool needsInitialSetup = true;
      public Vehicle vehicle;
      public Track track;
      public AudioMixer audioMixer;
      private bool _soundtrackEnabled = true;
      
      public bool soundtrackEnabled {
        get {
          return _soundtrackEnabled;
        }
      
        set {
          if (_soundtrackEnabled != value) {
            _soundtrackEnabled = value;
            Game.currentGame.audioMixer.SetFloat("SoundtrackVolume", _soundtrackEnabled ? 0f : -80f);
          }
        }
      }
      
      private State _state;
    
      public static Game currentGame {
        get {
          if (_currentGame == null) {
            _currentGame = GameObject.FindObjectOfType<Game>();
            DontDestroyOnLoad(_currentGame.gameObject);
          }
          
          return _currentGame;
        }
      }
      
      public static DisplayMode displayMode {
        get {
          return _displayMode;
        }
      
        set {
          if (_displayMode != value) {
            _displayMode = value;
            NotificationServices.postNotification(OnDisplayModeChangedNotification, currentGame);
          }
        }
      }
      
      void Awake() {
        if (currentGame != this) {
          DestroyImmediate(this.gameObject);
        }
        else if (tag == "EditorOnly") {
          initialize();
        }
      }
      
      public State state {
        get {
          return _state;
        }
        
        private set {
          if (_state != value) {
            _state = value;
            NotificationServices.postNotification(OnStateChangedNotification, this);
          }
        }
      }
      
      public void initialize(Action<float> progress = null) {    
        if (progress == null) {
          progress = (p) => {};
        }    
        state = State.Initializing;
        track.initialize((p1) => {
          progress(p1);
          if (p1 == 1f) {
            state = State.Ready;
          }});
      }
      
      public void play() {
        Time.timeScale = 1f;
        state = State.Playing;
        audioMixer.SetFloat("TrapVolume", 20f);
        if (soundtrackEnabled) {
          audioMixer.SetFloat("SoundtrackVolume", 0f);
        }
        
        if (needsInitialSetup) {
          needsInitialSetup = false;
          vehicle = GameObject.FindObjectOfType<Vehicle>();
          NotificationServices.addObserver(Vehicle.OnHealthDecreasedNotification, gameOver);
          vehicle.health = 1f;
          vehicle.GetComponent<Driver>().enabled = true;
          vehicle.GetComponent<Driver>().autoThrust = true;
        }
        
        FindObjectOfType<PlayerCamera>().fade(false);
      }
      
      public void pause() {
        FindObjectOfType<PlayerCamera>().fade(true, () => {
          audioMixer.SetFloat("TrapVolume", -80f);
          if (soundtrackEnabled) {
            audioMixer.SetFloat("SoundtrackVolume", -80f);
          }
          
          Time.timeScale = 0f;
          state = State.Paused;
        });
      }
      
      public void abort() {
        NotificationServices.removeObserver(Vehicle.OnHealthDecreasedNotification, gameOver);
        audioMixer.SetFloat("TrapVolume", -80f);
        needsInitialSetup = true;
        Time.timeScale = 1f;
        state = State.Aborted;
      }
      
      private void gameOver(Component sender) {
        NotificationServices.removeObserver(Vehicle.OnHealthDecreasedNotification, gameOver);
        audioMixer.SetFloat("TrapVolume", -80f);
        needsInitialSetup = true;
        Time.timeScale = 1f;
        state = State.Over;
      }      
    }
    
  }  
}
