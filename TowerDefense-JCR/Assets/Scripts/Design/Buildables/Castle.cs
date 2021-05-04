using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Castle : OffensiveStructure
{
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

    public override void OnDeath()
    {
        base.OnDeath();
        FindObjectOfType<LevelScript>().GetComponent<MainLevel>().GameOver(false);
    }
}
