using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Core.TileMap;

public class MapEditorLevel : LevelScript
{
    // this.component references.
    private MapEditorManager mapEditorManagerRef;

    // LoadMap menu references (set by editor):
    [Header("Load Map Menu References")]
    [SerializeField] private RectTransform mapEditorUIRef;
    [SerializeField] private Button createMapButton_Editor;
    [SerializeField] private Button loadMapButton;

    // LoadMap private references
    private string editMapPath;
    private string editMapName;

    // CreateMap menu references (set by editor):
    [Header("Create Map Menu References")]
    [SerializeField] private RectTransform createNewMapUIRef;
    [SerializeField] private Button createMapButton_Creation;
    [SerializeField] private InputField mapNameField;
    [SerializeField] private Dropdown rowsDropdown;
    [SerializeField] private Dropdown colsDropdown;
    [SerializeField] private int minRowColSize = 5;

    // CreateMap private references
    private string mapName;
    private int numRows;
    private int numCols;

    // Editor HUD references
    [Header("Editor HUD References")]
    [SerializeField] private RectTransform editorHUDRef;
    [SerializeField] private Text errorText;
    [SerializeField] private Button checkMapButton;
    [SerializeField] private Button saveMapButton;
    public bool isMapClean { get; private set; }


    // Class init at GameStart
    void Awake()
    {
        // From LevelScript:
        this.PC = FindObjectOfType<PlayerController>();
        this.tileMapRef = FindObjectOfType<TileMap>();
        this.gameState = GameState.EDIT;
        this.audioRef = FindObjectOfType<AudioManager>();

        // From MapEditorLevel:
        this.mapEditorManagerRef = this.GetComponent<MapEditorManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // MapEditorLevel must be in EditMode.
        PC.inEditMode = true;

        // disable the Update() function, as it is not needed in this levelscript.
        //this.enabled = false;

        // Set the current edit map path to be listed on screen.
        SetEditMapPath();
        loadMapButton.transform.GetChild(1).GetComponent<Text>().text = "Current Map: " + editMapName;

        // disable the tilemap until we generate one.
        this.tileMapRef.gameObject.SetActive(false);

        isMapClean = false;

        this.audioRef.PlayMenuThemePlaylist();
    }


    // Switch to the CreateMap menu. 
    public void OpenCreateNewMapUI()
    {
        // Fill the dropdowns for Rows/Cols to valid sizes.
        FillDropDowns();

        // Set the current Row/Col to minimum value.
        numRows = minRowColSize;
        numCols = minRowColSize;

        // Switch the active MapEditor window with the Create New Map window.
        mapEditorUIRef.gameObject.SetActive(false);
        createNewMapUIRef.gameObject.SetActive(true);
    }

    // CreateMap function.
    // Called when the mapname field has been modified.
    // Sets the mapName variable, checks for validity, and disables the 
    // create map button if it is not.
    public void SetMapName()
    {
        mapName = mapNameField.text;

        if (AreNewMapSettingsValid())
            createMapButton_Creation.interactable = true;
        else
            createMapButton_Creation.interactable = false;
    }

    // CreateMap function
    // Checks if the name is valid and rows/cols are within bounds
    private bool AreNewMapSettingsValid()
    {
        if (IsValidMapName() && numRows >= minRowColSize && numCols >= minRowColSize)
            return true;
        return false;
    }

    // CreateMap function.
    // Checks if the mapName variable is a valid name.
    public bool IsValidMapName()
    {
        return System.Text.RegularExpressions.Regex.IsMatch(mapName, "^[a-zA-Z0-9]*$");
    }

    // CreateMap function.
    // Called when Rows dropdown value is changed.
    public void SetNumRows()
    {
        numRows = rowsDropdown.value + minRowColSize;

        // Check if the map settings are valid and toggle create map button
        // appropriately.
        if (AreNewMapSettingsValid())
            createMapButton_Creation.interactable = true;
        else
            createMapButton_Creation.interactable = false;
    }

    // CreateMap function.
    // Called when Cols dropdown value is changed.
    public void SetNumCols()
    {
        numCols = colsDropdown.value + minRowColSize;

        // Check if the map settings are valid and toggle create map button
        // appropriately.
        if (AreNewMapSettingsValid())
            createMapButton_Creation.interactable = true;
        else
            createMapButton_Creation.interactable = false;
    }

