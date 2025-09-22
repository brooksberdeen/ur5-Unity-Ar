using System.Collections;
using System.Linq;
using UnityEngine;

public class UR5eSnapToAnchorBB : MonoBehaviour
{
    [Header("Assign your UR5e twin root (base_link)")]
    public Transform ur5Root;

    [Header("Optional: lock to a specific anchor UUID")]
    public string anchorUuid = "";   // leave empty to use the first found

    [Header("Parent or set world pose?")]
    public bool parentToAnchor = false;

    [Header("Use offset from anchor → robot base?")]
    public bool useOffset = false;
    public Vector3 anchorToRobotPos;
    public Quaternion anchorToRobotRot = Quaternion.identity;

    private OVRSpatialAnchor anchor; // <- no Meta.XR namespace

    private void Start()
    {
        StartCoroutine(FindAndSnap());
    }

    private IEnumerator FindAndSnap()
    {
        // 1) Find the OVRSpatialAnchor spawned by the Building Block
        float timeout = 8f, t = 0f;
        while (anchor == null && t < timeout)
        {
            var anchors = FindObjectsOfType<OVRSpatialAnchor>();
            anchor = string.IsNullOrEmpty(anchorUuid)
                ? anchors.FirstOrDefault()
                : anchors.FirstOrDefault(a => a.Uuid.ToString().Equals(anchorUuid, System.StringComparison.OrdinalIgnoreCase));

            if (anchor != null) break;
            t += Time.deltaTime; yield return null;
        }
        if (anchor == null) { Debug.LogWarning("No OVRSpatialAnchor found."); yield break; }

        // 2) Wait until the anchor is localized before using its pose
        // (Meta’s guide: load → localize → bind → use the pose)
        while (!anchor.Localized) yield return null;  // poll until localized

        // 3) Snap UR5e to the anchor
        if (parentToAnchor)
        {
            ur5Root.SetParent(anchor.transform, false);
            ur5Root.localPosition = useOffset ? anchorToRobotPos : Vector3.zero;
            ur5Root.localRotation = useOffset ? anchorToRobotRot : Quaternion.identity;
        }
        else
        {
            if (!useOffset)
            {
                ur5Root.SetPositionAndRotation(anchor.transform.position, anchor.transform.rotation);
            }
            else
            {
                var TwA = Matrix4x4.TRS(anchor.transform.position, anchor.transform.rotation, Vector3.one);
                var TaR = Matrix4x4.TRS(anchorToRobotPos, anchorToRobotRot, Vector3.one);
                var TwR = TwA * TaR;

                var pos = TwR.GetColumn(3);
                var rot = Quaternion.LookRotation(TwR.GetColumn(2), TwR.GetColumn(1));
                ur5Root.SetPositionAndRotation(pos, rot);
            }
        }

        Debug.Log("[UR5eSnapToAnchorBB] Snapped UR5e to anchor.");
    }

    [ContextMenu("Capture Offset From Current Pose")]
    public void CaptureOffsetFromCurrentPose()
    {
        if (anchor == null || ur5Root == null) return;

        var TwA = Matrix4x4.TRS(anchor.transform.position, anchor.transform.rotation, Vector3.one);
        var TwR = Matrix4x4.TRS(ur5Root.position, ur5Root.rotation, Vector3.one);
        var TaR = TwA.inverse * TwR;

        anchorToRobotPos = TaR.GetColumn(3);
        anchorToRobotRot = Quaternion.LookRotation(TaR.GetColumn(2), TaR.GetColumn(1));
        useOffset = true;

        Debug.Log("[UR5eSnapToAnchorBB] Captured T_anchor_robotBase.");
    }
}
