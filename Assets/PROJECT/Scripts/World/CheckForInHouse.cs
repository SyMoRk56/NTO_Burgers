using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CheckForInHouse : MonoBehaviour
{
    public FullScreenPassRendererFeature data;
    int counter = 0;
    public void OnStartGame()
    {

        Debug.LogWarning("Check for in house"+ (GameManager.Instance.GetPlayer().transform.position - transform.position).magnitude);
        if((GameManager.Instance.GetPlayer().transform.position - transform.position).magnitude < 40)
        {
            CameraSwitcher.Instance.Switch();
        }
    }
    private void FixedUpdate()
    {
        counter += 1;
        if(counter > 4)
        {
            counter = 0;
            data.SetActive((GameManager.Instance.GetPlayer().transform.position - transform.position).magnitude > 40);
        }
    }
}
