using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadMapLevel : LevelScript
{
    // Default Map file paths
    private string defaultMapPath;
    private DirectoryInfo defaultMapDirectory;
    private FileInfo[] defaultMapFiles;

    // Custom Map file paths
    private string customMapPath;
    private DirectoryInfo customMapDirectory;
    private FileInfo[] customMapFiles;

    // MapList container elements
    [Header("Map List Container Reference")]
    public RectTransform mapList;
    [Header("Map List Prefab References")]
    public RectTransform defaultMapHeader;
    public RectTransform customMapHeader;
    public RectTransform mapFileEntry;
    private List<RectTransform> defaultMapEntries;
    private List<RectTransform> customMapEntries;

    // Current Map Readouts
    [Header("Current Map Text References")]
    private string currentPlayMap;
    public Text currentPlayMapUI;
    private string currentEditMap;
    public Text currentEditMapUI;

    // Current Map Selection Type
    private bool isDefaultPlayMap = true;
    private bool isDefaultEditMap = false;


    private void Awake()
    {
        // From LevelScript:
        this.gameState = GameState.EDIT;
        this.tileMapRef = null;
        this.PC = null;
        this.audioRef = FindObjectOfType<AudioManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Set the path variables.
        defaultMapPath = Application.dataPath + "/SavedMaps/" + "DefaultMaps";
        customMapPath = Application.dataPath + "/SavedMaps/" + "CustomMaps";

        // Load the files
        defaultMapDirectory = new DirectoryInfo(defaultMapPath);
        defaultMapFiles = defaultMapDirectory.GetFiles();
        customMapDirectory = new DirectoryInfo(customMapPath);
        customMapFiles = customMapDirectory.GetFiles();

        // Set the default play map to the first one in the default map list.
        currentPlayMap = defaultMapFiles[0].Name.Substring(0, defaultMapFiles[0].Name.Length - 4);
        currentPlayMapUI.text = currentPlayMap;

        // Set the default edit map to "Empty"
        // this will create a new map if the user does not set a different map.
        currentEditMap = "Empty";
        currentEditMapUI.text = currentEditMap;

        // MapEntry lists that contain a list of default and custom maps.
        defaultMapEntries = new List<RectTransform>();
        customMapEntries = new List<RectTransform>();

        // Creates the map UI elements in the MapList container.
        FillMapList();

        this.audioRef.PlayMenuThemePlaylist();

        // disable the Update() function, as it is not needed in this menu.
        this.enabled = false;
    }

    // Updates the UI with the currently set map names.
    private void UpdateCurrentMapUI()
    {
        currentPlayMapUI.text = currentPlayMap;
        currentEditMapUI.text = currentEditMap;
    }

    // Creates the map UI elements in the MapList container.
    private void FillMapList()
    {
        float yOffset = 0;
        Vector3 pos;

        /*
         * Entry:
         * - Child(0): PlayToggle
         * - Child(1): EditToggle
         * - Child(2): MapName
         */

        // Default Map List
        // Default Header
        RectTransform entry = Instantiate(defaultMapHeader, mapList, false);
        yOffset += entry.rect.height;

        // Default Map Entries
        for (int i = 0; i < defaultMapFiles.Length; i++)
        {
            // Skip any Unity meta files.
            if (defaultMapFiles[i].Name.Contains(".meta"))
                continue;

            // Create a new map entry offset to not overlap with the previous entry.
            entry = Instantiate(mapFileEntry, mapList, false);
            pos = entry.position;
            pos.y -= yOffset;
            entry.position = pos;
            yOffset += entry.rect.height;

            // Set the UI MapName
            entry.GetChild(2).GetChild(0).GetComponent<Text>().text = defaultMapFiles[i].Name.Substring(0, defaultMapFiles[i].Name.Length - 4);
            
            // Set the play toggle
            Toggle PlayToggle;
            PlayToggle = entry.GetChild(0).GetChild(0).GetComponent<Toggle>();
            PlayToggle.group = entry.parent.parent.GetChild(1).GetComponent<ToggleGroup>();
            PlayToggle.onValueChanged.AddListener(delegate { ToggleValueChanged(PlayToggle); });

            // Set the edit toggle
            Toggle EditToggle;
            EditToggle = entry.GetChild(1).GetChild(0).GetComponent<Toggle>();
            EditToggle.group = entry.parent.parent.GetChild(2).GetComponent<ToggleGroup>();
            EditToggle.onValueChanged.AddListener(delegate { ToggleValueChanged(EditToggle); });

            // Add it to the default map list.
            defaultMapEntries.Add(entry);
        }


        // Custom Map List
        // Custom Header
        entry = Instantiate(customMapHeader, mapList, false);
        pos = entry.position;
        pos.y -= yOffset;
        entry.position = pos;
        yOffset += entry.rect.height;

        // Custom Map Entries
        for (int i = 0; i < customMapFiles.Length; i++)
        {
            // Skip any Unity meta files.
            if (customMapFiles[i].Name.Contains(".meta"))
                continue;

            // Create a new map entry offset to not overlap with the previous entry.
            entry = Instantiate(mapFileEntry, mapList, false);
            pos = entry.position;
            pos.y -= yOffset;
            entry.position = pos;
            yOffset += entry.rect.height;

            // Set the UI MapName
            entry.GetChild(2).GetChild(0).GetComponent<Text>().text = customMapFiles[i].Name.Substring(0, customMapFiles[i].Name.Length - 4);

            // Set the play toggle
            Toggle PlayToggle;
            PlayToggle = entry.GetChild(0).GetChild(0).GetComponent<Toggle>();
            PlayToggle.group = entry.parent.parent.GetChild(1).GetComponent<ToggleGroup>();
            PlayToggle.onValueChanged.AddListener(delegate { ToggleValueChanged(PlayToggle); });

            // Set the edit toggle
            Toggle EditToggle;
            EditToggle = entry.GetChild(1).GetChild(0).GetComponent<Toggle>();
            EditToggle.group = entry.parent.parent.GetChild(2).GetComponent<ToggleGroup>();
            EditToggle.onValueChanged.AddListener(delegate { ToggleValueChanged(EditToggle); });

            // Add it to the custom map list.
            customMapEntries.Add(entry);
        }

        // Scale the mapList scrolling to only fit the number of map elements.
        mapList.sizeDelta = new Vector2(mapList.sizeDelta.x, yOffset);
    }

    // Override the LevelScript GoToScene function to additionally
    // call the GenerateMapSelectionFile() function.
    new public void GoToScene(string sceneName)
    {
        // Generates the map seleciton file once the user navigates away from the page.
        GenerateMapSelectionFile();
        SceneManager.LoadScene(sceneName);
    }

    // Called when a toggle value is changed.
    private void ToggleValueChanged(Toggle changedToggle)
    {
        // Only Edit toggles can be switched off.
        // Play toggles MUST have a map selected.

        // If a toggle was switched to ON:
        if (changedToggle.isOn)
        {
            // Determine which toggle group it is
            if(changedToggle.group.allowSwitchOff)
            {
                // EditToggle

                // Set the current edit map to the selected map.
                currentEditMap = changedToggle.transform.parent.parent.GetChild(2).GetChild(0).GetComponent<Text>().text;

                // Determine if it's a default or custom map based on position in the list in 
                // relation to the custom map header entry.
                if (changedToggle.transform.parent.parent.position.y < defaultMapEntries[defaultMapEntries.Count - 1].position.y)
                    isDefaultEditMap = false;
                else
                    isDefaultEditMap = true;
            }
            else
            {
                // PlayToggle

                // Set the current play map to the selected map.
                currentPlayMap = changedToggle.transform.parent.parent.GetChild(2).GetChild(0).GetComponent<Text>().text;

                // Determine if it's a default or custom map based on position in the list in 
                // relation to the custom map header entry.
                if (changedToggle.transform.parent.parent.position.y < defaultMapEntries[defaultMapEntries.Count - 1].position.y)
                    isDefaultPlayMap = false;
                else
                    isDefaultPlayMap = true;
            }
        }
        else
        {
            // Determine which toggle group it is
            if (changedToggle.group.allowSwitchOff)
            {
                // EditToggle
                
                // Check if there are any toggles on.
                if(!changedToggle.group.AnyTogglesOn())
                    currentEditMap = "Empty";
            }
        }

        // Update the Map UI with the updated selection.
        UpdateCurrentMapUI();
    }

    // Creates a Selection.txt file containing the selected play/edit maps.
    public void GenerateMapSelectionFile()
    {
        string path = Application.dataPath + "/SavedMaps/";
        string filename = "Selection.txt";
        string fullPath = path + filename;

        // If a Selection.txt file doesn't yet exist, create one.
        // If the file does exist, overwrite it.

        /*
         * File is organized as follows:
         * [0] CurrentPlayMap: [PlayMapName]
         * [1] [Custom/Default]
         * [2] CurrentEditMap: [EditMapName]
         * [3] [Custom/Default]
         */

        // PlayMap
        File.WriteAllText(fullPath, "CurrentPlayMap: " + currentPlayMap + "\n");

        if(isDefaultPlayMap)
            File.AppendAllText(fullPath, "Default\n");
        else
            File.AppendAllText(fullPath, "Custom\n");

        // EditMap
        File.AppendAllText(fullPath, "CurrentEditMap: " + currentEditMap + "\n");

        if (isDefaultEditMap)
            File.AppendAllText(fullPath, "Default\n");
        else
            File.AppendAllText(fullPath, "Custom\n");
    }
}
