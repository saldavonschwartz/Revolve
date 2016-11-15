using UnityEngine;
using System.Collections;

public class CanvasScale : MonoBehaviour {
  private float nativeFrustumWidth = 10.32547f;
  private float nativeFrustumHeight = 5.773502f;
  
  void Start() {
    Vector3 cameraPosition = Camera.main.transform.position;
    float distanceBetweenUIAndCamera = Mathf.Abs(transform.position.z - cameraPosition.z);
    float frustumHeight = 2f * distanceBetweenUIAndCamera * Mathf.Tan(Camera.main.fieldOfView * .5f * Mathf.Deg2Rad);
    float frustumWidth = frustumHeight * Camera.main.aspect;
    float widthScale = frustumWidth / nativeFrustumWidth;
    float heightScale = frustumHeight / nativeFrustumHeight;
    RectTransform t = GetComponent<RectTransform>();
    t.sizeDelta = new Vector2(t.sizeDelta.x * widthScale, t.sizeDelta.y * heightScale);
  }
}
