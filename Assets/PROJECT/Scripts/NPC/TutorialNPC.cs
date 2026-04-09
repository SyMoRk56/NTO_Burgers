using System.Collections;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(DialogueRunner))]
public class TutorialNPC : MonoBehaviour
{
    Transform player;

    private void Start()
    {
        player = GameManager.Instance.GetPlayer().transform;
    }
    private void Update()
    {
        if (Vector3.Distance(transform.position, player.position) < 5)
        {
            Vector3 direction = player.position - transform.position;

            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5 * Time.deltaTime);
            }
        }
    }
    public void OnPlayerExitedHouse()
    {
        StartCoroutine(OnPlayerExitedHouseDelayed());
    }
    IEnumerator OnPlayerExitedHouseDelayed()
    {
        yield return new WaitForSeconds(.5f);
        GetComponent<DialogueRunner>().StartDialogue(true);
    }
}