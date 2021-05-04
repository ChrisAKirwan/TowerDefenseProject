using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour
{
    private PlayerController PC;
    [HideInInspector] public bool isPreviewObject = false;

    private void OnMouseEnter()
    {
        if (!PC.inEditMode && !isPreviewObject)
            transform.parent.GetComponent<Renderer>().material.color = Color.yellow;
    }

    private void OnMouseExit()
    {
        if (!PC.inEditMode && !isPreviewObject)
            transform.parent.GetComponent<Renderer>().material.color = transform.parent.GetComponent<MapTile>().startColor;
    }

    // Start is called before the first frame update
    void Start()
    {
        PC = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
