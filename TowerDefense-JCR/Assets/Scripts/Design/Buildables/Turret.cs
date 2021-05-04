using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : OffensiveStructure
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

        if (currentTarget != null)
            this.transform.GetChild(1).transform.LookAt(currentTarget.transform);
    }
}
