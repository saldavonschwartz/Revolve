using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using FEDE.Revolve;

namespace FEDE {
    
  public class LeaderBoard : MonoBehaviour { 
    public LeaderBoardCell prototypeCell;
    private List<Player> ranking = new List<Player>();
    public int numberOfcells = 3;
    private IEnumerator<Transform> queue;
      
    void Start() {
      setNumberOfCells(numberOfcells);
      
      Player p1 = World.NewPlayer("FEDE");
      p1.score = 111;
        
      Player p2 = World.NewPlayer("STE");
      p2.score = 112;
      
      updateOrInsertScore(p1);
      updateOrInsertScore(p2);
    }
      
    public void updateOrInsertScore(Player player) {
      Player entry = ranking.Find((p) => p.id == player.id);
      if (entry != null) {
        entry.score = player.score;
      }
      else {
        ranking.Add(player);
      }
        
      ranking.Sort((p1, p2) => p2.score.CompareTo(p1.score));
      updateUI();
    }
      
    private void updateUI() {
      LeaderBoardCell cell;
      for (int i = 0; i < numberOfcells; i++) {
        cell = dequeueCell();
        cell.player = (i < ranking.Count) ? ranking[i] : null;
      }
    }
      
    private LeaderBoardCell dequeueCell() {
      if (queue == null || !queue.MoveNext()) {
        IEnumerable<Transform> cells = transform.Cast<Transform>();
        queue = cells.GetEnumerator();
        queue.MoveNext();
      }
      
      return queue.Current.GetComponent<LeaderBoardCell>();
    }
      
    private void setNumberOfCells(int cellCount) {
      int diff = cellCount - transform.childCount;
      if (diff > 0) {
        while (diff-- > 0) {
          LeaderBoardCell cell = Instantiate<LeaderBoardCell>(prototypeCell);
          cell.transform.SetParent(transform, false);
        }
      }
      else if (diff < 0) {
        while (diff++ < 0) {
          Transform cell = transform.GetChild(transform.childCount - 1);
          Destroy(cell.gameObject);
        }
      }
    }
  }
    
}

