using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.TileMap;

public class Soldier : BaseUnitType
{
    protected override void Awake()
    {
        base.Awake();

        isVehicle = false;
        InitDefaults();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void InitDefaults()
    {
        if (maxHealth == 0.0f)
            maxHealth = 100.0f;
        if (armor == 0.0f)
            armor = 20.0f;
        if (range == 0.0f)
            range = 1.0f;
        if (attackSpeed == 0.0f)
            attackSpeed = 1.0f;
        if (damage == 0.0f)
            damage = 50.0f;
        if (unitValue == 0.0f)
            unitValue = 100.0f;
    }

    public override void FullStatModifier(float multiplier)
    {
        maxHealth *= multiplier;
        armor *= multiplier;
        attackSpeed *= multiplier;
        damage *= multiplier;
        unitValue *= multiplier;
    }
}
