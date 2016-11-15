using UnityEngine;
using System.Collections;

public class StartGame : MonoBehaviour {

  void Start() {
    FEDE.BaseBehaviour.GlobalLogEnabled = false;
    Screen.sleepTimeout = SleepTimeout.NeverSleep;
    Application.LoadLevelAsync("MainMenu");
  }
	
}
