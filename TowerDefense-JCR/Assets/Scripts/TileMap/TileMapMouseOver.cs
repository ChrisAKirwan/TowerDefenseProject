using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Core.TileMap;


namespace Core.TileMap
{
    [RequireComponent(typeof(TileMap))]
    public class TileMapMouseOver : MonoBehaviour
    {
        // TileMapMouseOver Properties *******************************************************************
        // ***********************************************************************************************

        private TileMap tileMap;
        private Vector2 tileCoord;
        private Vector3 hideCube = new Vector3(-100.0f, 100.0f, -100.0f);
        public GameObject highlightCube;

        // Native Functions ******************************************************************************
        // ***********************************************************************************************

        private void Start()
        {
            // Set the tilemap, create the highlighter object, and immediately spawn it off-camera.
            InitHighlightCube();

        }
        

        // Custom Functions ******************************************************************************
        // ***********************************************************************************************

        /// <summary>
        /// Destroys the highlightCube from the scene.
        /// </summary>
        void RemoveHighlightCube()
        {
            Destroy(highlightCube);
        }

        /// <summary>
        /// Sets the tilemap, creates the highlighter object, and immediately spawns it off-camera.
        /// </summary>
        void InitHighlightCube()
        {
            tileMap = this.GetComponent<TileMap>();
            highlightCube = Instantiate(highlightCube, hideCube, Quaternion.identity);
            highlightCube.transform.localScale = new Vector3(tileMap.tileSize, tileMap.tileSize * 2.0f, tileMap.tileSize);
            highlightCube.transform.GetChild(0).transform.position += Vector3.up * tileMap.tileSize / 2.0f;
            highlightCube.transform.parent = tileMap.transform;
        }

        // Update is called once per frame
        void Update()
        {
            // Raycast to the tilemap, obtain the coordinates, and snap the highlighter to those coordinates.
            // If you're not on the map, hide the highlighter.
            tileCoord = tileMap.GetMapGridCoordAtCursor();
            if (tileCoord.y >= 0)
            {
                Vector3 newPos = new Vector3(tileCoord.x * tileMap.tileSize, 0, tileCoord.y * tileMap.tileSize);
                highlightCube.transform.position = newPos;
            }
            else
            {
                highlightCube.transform.position = hideCube;
            }
        }
    }
}

