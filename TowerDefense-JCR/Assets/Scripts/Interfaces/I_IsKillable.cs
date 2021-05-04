using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_IsKillable
{
    void OnDeath();
    void TakeDamage(float damage);
    void Heal(float amount);
}
