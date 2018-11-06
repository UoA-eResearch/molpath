using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Player))]
public class PlayerUI : MonoBehaviour
{
    public List<GameObject> menus;

    public Player myPlayer;

    public Hand[] myHands;

    private int activeMenuIndex = 0;
    private List<Transform> menuOffset;

    void Awake()
    {
        if (myPlayer == null)
        {
            myPlayer = this.gameObject.GetComponent<Player>();
        }
        if (myHands.Length == 0)
        {
            myHands = myPlayer.hands;
        }

        // add an empty game object to the menu list so there's a no menu option.
        menus.Add(null);
    }
}

