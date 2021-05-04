using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.TileMap;
using System.Linq;

public class Wall : BaseStructureClass
{
    private TileMap tileMapRef;
    private SpawnManager spawnManagerRef;
    public Wall currentShape;
    public float currentYRot;
    public bool hasUpdated = false;

    void Awake()
    {
        tileMapRef = FindObjectOfType<TileMap>();
        spawnManagerRef = FindObjectOfType<SpawnManager>();
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public void UpdateShape()
    {
        hasUpdated = true;
        // Check North Tile:
        Vector2 neighborCoord = new Vector2(tileCoord.x, tileCoord.y + 1);
        bool hasWallNorth = DoesTileHaveWall(neighborCoord);

        // Check East Tile:
        neighborCoord = new Vector2(tileCoord.x + 1, tileCoord.y);
        bool hasWallEast = DoesTileHaveWall(neighborCoord);

        // Check South Tile:
        neighborCoord = new Vector2(tileCoord.x, tileCoord.y - 1);
        bool hasWallSouth = DoesTileHaveWall(neighborCoord);

        // Check West Tile:
        neighborCoord = new Vector2(tileCoord.x - 1, tileCoord.y);
        bool hasWallWest = DoesTileHaveWall(neighborCoord);

        float rot = 0;
        Wall wallShape = GetWallShapeAndRot(hasWallNorth, hasWallSouth, hasWallEast, hasWallWest, out rot);

        if (wallShape == currentShape &&
            rot == currentYRot)
        {
            hasUpdated = false;
            return;
        }
        else
        {
            RefreshWall(wallShape, rot, tileCoord);
        }

        hasUpdated = false;
    }

    public void RefreshWall(Wall wallPrefab, float wallYRot, Vector2 coord)
    {
        Destroy(tileMapRef.GetTileAtMapGridCoord(coord).structure.gameObject);
        Quaternion rot = Quaternion.Euler(0, wallYRot, 0);
        Wall wallRef = tileMapRef.SetObjAtMapGridCoord(wallPrefab, rot, coord).GetComponent<Wall>();
        wallRef.currentShape = wallPrefab;
        wallRef.currentYRot = wallYRot;
    }

    private bool IsWithinBounds(Vector2 coord)
    {
        if (coord.x >= tileMapRef.size_x ||
            coord.x < 0 ||
            coord.y >= tileMapRef.size_z ||
            coord.y < 0)
            return false;

        return true;
    }

    private bool DoesTileHaveWall(Vector2 coord)
    {
        if (IsWithinBounds(coord))
        {
            MapTile tile = tileMapRef.GetTileAtMapGridCoord(coord);
            if (tile == null)
            {
                Debug.LogError("Tile at coord " + coord.ToString() + " does not have a MapTile Component!");
                return false;
            }
            if (tile.hasStructure)
            {
                Wall wallObj = tile.structure.GetComponent<Wall>();

                if (wallObj != null)
                {
                    if (!wallObj.hasUpdated)
                        wallObj.UpdateShape();

                    return true;
                }
            }
        }
        return false;
    }

    /*
     * Wall Tower 0:    [] No orientation
     * 
     * Wall End 1:      -[]
     *                  __
     * Wall Corner 2:     |
     *                  
     * Wall Straight 2: |
     * 
     * Wall T 3:        |-
     * 
     * Wall Cross 4:    + No orientation
     */
    private Wall GetWallShapeAndRot(bool north, bool south, bool east, bool west, out float rot)
    {
        Wall wallShape;

        if(north && south && east && west)
        {
            // Wall shape is a cross (+)
            wallShape = spawnManagerRef.wallCrossRef;
            rot = 0.0f;
        }
        else if(north)
        {
            // Single, potentially double or triple.
            if(east)
            {
                // Double, potentially triple. Currently NE corner.
                if(south)
                {
                    // triple, NES orientation.
                    wallShape = spawnManagerRef.wallTRef;
                    rot = 0.0f;
                }
                else if(west)
                {
                    // triple, WNE orientation.
                    wallShape = spawnManagerRef.wallTRef;
                    rot = 270.0f;
                }
                else
                {
                    // double NE corner orientation.
                    wallShape = spawnManagerRef.wallCornerRef;
                    rot = 180.0f;
                }
            }
            else if(south)
            {
                // Double, potentially triple. Currently straight NS
                if(west)
                {
                    // triple. SWN orientation.
                    wallShape = spawnManagerRef.wallTRef;
                    rot = 180.0f;
                }
                else
                {
                    // double NS straight orientation.
                    wallShape = spawnManagerRef.wallStraightRef;
                    rot = 0.0f;
                }
            }
            else if(west)
            {
                // double NW corner orientation.
                wallShape = spawnManagerRef.wallCornerRef;
                rot = 90.0f;
            }
            else
            {
                // single N orientation.
                wallShape = spawnManagerRef.wallEndRef;
                rot = 90.0f;
            }
        }
        else if(east)
        {
            // Single, potentially double, triple orientation.
            if(south)
            {
                // Double, potentially triple. Currently SE corner orientation.
                if(west)
                {
                    // triple ESW orientation.
                    wallShape = spawnManagerRef.wallTRef;
                    rot = 90.0f;
                }
                else
                {
                    // double SE corner orientation
                    wallShape = spawnManagerRef.wallCornerRef;
                    rot = 270.0f;
                }
            }
            else if(west)
            {
                // double EW straight orientation.
                wallShape = spawnManagerRef.wallStraightRef;
                rot = 90.0f;
            }
            else
            {
                // single E orientation.
                wallShape = spawnManagerRef.wallEndRef;
                rot = 180.0f;
            }
        }
        else if(south)
        {
            // Single, potentially double orientation.
            if(west)
            {
                // double, SW corner orientation
                wallShape = spawnManagerRef.wallCornerRef;
                rot = 0.0f;
            }
            else
            {
                // single S orientation.
                wallShape = spawnManagerRef.wallEndRef;
                rot = 270.0f;
            }
        }
        else if(west)
        {
            // single W orientation
            wallShape = spawnManagerRef.wallEndRef;
            rot = 0.0f;
        }
        else
        {
            // index, rot zero; isolated wall.
            wallShape = spawnManagerRef.wallTowerRef;
            rot = 0.0f;
        }

        return wallShape;
    }

    public override void OnDeath()
    {
        base.OnDeath();
        UpdateShape();
    }
}
