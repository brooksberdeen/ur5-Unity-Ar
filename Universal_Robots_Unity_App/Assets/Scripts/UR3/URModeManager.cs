using UnityEngine;

public enum URMode { Live, Playback }

public class URModeManager : MonoBehaviour
{
    public URMode currentMode = URMode.Live;

    private static URModeManager _instance;
    public static URModeManager Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void SetMode(URMode mode)
    {
        currentMode = mode;
        Debug.Log("Switched mode to: " + currentMode);
    }
}
