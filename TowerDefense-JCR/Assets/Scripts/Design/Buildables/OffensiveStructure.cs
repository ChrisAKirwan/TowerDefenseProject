using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffensiveStructure : BaseStructureClass
{
    [SerializeField] protected float damage = 50.0f;
    [SerializeField] protected float range = 2;
    [SerializeField] protected float attackSpeed = 1.0f;
    protected SphereCollider rangeCollider;

    protected BaseUnitType currentTarget;
    protected List<BaseUnitType> nearbyEnemiesQueue;
    protected float attackTimer;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        nearbyEnemiesQueue = new List<BaseUnitType>();
        rangeCollider = this.GetComponent<SphereCollider>();
        rangeCollider.isTrigger = true;
        rangeCollider.radius = range;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if (currentTarget != null && currentTarget.isActiveAndEnabled)
        {
            if (attackTimer <= 0)
            {
                attackTimer = 1.0f / attackSpeed;
                mainLevelRef.audioRef.PlaySFX("BuildingAttack");
                currentTarget.TakeDamage(damage);
            }
            else
                attackTimer -= Time.deltaTime;
        }
        else
        {
            if ((currentTarget == null || !currentTarget.isActiveAndEnabled) && nearbyEnemiesQueue.Count > 0)
            {
                currentTarget = nearbyEnemiesQueue[0];
                nearbyEnemiesQueue.RemoveAt(0);
            }
        }
    }


    public void OnTriggerEnter(Collider col)
    {
        BaseUnitType enemy = col.gameObject.GetComponent<BaseUnitType>();
        if (enemy != null)
        {
            if (col is BoxCollider)
            {
                if (nearbyEnemiesQueue == null)
                    Debug.Log("queue is null");
                // Enemy is within Range.
                nearbyEnemiesQueue.Add(enemy);
                if (nearbyEnemiesQueue.Count == 1)
                    currentTarget = enemy;
            }
        }
    }

    public void OnTriggerExit(Collider col)
    {
        BaseUnitType enemy = col.gameObject.GetComponent<BaseUnitType>();
        if (enemy != null)
        {
            if (col is BoxCollider)
            {
                // Enemy is within Range.
                nearbyEnemiesQueue.Remove(enemy);

                if (enemy == currentTarget)
                {
                    if (nearbyEnemiesQueue.Count > 0)
                        currentTarget = nearbyEnemiesQueue[0];
                    else
                        currentTarget = null;
                }
            }
        }
    }

    public override void FullStatModifier(float multiplier)
    {
        base.FullStatModifier(multiplier);
        attackSpeed *= multiplier;
        damage *= multiplier;
        range *= multiplier;
    }
}
