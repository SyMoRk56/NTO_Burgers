using System;
using System.Collections.Generic;
using UnityEngine;

public class MailBox : MonoBehaviour
{
    public GameObject letterPrefab;
    public List<string> letters = new List<string>();
    public void Interact()
    {
        print("Interact");
        if(letters.Count == 0) return;
        var GO = GameObject.FindWithTag("Pickup");
        if (GO != null) return;
        var letterGO = Instantiate(letterPrefab);
        var letter = letterGO.GetComponent<Letter>();
        letter.recieverName = letters[0];
        letters.RemoveAt(0);
        GameManager.Instance.GetPlayer().GetComponent<PlayerInteraction>().pickupedLetter = letter;
    }
}
