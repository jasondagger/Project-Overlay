
using UnityEngine;

public sealed class Main :
    MonoBehaviour
{
    private async void Start()
    {
        await Services.Start();
    }

    private async void OnApplicationQuit()
    {
        await Services.Stop();
    }
}