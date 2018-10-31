using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

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

    void Start()
    {

    }

    void Update()
    {
        // poll for inputs from either hand
        foreach (var hand in myHands)
        {
            if (hand.controller != null)
            {
                if (hand.controller.GetPressDown(EVRButtonId.k_EButton_ApplicationMenu))
                {
                    GameObject newMenu = CycleMenu();
                    if (newMenu.transform.parent != hand.transform)
                    {
                        SwapMenuHand(newMenu, hand);
                    }
                }
            }
        }
    }

    private GameObject CycleMenu()
    {
        // switch off current menu
        activeMenuIndex = activeMenuIndex % menus.Count;
        if (menus[activeMenuIndex] != null)
        {
            menus[activeMenuIndex].SetActive(false);
        }

        // update index and active new menu
        activeMenuIndex++;
        activeMenuIndex %= menus.Count;
        if (menus[activeMenuIndex] != null)
        {
            menus[activeMenuIndex].SetActive(true);
            return menus[activeMenuIndex];
        }
        return null;
    }

    private void SwapMenuHand(GameObject menu, Hand hand)
    {
        var offset = menu.transform.localPosition;
        menu.transform.parent = hand.transform;
        menu.transform.localPosition = offset;
    }
}
