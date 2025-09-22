using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class URScriptLoader : MonoBehaviour
{
    public string filePath = "C:/path/to/your/file.script";
    public List<float[]> jointWaypoints = new();

    public void LoadURScript()
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("URScript file not found at " + filePath);
            return;
        }

        jointWaypoints.Clear();

        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            if (line.Contains("movej("))
            {
                var match = Regex.Match(line, @"movej\(\[(.*?)\]");
                if (match.Success)
                {
                    string[] angles = match.Groups[1].Value.Split(',');
                    float[] jointPos = new float[angles.Length];
                    for (int i = 0; i < angles.Length; i++)
                    {
                        if (float.TryParse(angles[i], out float val))
                            jointPos[i] = val;
                    }
                    jointWaypoints.Add(jointPos);
                }
            }
        }

        Debug.Log("Loaded " + jointWaypoints.Count + " waypoints.");
        URModeManager.Instance.SetMode(URMode.Playback);
    }
}
