using UnityEngine;

public enum SkillUpgradeType
{
    None,
    
    Dash,
    Dash_CloneOnStart,
    Dash_CloneOnStartAndArrival,
    Dash_ShardOnStart,
    Dash_ShardOnStartAndArrival,
    
    Shard,
    Shard_MoveToEnemy,
    Shard_MultiCast,
    Shard_Teleport,
    Shard_TeleportHpRewind
}