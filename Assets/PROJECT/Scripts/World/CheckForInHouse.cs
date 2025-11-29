using UnityEngine;

public class CheckForInHouse : MonoBehaviour
{
    public void OnStartGame()
    {

        Debug.LogWarning("Check for in house"+ (GameManager.Instance.GetPlayer().transform.position - transform.position).magnitude);
        if((GameManager.Instance.GetPlayer().transform.position - transform.position).magnitude < 40)
        {
            CameraSwitcher.Instance.Switch();
        }
    }
}
