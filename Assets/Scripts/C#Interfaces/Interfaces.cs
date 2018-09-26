using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKillable
{
    void TakeDamage(float _value);
    void CheckDeath();
    void KillEntity();
    bool IsAlive();
}