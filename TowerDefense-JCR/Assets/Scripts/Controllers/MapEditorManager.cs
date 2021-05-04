using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Core.TileMap;


public class MapEditorManager : MonoBehaviour
{
    [Header("GameObject Palettes for the Map Editor")]
    public List<MapTile> tilePalette;
    public List<Material> materialPalette;
    public List<Prop> propPalette;

    private TileMap tileMapRef;

    // Start is called before the first frame update
    void Start()
    {
        enabled = false;
        tileMapRef = FindObjectOfType<LevelScript>().tileMapRef;
    }


    /// <summary>
    /// Reads in a file saved by the "SaveMapToFile()" function
    /// and loads it into a List<List<int>> to be read in by the
    /// "GenerateMapFromList()" function.
    /// </summary>
    /// <returns></returns>
    public List<List<int>> LoadMapFromFile(string path)
    {
        if (!File.Exists(path))
        {
            Debug.Log("No file found. [LoadMapFromFile()]" + path);
            return null;
        }

        List<string> fileLines = File.ReadAllLines(path).ToList();
        string line = fileLines[2];

        List<string> tileElements = line.Split(' ').ToList();
        List<int> mapSize = new List<int>(2);
        mapSize.Add(System.Convert.ToInt32(tileElements[0]));
        mapSize.Add(System.Convert.ToInt32(tileElements[1]));

        List<List<int>> savedMap = new List<List<int>>(mapSize[0] * mapSize[1]);
        savedMap.Add(mapSize);

        for (int i = 3; i < fileLines.Count; i++)
        {
            line = fileLines[i];
            tileElements = line.Split(' ').ToList();

            List<int> tile = new List<int>(4);
            foreach (string s in tileElements)
                tile.Add(System.Convert.ToInt32(s));

            savedMap.Add(tile);
        }

        return savedMap;
    }

    public List<List<int>> LoadEmptyMapWithSize(int rows, int cols)
    {
        List<int> mapSize = new List<int>(2);
        mapSize.Add(rows);
        mapSize.Add(cols);

        List<List<int>> savedMap = new List<List<int>>(rows * cols);
        savedMap.Add(mapSize);

        for (int i = 0; i < rows * cols; i++)
        {
            List<int> tile = new List<int>(4);
            // tileIndex
            tile.Add(0);
            // rotational ID
            tile.Add(0);
            // materialIndex
            tile.Add(0);
            // propIndex
            tile.Add(-1);

            savedMap.Add(tile);
        }

        return savedMap;
    }

    public void SaveMapToFile(string filename)
    {
        string path = Application.dataPath + "/SavedMaps/CustomMaps/" + filename + ".txt";

        File.WriteAllText(path, filename + ":\n");
        File.AppendAllText(path, "Last Updated: " + System.DateTime.Now + "\n");

        // Add rows + cols to file.
        File.AppendAllText(path, tileMapRef.size_x.ToString() + " " + tileMapRef.size_z.ToString() + "\n");

        // For each item in TileList[]:
        for (int i = 0; i < tileMapRef.size_x * tileMapRef.size_z; i++)
        {
            MapTile tilePrefab = tileMapRef.TileList[i].tileObject;

            // Get the prefab index
            File.AppendAllText(path, tilePrefab.tileIndex.ToString() + " ");

            // Get the rotational ID
            File.AppendAllText(path, (tilePrefab.transform.eulerAngles.y / 90.0f).ToString() + " ");

            // Get the Material index
            File.AppendAllText(path, tilePrefab.materialIndex.ToString() + " ");

            // Find the prop index
            if (tileMapRef.TileList[i].OnTopOfTile.Count == 0)
            {
                File.AppendAllText(path, "-1\n");
                continue;
            }
            else
            {
                File.AppendAllText(path, tilePrefab.propIndex.ToString() + "\n");
            }
        }
    }
}
