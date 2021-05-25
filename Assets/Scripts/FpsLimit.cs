using UnityEngine;

public class FpsLimit : MonoBehaviour
{
    [Range(30,120)]
    public int targetFramerate = 60;

    private void Awake() {
        Application.targetFrameRate = targetFramerate;
    }
}