    // CreateMap function.
    // Fill the dropdowns for Rows/Cols to valid sizes.
    private void FillDropDowns()
    {
        // Clear any existing options from both dropdowns.
        rowsDropdown.options.Clear();
        colsDropdown.options.Clear();

        // Add an option for each valid row/col size to each dropdown.
        for (int i = minRowColSize; i < 100; i++)
        {
            rowsDropdown.options.Add(new Dropdown.OptionData() { text = i.ToString() + " rows" });
            colsDropdown.options.Add(new Dropdown.OptionData() { text = i.ToString() + " columns" });
        }
    }

    // CreateMap function
    // Creates a new map using the set rows/cols
    public void EditNewMap()
    {
        // Create the tilemap
        List<List<int>> SavedMap = mapEditorManagerRef.LoadEmptyMapWithSize(numRows, numCols);
        if (!tileMapRef.GenerateMapFromList(SavedMap))
        {
            Debug.LogError("Failed to Generate New Map!");
            GoToScene("MainMenu");
            return;
        }

        // Remove the CreateMap UI menu
        createNewMapUIRef.gameObject.SetActive(false);

        // Set the tilemap to active,
        // and set our playercontroller to editmode.
        tileMapRef.gameObject.SetActive(true);
        editorHUDRef.gameObject.SetActive(true);
        PC.InitEditMode();
    }

    // LoadMap function.
    // Checks if an existing map is selected. Otherwise, switches to CreateMap menu.
    // If existing map does exist, open it, close the menus, enter edit mode.
    public void EditExistingMap()
    {
        // Check if an edit map was selected.
        if(editMapName == "Empty")
        {
            // If no map is selected, switch to CreateMap menu.
            OpenCreateNewMapUI();
            return;
        }

        // load the selected tilemap
        mapName = editMapName;
        string path = editMapPath + editMapName;
        List<List<int>> SavedMap = mapEditorManagerRef.LoadMapFromFile(path);
        if (!tileMapRef.GenerateMapFromList(SavedMap))
        {
            Debug.LogError("Failed to Generate Map from Saved File!");
            GoToScene("MainMenu");
            return;
        }

        // disable the LoadMap UI, enable the generated tilemap,
        // and set our playercontroller to editmode.
        mapEditorUIRef.gameObject.SetActive(false);
        tileMapRef.gameObject.SetActive(true);
        editorHUDRef.gameObject.SetActive(true);
        PC.InitEditMode();
    }

    // Sets the active edit map path variables.
    private void SetEditMapPath()
    {
        string path = Application.dataPath + "/SavedMaps/Selection.txt";
        if (!File.Exists(path))
        {
            Debug.LogError("No file found. [SetEditMapPath()]");
            return;
        }

        /*
         * File is organized as follows:
         * [0] CurrentPlayMap: [PlayMapName]
         * [1] [Custom/Default]
         * [2] CurrentEditMap: [EditMapName]
         * [3] [Custom/Default]
         */

        List<string> fileLines = File.ReadAllLines(path).ToList();
        string line = fileLines[2];

        // Split at ' ':
        // line[0]: CurrentEditMap:
        // line[1]: [EditMapName]
        editMapName = line.Split(' ')[1] + ".txt";

        // Is the map Default or Custom?
        line = fileLines[3];

        editMapPath = Application.dataPath + "/SavedMaps/";
        if (line == "Default")
            editMapPath += "DefaultMaps/";
        else
            editMapPath += "CustomMaps/";
    }

    public void CheckMap()
    {
        string errorString = "";
        if (tileMapRef.IsCurrentMapValid(out errorString))
        {
            errorText.gameObject.SetActive(false);
            isMapClean = true;
            saveMapButton.interactable = true;
        }
        else
        {
            isMapClean = false;
            errorText.text = errorString;
            errorText.gameObject.SetActive(true);
        }

    }

    public void DirtyMap()
    {
        isMapClean = false;
        saveMapButton.interactable = false;
    }

    public void SaveMap()
    {
        mapEditorManagerRef.SaveMapToFile(mapName);
    }
}
