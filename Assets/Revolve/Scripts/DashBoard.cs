using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using FEDE.Revolve;

public class DashBoard : MonoBehaviour {
  public Vehicle vehicle;
  private BuildCircleMesh gauge;
  private Text pedometer;
  
  void Start() {
    gauge = gameObject.GetComponentInChildren<BuildCircleMesh>();
    pedometer = gameObject.GetComponentInChildren<Text>();
  }
  
  void Update() {
    gauge.endAngle = 360f * vehicle.speed / vehicle.terminalSpeed;
    pedometer.text = string.Format("{0:0000} M", Mathf.RoundToInt(vehicle.position));
  }
}
