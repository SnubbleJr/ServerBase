using UnityEngine;
using System.Collections;

/**
 * Handles displaying of scores highscore list
 */

public class ScoreDisplay : MonoBehaviour {
	public GUITexture backdrop;
	
	public bool  displayScores;
	private string[] scoreTable;
	private bool  backdropIsHidden;
	
	public void  Start (){
		hideBackdrop();
	}
	
	public void  setScoreTable ( string[] tbl  ){
		scoreTable = tbl;
	}
	
	public void  showBackdrop (){
        Rect BDrect = backdrop.pixelInset;
		BDrect.width = Screen.width + 100;
		BDrect.height = Screen.height + 100;
        backdrop.pixelInset = BDrect;

        Color BDcolor = backdrop.color;
        BDcolor.a = 0.48f;
        backdrop.color = BDcolor;
		backdropIsHidden = false;
	}
	
	public void  hideBackdrop (){

        Rect BDrect = backdrop.pixelInset;
        BDrect.width = 0;
        BDrect.height = 0;
        backdrop.pixelInset = BDrect;

        Color BDcolor = backdrop.color;
        BDcolor.a = 0f;
        backdrop.color = BDcolor;
        backdropIsHidden = true;
	}
	
	public void  OnGUI (){
		if (!displayScores) {
			return;
		}
		if (backdropIsHidden) {
			showBackdrop();
		}
		int position = 0;
		GUILayout.BeginArea( new Rect(Screen.width/2, 40, 350, Screen.height-40));
			foreach(string str in scoreTable) {
				string[] split = str.Split(":"[0]);
				GUILayout.BeginArea( new Rect(5, (50*position) + 5, 200, 50));
					GUILayout.Label(split[0]+": " + split[1] + " Points!");
					
				GUILayout.EndArea();
				position++;
			}
		GUILayout.EndArea();
	}
}
