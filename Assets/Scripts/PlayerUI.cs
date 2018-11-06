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

    void Start()
    {

    }

    void Update()
    {
        // poll for inputs from either hand
        // TODO: switch from polling to event triggers.
        foreach (Hand hand in myHands)
        {
            if (hand.controller != null)
            {
                if (hand.controller.GetPressDown(EVRButtonId.k_EButton_ApplicationMenu))
                {
                    GameObject newMenu = CycleMenu();
                    if (newMenu.transform.parent && newMenu.transform.parent != hand.transform)
                    {
                        SwapMenuHand(newMenu, hand);
                    }
                }
            }
        }

        DetectMovement();
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


    private Dictionary<GameObject, Vector3> menuItemPositions = new Dictionary<GameObject, Vector3>();
    private void DetectMovement()
    {
        // iterate the children
        foreach (GameObject menu in menus)
        {
            // RecursiveLogLocalPositions(menu.transform);
            // Debug.Log(menu.name);
            // foreach (Transform sub in menu.transform)
            // {
            //     Debug.Log(sub.name);
            //     Debug.Log(sub.localPosition);
            // }
        }
    }

    public static void RecursiveLogLocalPositions(Transform t)
    {
        foreach (Transform sub in t)
        {
            // something recursive here.
            RecursiveLogLocalPositions(sub.transform);
        }
    }
}
