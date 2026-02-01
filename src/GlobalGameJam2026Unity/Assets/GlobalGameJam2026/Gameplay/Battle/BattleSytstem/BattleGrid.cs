using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class BattleGrid : MonoBehaviour
{
    
    public static BattleGrid instance;
    public GameObject tilePrefab;

    public Dictionary<Vector3, BattleTile> tiles;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        GetBattleGrid(10,10);
    }

    void GetBattleGrid(int height, int width)
    {
        Debug.Log("Woke up");
        tiles = new Dictionary<Vector3, BattleTile>();
        for(int c = 0; c < height; c++)
        {
            Vector2Int pos = new Vector2Int();
            pos.x = c;
            Debug.Log("done height at: " + c);
            for(int n = 0; n < width; n++)
            {   
                pos.y = n;
                Debug.Log("done width at: " + n);  
                Vector3 localPlace = new Vector3(c, 0, n);
                    BattleTile tile = new BattleTile(
                    localPlace,
                    pos,
                    "Tile: " + c + ", " + n
                );
                createTile(localPlace);
                tiles.Add(localPlace, tile);
            }
        }
    }

    public void createTile(Vector3 pos)
    {
        Instantiate(tilePrefab, pos, this.transform.rotation);
    }
}
