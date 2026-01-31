using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

public class BattleTile : MonoBehaviour
{
    public Vector3 localPlace {get; set;}
    public Vector2 gridPosition {get; set;}
    public string tileName {get; set;}
    private bool hasUnit {get ; set;}

    public BattleTile(Vector3 place, Vector2Int position, string name)
    {
        Debug.Log("made it to constructor");
        localPlace = place;
        Debug.Log("made it past place");
        gridPosition = position;
        Debug.Log("made it past pos");
        tileName = name;
        Debug.Log("made it past name");
        hasUnit = false;
    }
}