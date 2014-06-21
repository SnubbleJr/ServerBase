using UnityEngine;
using System.Collections;

public class PlayerHudManager : MonoBehaviour
{
    public GUITexture backdrop;
    public GUIText text;
    public GUIText data;
    public HudType type;

    public int slot; //The slot number, the heighter, the further away from screen top

    private float screenHeight = 0f;
    private float screenWidth = 0f;

    private GameObject father; //The topmost element

    private C_BonusManager bonusMan;

    void Start()
    {
        if (Network.isServer)
        {
            enabled = false;
            return;
        }
        father = Utils.findPlayer(Network.player);
        bonusMan = father.GetComponentInChildren<C_BonusManager>();
        if (bonusMan == null)
        {
            Debug.LogError("Could not find C_BonusManager for HUD panel " + type);
        }
        updatePosition();
    }

    void Update()
    {
        updatePosition();

        switch (type)
        {
            case HudType.TIMER:
                updateTime();
                break;
            case HudType.POINTS:
                updatePoints();
                break;
            case HudType.SPEED:
                updateBonus(BonusType.SPEED);
                break;
            case HudType.POINT_MULTIPLY:
                updateBonus(BonusType.POINT_MULTIPLY);
                break;
            default:
                break;
        }
    }

    private void updatePoints()
    {
        data.text = bonusMan.getPoints().ToString();
    }

    private void updateBonus(BonusType type)
    {
        data.text = "x" + bonusMan.getBonusForType(type);
    }

    private void updateTime()
    {
        data.text = bonusMan.getTimeAsString();
    }

    private void updatePosition()
    {
        fixElementPositions();
        if (screenHeight != Screen.height || screenWidth != Screen.width)
        {
            screenHeight = Screen.height;
            screenWidth = Screen.width;

            //Set backdrop:
            Rect rect = backdrop.pixelInset;
            rect.x = -((screenWidth / 2));
            rect.y = ((Screen.height / 2) - ((backdrop.texture.height * slot) + 10 * slot));
            backdrop.pixelInset = rect;

            //Set the headline
            Vector2 vect = text.pixelOffset;
            text.pixelOffset.Set(-((screenWidth / 2) - (backdrop.texture.width - 70)), (screenHeight / 2) - ((backdrop.texture.height * slot) + 10 * slot) + (backdrop.texture.height - 5));
            

            //Set the value
            vect.x = -((screenWidth / 2) - (backdrop.texture.width - 70));
            vect.y = (screenHeight / 2) - ((backdrop.texture.height * slot) + 10 * slot) + (backdrop.texture.height - 32);
            data.pixelOffset = vect;
        }
    }

    /**
     * Make sure the elements of the HUD are always on 0.5f x and y on their local axis
     */
    private void fixElementPositions()
    {
        if (father.transform.position != new Vector3(0.5f, 0.5f, 0))
        {
            father.transform.position = new Vector3(0.5f, 0.5f, 0);
        }
    }

    public enum HudType
    {
        TIMER = 0,
        POINTS = 1,
        SPEED = 2,
        POINT_MULTIPLY = 3
    }
}