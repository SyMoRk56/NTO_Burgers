using UnityEngine;
using DG.Tweening;

public class FishWater : MonoBehaviour
{
    public GameObject fishPrefab;
    public float jumpHeight = 2f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject != gameObject) return;
                GameObject fish = Instantiate(fishPrefab, hit.point, Quaternion.identity);
                fish.transform.DOJump(hit.point + new Vector3(Random.Range(-1,1), -1, Random.Range(-1,1)), jumpHeight, 1, .7f)
                    .OnComplete(()=> fish.transform.DOScale(0, 1).SetEase(Ease.OutQuad).OnComplete(() => Destroy(fish)));
            }
        }
    }
}