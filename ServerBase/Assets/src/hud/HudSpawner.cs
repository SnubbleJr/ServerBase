using UnityEngine;
using System.Collections;

public class HudSpawner : MonoBehaviour {

    public GameObject theHud, theScores;

	// Use this for initialization
    void Start()
    {
        Instantiate(theHud, new Vector3(0.5f, 0.5f, 0f), Quaternion.identity); 
	    GameObject scores = Instantiate(theScores, new Vector3(0.5f, 0.5f, 0f), Quaternion.identity) as GameObject;
        GameObject GO = Utils.getTopmostParent(gameObject);
	    C_BonusManager player = GO.GetComponentInChildren<C_BonusManager>();
	    player.setScoreBoard(scores.GetComponent<ScoreDisplay>());
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
