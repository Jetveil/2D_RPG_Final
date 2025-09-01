using System;
using UnityEngine;

public class Player_AnimationTriggers : MonoBehaviour
{
    public Player player;

    private void Awake()
    {
        player = GetComponentInParent<Player>();

    }

    private void CurrentStateTrigger()
    {
        player.CallAnimationTrigger();
    }
}
