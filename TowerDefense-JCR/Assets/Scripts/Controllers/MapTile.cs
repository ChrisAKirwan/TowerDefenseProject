using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapTile : MonoBehaviour
{
    // initial color to return to after mouse-over.
    public Color startColor { get; private set; }

    // Index properties for saving/loading map files.
    [HideInInspector] public int tileIndex;
    [HideInInspector] public int materialIndex;
    [HideInInspector] public int propIndex;
    [HideInInspector] public Prop prop;
    [HideInInspector] public bool hasProp;
    [HideInInspector] public BaseStructureClass structure;
    [HideInInspector] public bool hasStructure;
    [HideInInspector] public bool isFlat;

    [HideInInspector] public List<MapTile> neighbors;
    public float topYPos { get; private set; }

    private float delay;

    private PlayerController PC;

    private void OnMouseEnter()
    {
        if (PC.inEditMode)
        {
            PC.tile_index = this.tileIndex;
            PC.mat_index = this.materialIndex;
            if (this.hasProp)
                PC.prop_index = this.propIndex;
            else
                PC.prop_index = 0;
        }
        else
        { 
            transform.GetComponent<Renderer>().material.color = Color.yellow;
        }
    }

    private void OnMouseExit()
    {
        if (!PC.inEditMode)
        {
            transform.GetComponent<Renderer>().material.color = startColor;
        }
    }

    void Awake()
    {
        neighbors = new List<MapTile>();
        topYPos = this.transform.GetChild(0).transform.position.y;
        PC = FindObjectOfType<PlayerController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        startColor = transform.GetComponent<Renderer>().material.color;
        this.enabled = false;
        hasStructure = false;
        hasProp = false;
    }

    void Update()
    {
        if(Time.realtimeSinceStartup > delay)
        {
            transform.GetComponent<Renderer>().material.color = startColor;
            this.enabled = false;
        }
    }

    public void InvalidPlacement()
    {
        delay = Time.realtimeSinceStartup + 1.0f;
        transform.GetComponent<Renderer>().material.color = Color.red;
        this.enabled = true;
    }
}
