using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreOnDestroy : MonoBehaviour
{
    [Tooltip("The amount of score gained when destroyed.")]
    public int scoreAmount = 1;

    /// <summary>
    /// Description:
    /// When gameObject destroyed, add score to the player via the game manager
    /// Inputs: N/A
    /// Outputs: N/A
    /// </summary>
    private void OnDestroy()
    {
        if (GameManager.instance != null)
        {
            GameManager.AddScore(scoreAmount);
        }
    }

}
