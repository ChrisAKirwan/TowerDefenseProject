using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

namespace Core.TileMap
{

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class TileMap : MonoBehaviour
    {
        // TileMap Properties ****************************************************************************
        // ***********************************************************************************************
        public int size_x { get; private set; }
        public int size_z { get; private set; }

        [Header("Tile Size (X by X units)")]
        public float tileSize = 2.0f;
        private Mesh mesh;

        // Assign our mesh to our mesh components
        private MeshFilter mesh_filter;
        private MeshRenderer mesh_renderer;
        private MeshCollider mesh_collider;

        // Palettes for generating maps. Filled in via editor.
        [Header("GameObject Palettes for this map")]
        public List<GameObject> tilePalette;
        public List<Material> materialPalette;
        public List<GameObject> propPalette;
        public List<GameObject> structurePalette;
        public List<GameObject> unitPalette;
        public List<GameObject> wallPalette;

        // List of Active GameObjects
        public TileContents[] TileList { get; private set; }
        public class TileContents
        {
            public MapTile tileObject;
            public Vector2 coord;
            public List<GameObject> OnTopOfTile;
            public bool isEmpty = true;
        }

        public LevelScript levelRef { get; private set; }
        private MapEditorManager mapManagerRef;



        // Native Functions ******************************************************************************
        // ***********************************************************************************************

        void Awake()
        {
            levelRef = FindObjectOfType<LevelScript>();
            mapManagerRef = levelRef.GetComponent<MapEditorManager>();
        }

        // Start is called before the first frame update
        void Start()
        {
            // Disable update function.
            this.enabled = false;
        }


        // Custom Functions ******************************************************************************
        // ***********************************************************************************************

        public void CreateTileMapFromSize(int numRows, int numCols)
        {
            size_x = numRows;
            size_z = numCols;

            InitTileMap();
            BuildMesh();
        }

        /// <summary>
        /// Initializes all components of the tilemap, and prepares the list of tiles.
        /// </summary>
        public void InitTileMap()
        {
            mesh = new Mesh();
            mesh_filter = GetComponent<MeshFilter>();
            mesh_renderer = GetComponent<MeshRenderer>();
            mesh_collider = GetComponent<MeshCollider>();
            TileList = new TileContents[size_x * size_z];

            for (int i = 0; i < size_x * size_z; i++)
            {
                TileList[i] = new TileContents();
                TileList[i].OnTopOfTile = new List<GameObject>();
                TileList[i].isEmpty = true;
            }
        }

        /// <summary>
        /// Generates the tile grid using the size_x, size_z, and tileSize properties.
        /// </summary>
        void BuildMesh()
        {
            int numTiles = size_x * size_z;
            int numTris = numTiles * 2;

            int vsize_x = size_x + 1;
            int vsize_z = size_z + 1;
            int numVerts = vsize_x * vsize_z;

            // Generate mesh data
            Vector3[] vertices = new Vector3[numVerts];
            Vector3[] normals = new Vector3[numVerts];
            Vector2[] uv = new Vector2[numVerts];

            int[] triangles = new int[numTris * 3];


            for (int z = 0; z < vsize_z; z++)
            {
                for (int x = 0; x < vsize_x; x++)
                {
                    vertices[z * vsize_x + x] = new Vector3(x * tileSize, 0, z * tileSize);
                    normals[z * vsize_x + x] = Vector3.up;
                    uv[z * vsize_x + x] = new Vector2((float)x / vsize_x, (float)z / vsize_z);
                }
            }

            for (int z = 0; z < size_z; z++)
            {
                for (int x = 0; x < size_x; x++)
                {
                    int squareIndex = z * size_x + x;
                    int triOffset = squareIndex * 6;

                    // Triangle 1
                    triangles[triOffset + 0] = z * vsize_x + x + 0;
                    triangles[triOffset + 1] = z * vsize_x + x + vsize_x + 0;
                    triangles[triOffset + 2] = z * vsize_x + x + vsize_x + 1;

                    // Triangle 2
                    triangles[triOffset + 3] = z * vsize_x + x + 0;
                    triangles[triOffset + 4] = z * vsize_x + x + vsize_x + 1;
                    triangles[triOffset + 5] = z * vsize_x + x + 1;
                }
            }

            // Create a new mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.uv = uv;


            mesh_filter.mesh = mesh;
            mesh_collider.sharedMesh = mesh;
        }


        public Vector2 GetTileCoordFromWorldPos(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt(worldPos.x / tileSize);
            int y = Mathf.FloorToInt(worldPos.z / tileSize);

            return new Vector2(x, y);
        }

        /// <summary>
        /// Returns a Vector2 grid coordinate at the cursor location.
        /// Note: THIS IS A RAYTRACE TO THE ACTUAL GRID, NOT THE TILE.
        /// If a tile coordinate is needed by raycasting to a tile, use
        /// GetTileCoordAtCursorPosition()!
        /// </summary>
        /// <returns></returns>
        public Vector2 GetMapGridCoordAtCursor()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (mesh_collider.Raycast(ray, out hitInfo, Mathf.Infinity))
            {
                int x = Mathf.FloorToInt(hitInfo.point.x / tileSize);
                int y = Mathf.FloorToInt(hitInfo.point.z / tileSize);

                return new Vector2(x, y);
            }
            else
                return new Vector2(-1, -1);
        }

