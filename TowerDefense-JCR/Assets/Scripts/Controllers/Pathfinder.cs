using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.TileMap;

public class Pathfinder : MonoBehaviour
{
    private TileMap tileMapRef;
    private Vector2 currentCoord;

    void Awake()
    {
        tileMapRef = FindObjectOfType<TileMap>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Modify with a bool argument for skipping walls by default. If no path, go for walls. If still no path, ??? Broken map?
    public Stack<MapTile> BreadthFirstSearch(bool shouldAttackWalls = false)
    {
        Queue<MapTile> frontier = new Queue<MapTile>();
        currentCoord = tileMapRef.GetTileCoordFromWorldPos(this.transform.position);

        MapTile currentTile = tileMapRef.GetTileAtMapGridCoord(currentCoord);

        frontier.Enqueue(currentTile);
        Dictionary<MapTile, MapTile> cameFrom = new Dictionary<MapTile, MapTile>();
        cameFrom[currentTile] = null;


        MapTile checkTile;

        while(frontier.Count != 0)
        {
            checkTile = frontier.Dequeue();

            // Goal Found.
            if (checkTile.hasStructure && (checkTile.structure.GetComponent<Wall>() == null || shouldAttackWalls))
            {
                this.GetComponent<BaseUnitType>().SetTarget(checkTile.structure);
                Stack<MapTile> path = new Stack<MapTile>();
                while(cameFrom[checkTile] != null)
                {
                    path.Push(checkTile);
                    checkTile = cameFrom[checkTile];
                }

                return path;
            }

            for(int i = 0; i < checkTile.neighbors.Count; i++)
            {
                // Don't add tiles with walls to our path.
                if (checkTile.neighbors[i].hasStructure && checkTile.neighbors[i].structure.GetComponent<Wall>() != null && shouldAttackWalls == false)
                {
                    continue;
                }

                if (!cameFrom.ContainsKey(checkTile.neighbors[i]))
                {
                    frontier.Enqueue(checkTile.neighbors[i]);
                    cameFrom[checkTile.neighbors[i]] = checkTile;
                }
            }
        }

        if (!shouldAttackWalls)
            return BreadthFirstSearch(true);

        return null;
    }
}
