using UnityEngine;
using System.Collections;

[RequireComponent (typeof(SphereCollider))]

public class BonusHandler : MonoBehaviour {

    public BonusType type;
    public float timeToLife, minValue, maxValue;
    public GameObject effectOnCollect;

    private Bonus bonus;

	// Use this for initialization
    void Start()
    {
        bonus = new Bonus(type, timeToLife, minValue, maxValue);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	/**
	 * Get the attached bonus
	 * @return
	 */
	public Bonus getBonus()
    {
		return bonus;
	}
}
