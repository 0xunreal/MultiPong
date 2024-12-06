using UnityEngine;

public class ServerOnly : MonoBehaviour
{
    void Awake()
    {
        var networkMode = FindFirstObjectByType<NetworkMode>();
        if (networkMode.IsClient)
        {
            Destroy(gameObject);
        }
    }
}
