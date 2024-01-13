using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using System.Runtime.InteropServices;

public class ChangeInteractions : MonoBehaviour, IDataPersistence
{

    [Header ("Red Row")]
    public TMP_InputField redRed;
    public TMP_InputField redYellow;
    public TMP_InputField redBlue;
    public TMP_InputField redOrange;
    public TMP_InputField redPurple;
    public TMP_InputField redGreen;

    [Header ("Yellow Row")]
    public TMP_InputField yellowRed;
    public TMP_InputField yellowYellow;
    public TMP_InputField yellowBlue;
    public TMP_InputField yellowOrange;
    public TMP_InputField yellowPurple;
    public TMP_InputField yellowGreen;

    [Header ("Blue Row")]
    public TMP_InputField blueRed;
    public TMP_InputField blueYellow;
    public TMP_InputField blueBlue;
    public TMP_InputField blueOrange;
    public TMP_InputField bluePurple;
    public TMP_InputField blueGreen;

    [Header ("Orange Row")]
    public TMP_InputField orangeRed;
    public TMP_InputField orangeYellow;
    public TMP_InputField orangeBlue;
    public TMP_InputField orangeOrange;
    public TMP_InputField orangePurple;
    public TMP_InputField orangeGreen;

    [Header ("Purple Row")]
    public TMP_InputField purpleRed;
    public TMP_InputField purpleYellow;
    public TMP_InputField purpleBlue;
    public TMP_InputField purpleOrange;
    public TMP_InputField purplePurple;
    public TMP_InputField purpleGreen;

    [Header ("Green Row")]
    public TMP_InputField greenRed;
    public TMP_InputField greenYellow;
    public TMP_InputField greenBlue;
    public TMP_InputField greenOrange;
    public TMP_InputField greenPurple;
    public TMP_InputField greenGreen;

    public List<TMP_InputField> interactions;
    public float[] interactionMatrix;
    public float[] archiveInteractionMatrix;

    private void Start() {
        interactions.Add(redRed); // RED
        interactions.Add(redYellow);
        interactions.Add(redBlue);
        interactions.Add(redOrange);
        interactions.Add(redPurple);
        interactions.Add(redGreen);

        interactions.Add(yellowRed); // YELLOW
        interactions.Add(yellowYellow);
        interactions.Add(yellowBlue);
        interactions.Add(yellowOrange);
        interactions.Add(yellowPurple);
        interactions.Add(yellowGreen);

        interactions.Add(blueRed); // BLUE
        interactions.Add(blueYellow);
        interactions.Add(blueBlue);
        interactions.Add(blueOrange);
        interactions.Add(bluePurple);
        interactions.Add(blueGreen);

        interactions.Add(orangeRed); // ORANGE
        interactions.Add(orangeYellow);
        interactions.Add(orangeBlue);
        interactions.Add(orangeOrange);
        interactions.Add(orangePurple);
        interactions.Add(orangeGreen);

        interactions.Add(purpleRed); // PURPLE
        interactions.Add(purpleYellow);
        interactions.Add(purpleBlue);
        interactions.Add(purpleOrange);
        interactions.Add(purplePurple);
        interactions.Add(purpleGreen);

        interactions.Add(greenRed); // GREEN
        interactions.Add(greenYellow);
        interactions.Add(greenBlue);
        interactions.Add(greenOrange);
        interactions.Add(greenPurple);
        interactions.Add(greenGreen);

        SetInputFieldDefaults();
    }
    public void SetInputFieldDefaults() {
        SetInputText();
    }
    public void ValidateInput(TMP_InputField interaction) {
        var match = Regex.Match(interaction.text, "[0-9]+\\.\\d+(?![^0-9])");
        var match2 = Regex.Match(interaction.text, "-[0-9]+\\.\\d+(?![^0-9])");
        var match3 = Regex.Match(interaction.text, "\\.\\d+(?![^0-9])");
        var match4 = Regex.Match(interaction.text, "-\\.\\d+(?![^0-9])");
        var match5 = Regex.Match(interaction.text, "[0-9]+");
        var match6 = Regex.Match(interaction.text, "-[0-9]+");

        List<Match> matches = new List<Match>();
        matches.AddRange(new List<Match> {
            match, match2, match3, match4, match5, match6
        });
        
        for (int i = 0; i < matches.Count; i++)
        {
            if (!matches[0].Success)
            {
                if (!matches[1].Success)
                {
                    if (!matches[2].Success)
                    {
                        if (!matches[3].Success)
                        {
                            if (!matches[4].Success)
                            {
                                if (!matches[5].Success)
                                {
                                    interaction.text = "0";
                                }
                            }
                        }
                    }
                }
            }
        }
        float convertedText = float.Parse(interaction.text);
        if (convertedText < -1) {
            interaction.text = "-1";
        }
        else if (convertedText > 1) {
            interaction.text = "1";
        }

    }
    public void ValidateAll() {
        for (int i = 0; i < interactions.Count; i++) {
            ValidateInput(interactions[i]);
            UpdateMatrix();
            
        }
    }
    public void UpdateMatrix() {
        var j = 0;
        for (int i = 0; i < interactionMatrix.Length; i++)
        {
            interactionMatrix[i] = float.Parse(interactions[j].text);
            j++;
        }
    }
    public void SetInputText() {
        var j = 0;
        for (int i = 0; i < interactions.Count; i++)
        {
            interactions[i].text = Math.Round(interactionMatrix[j], 4).ToString();
            interactions[i].textComponent.overflowMode = TextOverflowModes.Truncate;
            j++;
        }
    }

    public void LoadData(GameData data) {
        interactionMatrix = new float[36];
        interactionMatrix = data.interactionMatrix;
        archiveInteractionMatrix = data.interactionMatrix;
    }

    public void SaveData(GameData data) {
        data.interactionMatrix = interactionMatrix;
    }

}
