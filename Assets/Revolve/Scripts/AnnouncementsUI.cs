using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using FEDE.Revolve;

public class AnnouncementsUI : FEDE.BaseBehaviour {
  public Text announcement;
  public Driver driver;
  private Animator animator;
  private int announceState;
  public bool startGame;
  public AudioSource announcementsSpeaker;
  public AudioClip countDownReady;
  public AudioClip countDownGo;
  public AudioSource beat;
  public static AnnouncementsUI sharedInstance;
  
  void Awake() {
    sharedInstance = this;
  }
  
  void Start() {
    logEnabled = false;
    animator = announcement.GetComponent<Animator>();
    announceState = Animator.StringToHash("CountDown");
  }
  
  public void countDownToGame(Action completion) {
    StartCoroutine(countDown(completion));
  }
  
  private IEnumerator countDown(Action completion) {
    int count = 3;
    announcementsSpeaker.clip = countDownReady;
    for (int i = count; i > 0; i--) {
      announcementsSpeaker.Play();
      announce(i.ToString());
      yield return new WaitForSeconds(1f);
    }
    
    announcementsSpeaker.clip = countDownGo;
    announcementsSpeaker.Play();
    announce("GO!");
    completion();
    yield return new WaitForSeconds(1f);
    beat.Play();
  }
  
  public void announce(string text, bool soundAlert = false) {
    if (soundAlert) {
      announcementsSpeaker.clip = countDownReady;
      announcementsSpeaker.Play();
    }
    
    announcement.text = text;
    animator.Play(announceState, -1, 0f);
  }
}
