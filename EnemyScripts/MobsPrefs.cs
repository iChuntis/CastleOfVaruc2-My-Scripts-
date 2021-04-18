using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Mobs Prefabs saver", menuName = "Create Mobs Prefabs saver")]
public class MobsPrefs : ScriptableObject
{
    [SerializeField]
    public GameObject
        slashParticle,
        bloodSmash,
        floatingPoints,
        deathChunkParticle,
        deathBloodParticle,
        coinsDrop;


}
