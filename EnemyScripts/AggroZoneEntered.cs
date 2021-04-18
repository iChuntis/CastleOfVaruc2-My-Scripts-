using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggroZoneEntered : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other){
        if(other.tag == "Player"){
            transform.parent.SendMessage("EnteredAggroZone");
        }
    }

    private void OnTriggerExit2D(Collider2D other){
        if(other.tag == "Player"){
            transform.parent.SendMessage("ExitedAggroZone");
        }
    }
}
