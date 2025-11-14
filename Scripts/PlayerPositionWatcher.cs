using UnityEngine;

public class PlayerPositionWatcher : MonoBehaviour
{
    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
        Debug.LogError($"[WATCHER] 🎮 Player Start Position: {lastPosition}");
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, lastPosition) > 0.1f)
        {
            Debug.LogError($"[WATCHER] ⚠️ Player MOVED! From {lastPosition} → To {transform.position}");
            Debug.LogError($"[WATCHER] StackTrace:\n{System.Environment.StackTrace}");
            lastPosition = transform.position;
        }
    }
}