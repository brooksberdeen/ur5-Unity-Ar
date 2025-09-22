using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class URPlaybackController : MonoBehaviour
{
    public URScriptLoader scriptLoader;
    public Transform[] jointTransforms; // Assign your robot joint GameObjects
    public float moveDuration = 2.0f;

    private int currentWaypointIndex = 0;
    private float timer = 0f;
    private float[] currentAngles;
    private float[] targetAngles;

    void Start()
    {
        currentAngles = new float[jointTransforms.Length];
        targetAngles = new float[jointTransforms.Length];
    }

    void Update()
    {
        if (URModeManager.Instance.currentMode != URMode.Playback)
            return;

        if (scriptLoader.jointWaypoints.Count == 0)
            return;

        if (currentWaypointIndex >= scriptLoader.jointWaypoints.Count)
            return;

        if (timer == 0)
        {
            currentAngles = GetCurrentJointAngles();
            targetAngles = scriptLoader.jointWaypoints[currentWaypointIndex];
        }

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / moveDuration);
        for (int i = 0; i < jointTransforms.Length; i++)
        {
            float angle = Mathf.Lerp(currentAngles[i], targetAngles[i], t);
            jointTransforms[i].localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg); // Adjust axes as needed
        }

        if (t >= 1f)
        {
            timer = 0f;
            currentWaypointIndex++;
        }
    }

    private float[] GetCurrentJointAngles()
    {
        float[] angles = new float[jointTransforms.Length];
        for (int i = 0; i < jointTransforms.Length; i++)
        {
            angles[i] = jointTransforms[i].localRotation.eulerAngles.z * Mathf.Deg2Rad; // Adjust axes as needed
        }
        return angles;
    }
}
