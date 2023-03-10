using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScore : MonoBehaviour
{

    public Text redScoreText;
    public Text blueScoreText;

    private int _blueScore,_redScore;

    // Start is called before the first frame update
    void Start()
    {
        redScoreText.text = "Red:";
        blueScoreText.text = "Blue:";

        _blueScore = 0;
        _redScore = 0;
    }

   public void ScoreUpdateRed(int redScore)
    {
        _redScore += redScore;
        redScoreText.text = "Red: " + _redScore;
    }

    public void ScoreUpdateBlue(int blueScore)
    {
        _blueScore += blueScore;
        blueScoreText.text = "Blue: " + _blueScore;
    }
}