        /// <summary>
        /// Returns a tile details object from a Vector2 grid coordinate.
        /// </summary>
        /// <param name="mapGridCoord"></param>
        /// <returns></returns>
        public MapTile GetTileAtMapGridCoord(Vector2 mapGridCoord)
        {
            if (IsCoordWithinMap(mapGridCoord))
            {
                int x = Mathf.FloorToInt(mapGridCoord.x);
                int y = Mathf.FloorToInt(mapGridCoord.y);

                return TileList[y * size_x + x].tileObject;
            }
            else
                return null;
        }

        /// <summary>
        /// Raycasts to the tile the mouse is over and returns the tile.
        /// Returns null if a tile wasn't found.
        /// </summary>
        /// <returns></returns>
        public MapTile GetTileAtCursorPosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            int layerMask = 1 << 8;

            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask))
            {
                int x = Mathf.FloorToInt(hitInfo.point.x / tileSize);
                int y = Mathf.FloorToInt(hitInfo.point.z / tileSize);

                Vector2 coord = new Vector2(x, y);
                return GetTileAtMapGridCoord(coord);
            }
            else
                return null;
        }

        /// <summary>
        /// Raycasts to the tile the mouse is over and returns the coordinates
        /// of that tile. Returns a -1, -1 vector2 if a tile wasn't found.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetTileCoordAtCursorPosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            int layerMask = 1 << 8;

            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask))
            {
                int x = Mathf.FloorToInt(hitInfo.point.x / tileSize);
                int y = Mathf.FloorToInt(hitInfo.point.z / tileSize);

                return new Vector2(x, y);
            }
            else
                return new Vector2(-1, -1);
        }

        /// <summary>
        /// Returns a handle to an instantiated tile GameObject, or null, if one was unable
        /// to be spawned.
        /// 
        /// Takes a tile object to spawn, and the rotation to orient it.
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="rot"></param>
        /// <returns></returns>
        public MapTile SetTileAtCursorLocation(MapTile tile, Quaternion rot)
        {
            Vector2 location = GetMapGridCoordAtCursor();
            int x = (int)location.x;
            int z = (int)location.y;

            float tilePosOffset = tileSize / 2.0f;

            // make sure we actually hit the map.
            if (location.y >= 0)
            {
                // make sure the grid space is actually empty
                if (TileList[z * size_x + x].isEmpty)
                {
                    float x_pos = location.x * tileSize + tileSize / 2.0f + tilePosOffset;
                    float z_pos = location.y * tileSize + tileSize / 2.0f + tilePosOffset;

                    MapTile temp = Instantiate(tile, new Vector3(x_pos, 0, z_pos), rot);
                    TileList[z * size_x + x].tileObject = temp;
                    TileList[z * size_x + x].coord = location;
                    TileList[z * size_x + x].isEmpty = false;
                    temp.transform.parent = this.transform;

                    return temp;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a handle to an instantiated tile, or null, if one was unable
        /// to be spawned.
        /// 
        /// Takes a tile object to spawn, the rotation to orient it, and the grid location to place it.
        /// </summary>
        /// <param name="tileCoord"></param>
        /// <param name="tile"></param>
        /// <param name="rot"></param>
        /// <returns></returns>
        public MapTile SetObjAtMapGridCoord(MapTile obj, Quaternion rot, Vector2 tileCoord)
        {
            if (!IsCoordWithinMap(tileCoord))
                return null;

            int x = (int)tileCoord.x;
            int z = (int)tileCoord.y;


            // make sure the grid space is actually empty
            if (!TileList[z * size_x + x].isEmpty)
                RemoveTileAtCoord(tileCoord);

            float x_pos = tileCoord.x * tileSize + tileSize / 2.0f;
            float z_pos = tileCoord.y * tileSize + tileSize / 2.0f;

            MapTile newTile = Instantiate(obj, new Vector3(x_pos, 0.0f, z_pos), rot);
            TileList[z * size_x + x].tileObject = newTile;
            TileList[z * size_x + x].coord = tileCoord;
            TileList[z * size_x + x].isEmpty = false;

            newTile.transform.parent = this.transform;
            newTile.hasProp = false;
            newTile.hasStructure = false;

            if (newTile.GetComponent<BoxCollider>() != null)
                newTile.isFlat = true;
            else
                newTile.isFlat = false;

            return newTile;
        }

        // Removes a tile at a given tile coordinate.
        public void RemoveTileAtCoord(Vector2 tileCoord)
        {
            int x = Mathf.FloorToInt(tileCoord.x);
            int y = Mathf.FloorToInt(tileCoord.y);

            Destroy(TileList[y * size_x + x].tileObject.gameObject);
            TileList[y * size_x + x].isEmpty = true;
        }

        private bool IsPlaceableOnTile(Vector2 coord)
        {
            if (!GetTileAtMapGridCoord(coord).isFlat ||
                (coord.x >= size_x - 2 && coord.y >= size_z - 2) ||
                GetTileAtMapGridCoord(coord).hasStructure)
                return false;

            return true;
        }

        /// <summary>
        /// Returns a handle to an instantiated GameObject, or null, if one was unable
        /// to be spawned. Will be spawned on top of a tile.
        /// 
        /// Takes an object to spawn, and the rotation to orient it.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="rot"></param>
        /// <returns></returns>
        public BaseStructureClass SetObjAtCursorLocation(BaseStructureClass obj, Quaternion rot)
        {
            Vector2 location = GetTileCoordAtCursorPosition();

            // make sure we actually hit the map.
            if (location.y >= 0 && IsPlaceableOnTile(location))
            {
                float x_pos;
                float y_pos = GetTopOfTileYPosition(location);
                float z_pos;

                x_pos = location.x * tileSize + tileSize / 2.0f;
                z_pos = location.y * tileSize + tileSize / 2.0f;

                BaseStructureClass newStructure = Instantiate(obj, new Vector3(x_pos, y_pos, z_pos), rot);

                MapTile tile = GetTileAtMapGridCoord(location);

                newStructure.transform.parent = tile.transform;
                newStructure.tileCoord = location;

                tile.hasStructure = true;
                tile.structure = newStructure;

                if(!levelRef.PC.inEditMode)
                    levelRef.GetComponent<SpawnManager>().spawnedStructures.Add(newStructure);

                return newStructure;
            }

            return null;
        }

        public Prop SetObjAtCursorLocation(Prop obj, Quaternion rot)
        {
            Vector2 location = GetTileCoordAtCursorPosition();

            // make sure we actually hit the map.
            if (location.y >= 0 && IsPlaceableOnTile(location))
            {
                float x_pos;
                float y_pos = GetTopOfTileYPosition(location);
                float z_pos;

                x_pos = Random.Range(location.x * tileSize + tileSize * 0.4f, location.x * tileSize + tileSize * 0.6f);
                z_pos = Random.Range(location.y * tileSize + tileSize * 0.4f, location.y * tileSize + tileSize * 0.6f);

                Prop newProp = Instantiate(obj, new Vector3(x_pos, y_pos, z_pos), rot);

                MapTile tile = GetTileAtMapGridCoord(location);

                if (tile.hasProp)
                    Destroy(tile.prop.gameObject);

                newProp.transform.parent = tile.transform;
                tile.hasProp = true;
                tile.prop = newProp;

                return newProp;
            }

            return null;
        }


        /// <summary>
        /// Returns a handle to an instantiated GameObject, or null, if one was unable
        /// to be spawned. Will be spawned on top of a tile.
        /// 
        /// Takes an object to spawn, the rotation to orient it, and the grid coordinate to spawn it.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="rot"></param>
        /// <param name="coord"></param>
        /// <returns></returns>
        public BaseStructureClass SetObjAtMapGridCoord(BaseStructureClass obj, Quaternion rot, Vector2 coord)
        {
            if (!GetTileAtMapGridCoord(coord).isFlat || !IsCoordWithinMap(coord))
                return null;

            int x = (int)coord.x;
            int z = (int)coord.y;

            float x_pos;
            float y_pos = GetTopOfTileYPosition(coord);
            float z_pos;

            x_pos = x * tileSize + tileSize / 2.0f;
            z_pos = z * tileSize + tileSize / 2.0f;

            BaseStructureClass newStructure = Instantiate(obj, new Vector3(x_pos, y_pos, z_pos), rot);

            MapTile tile = GetTileAtMapGridCoord(coord);
            newStructure.transform.parent = tile.transform;
            newStructure.tileCoord = coord;

            tile.hasStructure = true;
            tile.structure = newStructure;

            if (!levelRef.PC.inEditMode)
                levelRef.GetComponent<SpawnManager>().spawnedStructures.Add(newStructure);

            return newStructure;
        }

        public Prop SetObjAtMapGridCoord(Prop obj, Quaternion rot, Vector2 coord)
        {
            if (!GetTileAtMapGridCoord(coord).isFlat || !IsCoordWithinMap(coord))
                return null;

            float x_pos;
            float y_pos = GetTopOfTileYPosition(coord);
            float z_pos;

            // Decorations/objects on top should be spawned with a randomized offset.
            x_pos = Random.Range(coord.x * tileSize + tileSize * 0.25f, coord.x * tileSize + tileSize * 0.75f);
            z_pos = Random.Range(coord.y * tileSize + tileSize * 0.25f, coord.y * tileSize + tileSize * 0.75f);

            Prop newProp = Instantiate(obj, new Vector3(x_pos, y_pos, z_pos), rot);
            MapTile tile = GetTileAtMapGridCoord(coord);

            newProp.transform.parent = tile.transform;
            tile.hasProp = true;
            tile.prop = newProp;

            return newProp;
        }

        public BaseUnitType SetObjAtMapGridCoord(BaseUnitType obj, Quaternion rot, Vector2 coord)
        {
            if (!GetTileAtMapGridCoord(coord).isFlat || !IsCoordWithinMap(coord))
                return null;

            float posOffset = 0;//  tileSize / 2.0f;

            float x_pos;
            float y_pos = GetTopOfTileYPosition(coord);
            float z_pos;

            // Decorations/objects on top should be spawned with a randomized offset.
            x_pos = Random.Range(coord.x * tileSize + tileSize * 0.25f, coord.x * tileSize + tileSize * 0.75f) + posOffset;
            z_pos = Random.Range(coord.y * tileSize + tileSize * 0.25f, coord.y * tileSize + tileSize * 0.75f) + posOffset;

            BaseUnitType newUnit = Instantiate(obj, new Vector3(x_pos, y_pos, z_pos), rot);
            newUnit.transform.parent = this.transform;

            if (!levelRef.PC.inEditMode)
                levelRef.GetComponent<SpawnManager>().spawnedEnemies.Add(newUnit);

            return newUnit;
        }


        /// <summary>
        /// Returns the y-position of the top of a tile at the provided grid coordinate.
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public float GetTopOfTileYPosition(Vector2 coord)
        {
            int index = Mathf.FloorToInt(coord.y * this.size_x + coord.x);
            if (TileList[index].isEmpty)
                return 0.0f;
            else
                return TileList[index].tileObject.transform.GetChild(0).transform.position.y;
        }

        /// <summary>
        /// Returns whether a map grid space at a provided coordinate has an associated tile.
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public bool IsGridSpaceEmpty(Vector2 coord)
        {
            if (coord.x >= size_x || coord.x < 0 ||
                coord.y >= size_z || coord.y < 0)
            {
                Debug.Log("Invalid coordinate. Coord requested: (" + coord.x.ToString() + ", " + coord.y.ToString() + "). " +
                    "Maximum Values: (" + size_x.ToString() + ", " + size_z.ToString() + ").");
                return false;
            }

            int x = (int)coord.x;
            int z = (int)coord.y;

            if (TileList[z * size_x + x].isEmpty)
                return true;

            return false;
        }

        private void SetTileNeighbors()
        {
            Vector2 tileACoord = new Vector2(0, 0);
            Vector2 tileBCoord = new Vector2(0, 0);

            MapTile currentTile;
            Transform tileEdges;
            float currentTileRotation;
            int edgeIndex;

            int numEdges;


            for (int z = 0; z < size_z; z++)
            {
                for (int x = 0; x < size_x; x++)
                {
                    tileACoord.x = x;
                    tileACoord.y = z;
                    currentTile = GetTileAtMapGridCoord(tileACoord);
                    tileEdges = currentTile.transform.GetChild(1);
                    numEdges = tileEdges.childCount;
                    currentTileRotation = currentTile.transform.rotation.eulerAngles.y;

                    // North-Edge is edge index 0 (with default rotation);
                    edgeIndex = Mathf.RoundToInt(currentTileRotation / 90.0f);
                    edgeIndex = (numEdges - edgeIndex) % numEdges;

                    tileBCoord.x = tileACoord.x;
                    tileBCoord.y = tileACoord.y + 1;

                    //CheckNeighbors(tileACoord, tileBCoord);
                    CheckNeighborAtEdge(tileEdges.GetChild(edgeIndex), tileBCoord);


                    // East-Edge is edge index 1 (with default rotation);
                    edgeIndex = (edgeIndex + 1) % numEdges;

                    tileBCoord.x++;
                    tileBCoord.y--;

                    //CheckNeighbors(tileACoord, tileBCoord);
                    CheckNeighborAtEdge(tileEdges.GetChild(edgeIndex), tileBCoord);
                }
            }
        }

        private bool IsCoordWithinMap(Vector2 coord)
        {
            if (coord.x >= 0 && coord.x < size_x &&
                coord.y >= 0 && coord.y < size_z)
                return true;

            return false;
        }

        private void CheckNeighborAtEdge(Transform tileAEdge, Vector2 tileBCoord)
        {
            MapTile tileA = tileAEdge.parent.parent.GetComponent<MapTile>();

            if (IsCoordWithinMap(tileBCoord))
            {
                Vector3 tileAWorldPos = this.transform.TransformPoint(tileAEdge.position);
                Vector3 tileBWorldPos;

                MapTile tileB = GetTileAtMapGridCoord(tileBCoord);

                foreach (Transform edge in tileB.transform.GetChild(1))
                {
                    tileBWorldPos = this.transform.TransformPoint(edge.transform.position);
                    if(tileAWorldPos == tileBWorldPos)
                    {
                        tileA.neighbors.Add(tileB);
                        tileB.neighbors.Add(tileA);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Takes a map generated by "LoadMapFromFile()" and 
        /// </summary>
        /// <param name="SavedMap"></param>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <returns></returns>
        public bool GenerateMapFromList(List<List<int>> SavedMap)
        {
            // Ensure that there are enough elements in the list to fill the entire map, and enough
            // resources available in our palettes to match the map requests.
            if (!IsSavedMapFileValid(SavedMap))
                return false;

            // Build the map using the requested map size.
            size_x = SavedMap[0][0];
            size_z = SavedMap[0][1];
            InitTileMap();
            BuildMesh();

            // Coordinate variables to identify which tile to use.
            float x_pos;
            float z_pos;

            // SavedMap properties
            int tileIndex;
            float y_rotation;
            int matIndex;
            int propIndex;

            // Tile attributes
            MapTile tilePrefab;
            Prop propPrefab;
            Quaternion propRot;
            float prop_YPos;

            MapTile temp_tile;
            Prop temp_prop;

            // Start at Bottom-Left Corner (0, 0), and fill each tile along x-axis.
            // Then, increment z and perform the same steps on the next row.

            // offset adjusts the first index of the savedmap list, which
            // represents the size_x, size_z variables, and not a tile.
            int index;
            int listOffset = 1;
            float tilePosOffset = 0; // tileSize / 2.0f;
            for (int z = 0; z < size_z; z++)
            {
                for (int x = 0; x < size_x; x++)
                {
                    index = (z * size_x + x) + listOffset;

                    // Assign the tile properties into variable indices.
                    tileIndex  = SavedMap[index][0];
                    y_rotation = SavedMap[index][1] * 90.0f;
                    matIndex   = SavedMap[index][2];
                    propIndex  = SavedMap[index][3];

                    // Use the map's palettes for associating attributes via integer index.
                    tilePrefab = mapManagerRef.tilePalette[tileIndex];

                    // Generate the tile block.
                    x_pos = x * tileSize + tileSize / 2.0f + tilePosOffset;
                    z_pos = z * tileSize + tileSize / 2.0f + tilePosOffset;
                    temp_tile = Instantiate(tilePrefab, new Vector3(x_pos, 0, z_pos), Quaternion.Euler(0, y_rotation, 0));
                    temp_tile.tileIndex = tileIndex;
                    if (temp_tile.GetComponent<BoxCollider>() != null)
                        temp_tile.isFlat = true;
                    else
                        temp_tile.isFlat = false;

                    // Set the tile's rotation.
                    //temp_tile.transform.GetChild(0).transform.eulerAngles = new Vector3(0, y_rotation, 0);

                    // Set the tile's material.
                    temp_tile.GetComponent<Renderer>().material = mapManagerRef.materialPalette[matIndex];
                    temp_tile.materialIndex = matIndex;

                    // Add the tile to the tile list.
                    TileList[index - listOffset].tileObject = temp_tile;
                    TileList[index - listOffset].coord = new Vector2(x, z);
                    TileList[index - listOffset].isEmpty = false;
                    temp_tile.transform.parent = this.transform;


                    // if the saved map tile's prop is -1, there is no prop object. Skip instantiation.
                    if (propIndex < 0)
                        continue;

                    // Generate the decal/decor on top of the block using an offset.
                    // positional offset will be within the center 75% of the block, at random.
                    // rotational direction is on y-axis and random.
                    propPrefab = mapManagerRef.propPalette[propIndex];
                    x_pos = Random.Range(x * tileSize + tileSize * 0.25f, x * tileSize + tileSize * 0.75f);
                    z_pos = Random.Range(z * tileSize + tileSize * 0.25f, z * tileSize + tileSize * 0.75f);
                    prop_YPos = tilePrefab.transform.GetChild(1).transform.position.y;
                    propRot = Quaternion.Euler(0, Random.Range(0, 359), 0);
                    temp_prop = Instantiate(propPrefab, new Vector3(x_pos, prop_YPos, z_pos), propRot);

                    temp_tile.propIndex = propIndex;
                    temp_tile.hasProp = true;
                    temp_tile.prop = temp_prop;

                    temp_prop.transform.parent = temp_tile.transform;
                }
            }

            SetTileNeighbors();

            return true;
        }

        /// <summary>
        /// Checks the validity of a map file (Represented in a List<List<int>> form).
        /// Returns false if map file is invalid.
        /// </summary>
        /// <param name="SavedMap"></param>
        /// <param name="mapSettings"></param>
        /// <returns></returns>
        private bool IsSavedMapFileValid(List<List<int>> SavedMap)
        {
            // Ensure that there are enough elements in the list to fill the entire map.
            if (SavedMap.Count - 1 != SavedMap[0][0] * SavedMap[0][1])
            {
                Debug.Log("Invalid Mapfile. NumTiles does not match num rows, cols. Num Rows/Cols: " + 
                    SavedMap[0][0].ToString() + "/" + SavedMap[0][1].ToString() + ". Num tiles: " + SavedMap.Count.ToString());
                return false;
            }

            int tileCount = mapManagerRef.tilePalette.Count;
            int matCount  = mapManagerRef.materialPalette.Count;
            int propCount = mapManagerRef.propPalette.Count;

            // Validate the palettes have been filled.
            if (tileCount == 0)
            {
                Debug.LogError("No tiles assigned to tilePalette in TileMap prefab!");
                return false;
            }
            if(matCount == 0)
            {
                Debug.LogError("No materials assigned to materialPaletter in TileMap prefab!");
                return false;
            }
            else if (propCount == 0)
            {
                Debug.LogError("No props assigned to propPalette in TileMap prefab!");
                return false;
            }
            else { }

            // Check for valid integer ranges in the saved map file based on available tile resources.
            for (int i = 1; i < SavedMap.Count; i++)
            {
                if (SavedMap[i][0] >= tileCount)
                {
                    Debug.LogError("Not enough tiles assigned to tilePalette in TileMap prefab based on imported map!");
                    return false;
                }
                if(SavedMap[i][2] >= matCount)
                {
                    Debug.LogError("Not enough materials assigned to materialPalette in TileMap prefab based on imported map!");
                    return false;
                }
                else if (SavedMap[i][3] >= propCount)
                {
                    Debug.LogError("Not enough decor objects assigned to decorPalette in TileMap prefab based on imported map!");
                    return false;
                }
                else { continue; }
            }

            return true;
        }

        private bool IsTileListFull()
        {
            for (int i = 0; i < size_x * size_z; i++)
            {
                if (TileList[i].isEmpty)
                    return false;
            }

            return true;
        }

        private bool AreAllTilesAccessible()
        {
            for(int i = 0; i < size_x * size_z; i++)
            {
                if (TileList[i].tileObject.neighbors.Count == 0)
                    return false;
            }

            return true;
        }

        public bool IsCurrentMapValid(out string errorText)
        {
            if (IsTileListFull())
            {
                SetTileNeighbors();

                if(!AreAllTilesAccessible())
                {
                    errorText = "Tiles Inaccessible!";
                    return false;
                }
            }
            else
            {
                errorText = "Missing Tiles!";
                return false;
            }

            errorText = "Map is valid!";
            return true;
        }
    }
}
