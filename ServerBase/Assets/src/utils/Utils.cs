using UnityEngine;
using System.Collections;

public class Utils
{

    /**
        * Returns the top-level parent element for the given GameObject.
        * Returns the given object if it has no parents and therefore
        * already is the top-level element
        * @param	input
        * @return the "master" parent game object in this objects hierarchy
        */
    public static GameObject getTopmostParent(GameObject input)
    {
        Debug.Log("getting topmost parent for " + input.name);
        Transform current = input.transform;
        while (current != null)
        {
            if (current.parent != null)
            {
                current = current.parent.transform;
            }
            else
            {
                return current.gameObject;
            }
        }
        //Actually - this code is never reached except if the input was null to start with
        Debug.Log("Input is null, returning");
        return input;
    }

    public static GameObject findPlayer(NetworkPlayer player)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            C_PlayerManager man = p.GetComponentInChildren<C_PlayerManager>();
            if (man != null)
            {
                if (man.getOwner() == player)
                {
                    return p as GameObject;
                }
            }
        }

        return null;
    }
}