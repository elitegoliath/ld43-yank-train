using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroundBotAI : MonoBehaviour {
    public float enagementRange = 10f;
    public GameObject target;
    public CombatController targetCombatController;
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        // TODO: Listen for when the APC has been exitted. Continue a short distance then activate AI Modes.
    }

    private void AISeekTarget()
    {
        // TODO: Move towards player until within enagement range.
    }

    private void AIEngageTarget()
    {

    }
}
