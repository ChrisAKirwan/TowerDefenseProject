using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.TileMap;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    // Player Controller Properties ******************************************************************
    // ***********************************************************************************************

    // Bottom-Left and Top-Right camera bounding box. Camera should remain within these bounds.
    [Header("Camera Movement")]
    [SerializeField] private float cameraSpeed = 20.0f;
    private Vector3 cameraBoundsBottomLeft;
    private Vector3 cameraBoundsTopRight;
    private bool isCameraInBounds;

    // Selection GameObjects. Objects can be MapTile, Structure, or GameObject prop.
    private GameObject selection;
    private GameObject preview;
    private bool isHoldingObject = false;
    private Vector3 previewCoord;

    // MapEditor Properties
    public bool inEditMode = false;
    public int tile_index = 0;
    public int mat_index = 0;
    public int prop_index = 0;
    public enum CurrentEditMode { NO_OBJ, TILE, TILEMAT, PROP }
    public CurrentEditMode editState { get; private set; }
    private float scrollDelta = 0.0f;

    // Specific Asset References
    private Castle castleRef;

    // GameMode generic properties:
    [HideInInspector] public MapEditorManager mapManagerRef;
    [HideInInspector] public LevelScript levelRef;

    // PlayMode Specific Properties
    private SpawnManager spawnManagerRef;
    private MenuManager UIMenuRef;



    // Native Functions ******************************************************************************
    // ***********************************************************************************************
    private void Awake()
    {
        this.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Check for player input to move the camera.
        ProcessCameraMovement();

        // Is the map editor active?
        if (inEditMode)
        {
            UpdateEditMode();
        }
        else
            UpdatePlayMode();

        MovePreviewAtCursor();
    }

    // Update function for the MapEditor
    private void UpdateEditMode()
    {
        if (scrollDelta != 0)
        {
            MapEditorScrollInput();
        }
        else if(Input.GetKeyUp(KeyCode.Q))
        {
            MapEditorRotateInput(1);
        }
        else if(Input.GetKeyUp(KeyCode.E))
        {
            MapEditorRotateInput(-1);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            editState = CurrentEditMode.TILE;
            if (preview != null)
                Destroy(preview);

            //preview = Instantiate(mapManagerRef.tilePalette[tile_index]).gameObject;
            //isHoldingObject = true;
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            editState = CurrentEditMode.TILEMAT;
            if (preview != null)
                Destroy(preview);

            //isHoldingObject = false;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                editState = CurrentEditMode.PROP;
                if (preview != null)
                    Destroy(preview);

                //preview = Instantiate(mapManagerRef.propPalette[prop_index]).gameObject;
                //preview.GetComponent<Prop>().isPreviewObject = true;
                //isHoldingObject = true;
            }
        }

        scrollDelta = Input.mouseScrollDelta.y;
    }

    private void UpdatePlayMode()
    {
        if (levelRef.GetComponent<MainLevel>().gameState == MainLevel.GameState.PLACECASTLE)
        {
            if (Time.timeSinceLevelLoad > levelRef.GetComponent<MainLevel>().castlePromptTime)
            {
                PlaceStructure(castleRef);
            }
        }
        else // Build/Combat Phases
        {
            if(Input.GetMouseButtonDown(0))
            {
                PlayModeClickInput();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            PauseGame();
    }

    // Init function for launching the playercontroller in the map editor.
    public void InitEditMode()
    {
        mat_index = 0;
        editState = CurrentEditMode.NO_OBJ;
        levelRef = FindObjectOfType<MapEditorLevel>();
        mapManagerRef = levelRef.GetComponent<MapEditorManager>();
        SetCameraRails();

        this.enabled = true;
    }

    // Init function for launching the playercontroller in play mode.
    public void InitPlayMode()
    {
        levelRef = FindObjectOfType<MainLevel>();

        SetCameraRails();

        UIMenuRef = levelRef.GetComponent<MenuManager>();
        spawnManagerRef = levelRef.GetComponent<SpawnManager>();
        mapManagerRef = levelRef.GetComponent<MapEditorManager>();

        // Index 0 of the tileMap's structure Palette is the Castle Prefab.
        castleRef = spawnManagerRef.castleRef;

        levelRef.GetComponent<MainLevel>().SetGameState(MainLevel.GameState.PLACECASTLE);
        UIMenuRef.OpenMenu(UIMenuRef.placeCastleNotif);

        this.enabled = true;
    }

    // Custom Functions ******************************************************************************
    // ***********************************************************************************************

    /// <summary>
    /// Dynamically reads the tile map and locates the bottom-left corner to
    /// identify the bottom-most boundary, and the left-most boundary. Then,
    /// locates the top-right corner and identifies the top-most boundary and
    /// the right-most boundary for the rails.
    /// Additionally, positions the camera at the bottom-left corner within those rails,
    /// regardless of where the camera originated in world space.
    /// </summary>
    void SetCameraRails()
    {
        // Find the offset for the camera "rails" at bottom and left
        float x = levelRef.tileMapRef.size_x * levelRef.tileMapRef.tileSize * 0.05f;
        float y = levelRef.tileMapRef.size_x * levelRef.tileMapRef.size_z * levelRef.tileMapRef.tileSize;
        float z = 0f;

        // Determine camera height. Might need some adjustments for various map sizing.
        if (y > 1000.0f)
            y = 15.0f;
        else if (y > 500)
            y = 12.0f;
        else
            y = 10.0f;

        Vector3 cameraOffset_BL = new Vector3(x, y, z);

        // Find the offset for the camera "rails" at top and right
        x = levelRef.tileMapRef.size_x * levelRef.tileMapRef.tileSize;
        z = levelRef.tileMapRef.size_z * levelRef.tileMapRef.tileSize * 0.85f;
        Vector3 cameraOffset_TR = new Vector3(x, y, z);

        cameraBoundsBottomLeft = cameraOffset_BL;
        cameraBoundsTopRight = cameraOffset_TR;
        transform.position = cameraBoundsBottomLeft;
    }

    /// <summary>
    /// Provides WASD movement controls for the camera based on camera rails.
    /// </summary>
    void ProcessCameraMovement()
    {
        isCameraInBounds = true;
        // Keep the player in-bounds along x-axis
        if (transform.position.x < cameraBoundsBottomLeft.x)
        {
            transform.position = new Vector3(cameraBoundsBottomLeft.x, transform.position.y, transform.position.z);
            isCameraInBounds = false;
        }
        else if (transform.position.x > cameraBoundsTopRight.x)
        {
            transform.position = new Vector3(cameraBoundsTopRight.x, transform.position.y, transform.position.z);
            isCameraInBounds = false;
        }
        else { }

        // Keep the player in-bounds along z-axis
        if (transform.position.z < cameraBoundsBottomLeft.z)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, cameraBoundsBottomLeft.z);
            isCameraInBounds = false;
        }
        else if (transform.position.z > cameraBoundsTopRight.z)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, cameraBoundsTopRight.z);
            isCameraInBounds = false;
        }
        else { }

        // Collect input and move camera. Checks current position to reduce camera jitter.
        if (isCameraInBounds)
        {
            if (Input.GetKey(KeyCode.W) && transform.position.z != cameraBoundsTopRight.z)
                transform.Translate(Vector3.forward * Time.deltaTime * cameraSpeed);
            else if (Input.GetKey(KeyCode.S) && transform.position.z != cameraBoundsBottomLeft.z)
                transform.Translate(Vector3.back * Time.deltaTime * cameraSpeed);
            else { }

            // Separately performing a z-axis check to allow diagonal movement.
            if (Input.GetKey(KeyCode.A) && transform.position.x != cameraBoundsBottomLeft.x)
                transform.Translate(Vector3.left * Time.deltaTime * cameraSpeed);
            else if (Input.GetKey(KeyCode.D) && transform.position.x != cameraBoundsTopRight.x)
                transform.Translate(Vector3.right * Time.deltaTime * cameraSpeed);
            else { }
        }
    }

    // sets the preview object to the castle object for placement.
    public void InitCastlePhase()
    {
        preview = Instantiate(castleRef, new Vector3(100.0f, 100.0f, 100.0f), Quaternion.Euler(0, 0, 0)).gameObject;
        preview.GetComponent<Castle>().isPreviewObject = true;
        isHoldingObject = true;
    }

    // Updates the preview object to the cursor location.
    void MovePreviewAtCursor()
    {
        // If we aren't holding anything, don't manage the preview object.
        if (!isHoldingObject || preview == null)
            return;

        // Prevent the preview from colliding with other objects.
        if (preview.GetComponent<Collider>() != null)
            preview.GetComponent<Collider>().enabled = false;

        // Get the current grid location and move the preview to the center of that spot.
        if (inEditMode && editState == CurrentEditMode.TILE)
            previewCoord = levelRef.tileMapRef.GetMapGridCoordAtCursor();
        else
            previewCoord = levelRef.tileMapRef.GetTileCoordAtCursorPosition();

        if (previewCoord.x >= 0)
        {
            float x_pos = previewCoord.x * levelRef.tileMapRef.tileSize + levelRef.tileMapRef.tileSize / 2.0f;
            
            float y_pos;
            if (inEditMode && editState == CurrentEditMode.TILE)
                y_pos = 0.0f;
            else
            {
                y_pos = levelRef.tileMapRef.GetTopOfTileYPosition(previewCoord);

                // If a tile exists and is not flat, move offscreen.
                if(y_pos > 0)
                    if (!levelRef.tileMapRef.GetTileAtCursorPosition().isFlat)
                        y_pos = 100.0f;
            }

            float z_pos = previewCoord.y * levelRef.tileMapRef.tileSize + levelRef.tileMapRef.tileSize / 2.0f;
            Vector3 newPos = new Vector3(x_pos, y_pos, z_pos);

            preview.transform.position = newPos;
        }
        else
        {
            Vector3 offGridPos = new Vector3(100.0f, 100.0f, 100.0f);
            preview.transform.position = offGridPos;
        }
    }

    // PlayMode function
    // Don't call if paused
    // if(Input.GetMouseButtonDown(0))
    // Right-click to dismiss UI -- UIMenuRef.CloseMenu(UIMenuRef.contextMenu);
    void PlayModeClickInput()
    {
        // If we hit our UI, don't do anything -- EventSystem will handle it.
        if (wasUIClicked())
            return;

        // We will only place something if we are actively holding something.
        if (isHoldingObject)
            PlaceStructure(selection.GetComponent<BaseStructureClass>());
        else
        {
            // Click was not on UI, and we are not holding anything. Grab the tile position.
            Vector2 clickCoord = levelRef.tileMapRef.GetTileCoordAtCursorPosition();

            // if we clicked somewhere on the map
            if (clickCoord.y >= 0)
            {
                levelRef.audioRef.PlaySFX("Click");

                // Make sure the context menu is open
                if (!UIMenuRef.contextMenu.isOpen)
                    UIMenuRef.OpenMenu(UIMenuRef.contextMenu);


                MapTile tile = levelRef.tileMapRef.GetTileAtMapGridCoord(clickCoord);

                // if the tile we clicked has a structure
                if (tile.hasStructure)
                {
                    BaseStructureClass structure = tile.structure.GetComponent<BaseStructureClass>();

                    UIMenuRef.FillDetailsMenu(structure);

                    // Close BuildMenu, if open
                    if (UIMenuRef.buildMenu.isOpen)
                        UIMenuRef.CloseMenu(UIMenuRef.buildMenu);

                    // Open buildingDetailsMenu;
                    UIMenuRef.OpenMenu(UIMenuRef.detailsMenu);
                }
                else
                {
                    // Close DetailsMenu, if open
                    if (UIMenuRef.detailsMenu.isOpen)
                        UIMenuRef.CloseMenu(UIMenuRef.detailsMenu);

                    // Open buildMenu;
                    UIMenuRef.OpenMenu(UIMenuRef.buildMenu);
                }
            }
        }
    }

    // MapEditor Input Functions *********************************************************************
    // ***********************************************************************************************

    /// <summary>
    /// Map Editor function to select a tile or object for placement.
    /// </summary>
    private void MapEditorScrollInput()
    {
        /*
         * if preview, destroy preview
         * 
         * switch editstate
         * on tile, spawn preview and move it to cursor loc on grid
         * 
         * on prop, set preview to prop and spawn on tile at cursor loc
         */


        if(preview != null)
            Destroy(preview);

        MapTile tile = null;

        switch (editState)
        {
            case CurrentEditMode.TILE:

                Vector2 tileCoord = levelRef.tileMapRef.GetTileCoordAtCursorPosition();

                tile_index += Mathf.RoundToInt(scrollDelta);
                tile_index += mapManagerRef.tilePalette.Count;
                tile_index %= mapManagerRef.tilePalette.Count;

                MapTile newTile = levelRef.tileMapRef.SetObjAtMapGridCoord(mapManagerRef.tilePalette[tile_index], Quaternion.Euler(0, 0, 0), tileCoord);

                if (newTile != null)
                {
                    newTile.tileIndex = tile_index;
                    newTile.GetComponent<Renderer>().material = mapManagerRef.materialPalette[mat_index];
                    newTile.materialIndex = mat_index;
                }

                break;
            case CurrentEditMode.TILEMAT:
                // Currently editing tile materials.
                tile = levelRef.tileMapRef.GetTileAtCursorPosition();

                // if a tile does not exist, we cannot edit its material.
                if (tile == null)
                    return;

                mat_index += Mathf.RoundToInt(scrollDelta);
                mat_index += mapManagerRef.materialPalette.Count;
                mat_index %= mapManagerRef.materialPalette.Count;

                tile.GetComponent<Renderer>().material = mapManagerRef.materialPalette[mat_index];
                tile.materialIndex = mat_index;

                break;
            case CurrentEditMode.PROP:
                // set preview to prop and spawn on tile at cursor loc
                tile = levelRef.tileMapRef.GetTileAtCursorPosition();

                // if a tile does not exist, we cannot place a prop.
                if (tile == null)
                {
                    Debug.Log("Prop Tile Null!");
                    return;
                }

                prop_index += Mathf.RoundToInt(scrollDelta);
                prop_index += mapManagerRef.propPalette.Count;
                prop_index %= mapManagerRef.propPalette.Count;

                levelRef.tileMapRef.SetObjAtCursorLocation(mapManagerRef.propPalette[prop_index], Quaternion.Euler(0, 0, 0));

                break;
            default:
                return;
        }

        if (levelRef.GetComponent<MapEditorLevel>().isMapClean)
            levelRef.GetComponent<MapEditorLevel>().DirtyMap();
    }

    /// <summary>
    /// Map Editor function to rotate the active object for placement.
    /// Direction should be a value of either '1' or '-1'.
    /// </summary>
    private void MapEditorRotateInput(int direction)
    {
        if (editState == CurrentEditMode.TILE)
        {
            MapTile tile = levelRef.tileMapRef.GetTileAtCursorPosition();
            tile.transform.Rotate(transform.up, 90.0f * direction);
        }
        else
        {
            if(editState == CurrentEditMode.PROP)
            {
                MapTile tile = levelRef.tileMapRef.GetTileAtCursorPosition();
                if(tile.hasProp)
                {
                    Prop prop = tile.prop;
                    prop.transform.Rotate(transform.up, 90.0f * direction);
                }
            }
        }
    }

    /// <summary>
    /// For initial placement of the castle at game-start.
    /// Places the player castle at user's choice.
    /// </summary>
    private void PlaceStructure(BaseStructureClass structure)
    {
        if (Input.GetMouseButtonDown(0) && !levelRef.GetComponent<MainLevel>().isPaused)
        {
            // If we hit our UI, don't do anything -- EventSystem will handle it.
            if (wasUIClicked())
                return;

            BaseStructureClass structureHandle = levelRef.tileMapRef.SetObjAtCursorLocation(structure, Quaternion.Euler(0, 0, 0));
            if(structureHandle == null)
            {
                // Invalid Placement;
                MapTile tile = levelRef.tileMapRef.GetTileAtCursorPosition();

                if (tile != null)
                {
                    tile.InvalidPlacement();
                    levelRef.audioRef.PlaySFX("ClickInvalid");
                }
                else
                    levelRef.audioRef.PlaySFX("Click");

                return;
            }

            Wall wallObj = structureHandle.GetComponent<Wall>();
            if (wallObj != null)
            {
                wallObj.currentShape = spawnManagerRef.wallTowerRef;
                wallObj.currentYRot = 0.0f;
                wallObj.UpdateShape();
            }

            if (levelRef.GetComponent<MainLevel>().gameState == MainLevel.GameState.PLACECASTLE)
            {
                levelRef.GetComponent<MainLevel>().SetCastleRef(structureHandle.GetComponent<Castle>());
                levelRef.GetComponent<MainLevel>().SetGameState(MainLevel.GameState.BUILDPHASE);
            }
            else
                levelRef.GetComponent<MainLevel>().RemoveCurrency(structure.structurePrice);

            Destroy(preview);
            isHoldingObject = false;
        }
    }


    public void SpawnStructureFromUIButton(BaseStructureClass structure)
    {
        selection = structure.gameObject;

        if (preview != null)
            Destroy(preview);

        preview = Instantiate(structure.gameObject, new Vector3(100.0f, 100.0f, 100.0f), Quaternion.Euler(0, 0, 0));
        isHoldingObject = true;
        preview.GetComponent<BaseStructureClass>().isPreviewObject = true;

        preview.GetComponent<BaseStructureClass>().SetEnabled(false);
    }

    public void PauseGame()
    {
        levelRef.GetComponent<MainLevel>().TogglePauseState();

        if(isHoldingObject)
            preview.SetActive(!levelRef.GetComponent<MainLevel>().isPaused);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private bool wasUIClicked()
    {
        // If we hit our UI, don't do anything -- EventSystem will handle it.
        if (EventSystem.current.IsPointerOverGameObject())
            return true;
        else
            return false;
    }

}
