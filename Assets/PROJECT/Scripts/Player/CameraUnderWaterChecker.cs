using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraUnderWaterChecker : MonoBehaviour
{
    public FullScreenPassRendererFeature data;
    private void Update()
    {
        data.SetActive(transform.position.y < .4);
    }
}
