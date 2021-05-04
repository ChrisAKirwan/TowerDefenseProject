using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyProduction : BaseStructureClass
{
    [SerializeField] private float currencyBonusPerMin = 100.0f;


    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        // Increase Player's currency by currencyBonusPerMin / 60.0f * Time.DeltaTime (to get value in seconds);
        if (mainLevelRef.gameState == MainLevel.GameState.COMBATPHASE)
        {
            mainLevelRef.AddCurrency((currencyBonusPerMin / 60.0f) * Time.deltaTime);
        }
    }

    public override void FullStatModifier(float multiplier)
    {
        base.FullStatModifier(multiplier);

        currencyBonusPerMin *= multiplier;
    }
}
