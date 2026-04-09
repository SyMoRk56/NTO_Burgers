using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;

public class Manual : MonoBehaviour
{
    public Image leftPageImage;
    public Image rightPageImage;

    [Header("Animation Pages")]
    public Image flipPageImageR;
    public Image flipPageImageL;

    public Sprite[] pages1;
    public Sprite[] pages2;
    public Sprite[] pages3;

    [Tooltip("0 = pages1, 1 = pages2, 2 = pages3")]
    public int pageIndex; // Индекс выбранной категории

    private int currentSpread = 0;
    private bool isAnimating = false;
    public float duration = 0.5f;
    public AudioSource source;

    // Вспомогательное свойство для получения текущего активного массива
    private Sprite[] CurrentPageSet
    {
        get
        {
            if (pageIndex == 1) return pages2;
            if (pageIndex == 2) return pages3;
            return pages1; // По умолчанию первая категория
        }
    }
    private void OnEnable()
    {
        int mailcount = 0;
        foreach (var m in PlayerMailInventory.Instance.carriedMails)
        {
            if (!m.adress.Contains("Tutorial"))
            {
                mailcount += 1;
            }
        }
        if(mailcount > 0)
        {
            pageIndex = 1;
        }
        if (PlayerManager.instance.Day > 0) pageIndex = 2;
        leftPageImage.GetComponent<Button>().onClick.RemoveAllListeners();
        leftPageImage.GetComponent<Button>().onClick.AddListener(() => NextSpread());
        rightPageImage.GetComponent<Button>().onClick.RemoveAllListeners();
        rightPageImage.GetComponent<Button>().onClick.AddListener(() => PreviousSpread());
    }
    void Start()
    {
        flipPageImageR.gameObject.SetActive(false);
        flipPageImageL.gameObject.SetActive(false);
        UpdateSpread();
    }

    // Метод для смены категории (вызывать извне, если нужно сменить тип страниц)
    public void ChangeCategory(int index)
    {
        if (isAnimating) return;
        pageIndex = index;
        currentSpread = 0;
        UpdateSpread();
    }

    public void NextSpread()
    {
        var pages = CurrentPageSet; // Берем нужный массив
        if (isAnimating || currentSpread + 2 >= pages.Length) return;
        isAnimating = true;
        source.Play();

        flipPageImageR.sprite = pages[currentSpread + 1];
        flipPageImageR.rectTransform.localScale = Vector3.one;
        flipPageImageR.gameObject.SetActive(true);

        flipPageImageL.sprite = pages[currentSpread + 2];
        flipPageImageL.rectTransform.localScale = new Vector3(0, 1, 1);
        flipPageImageL.gameObject.SetActive(true);

        if (currentSpread + 3 < pages.Length)
            rightPageImage.sprite = pages[currentSpread + 3];
        else
            rightPageImage.gameObject.SetActive(false);

        Sequence seq = DOTween.Sequence();
        seq.Append(flipPageImageR.rectTransform.DOScaleX(0, duration / 2).SetEase(Ease.InQuad));
        seq.Append(flipPageImageL.rectTransform.DOScaleX(1, duration / 2).SetEase(Ease.OutQuad));

        seq.OnComplete(() => {
            currentSpread += 2;
            UpdateSpread();
            flipPageImageR.gameObject.SetActive(false);
            flipPageImageL.gameObject.SetActive(false);
            isAnimating = false;
        });
    }

    public void PreviousSpread()
    {
        var pages = CurrentPageSet; // Берем нужный массив
        if (isAnimating || currentSpread - 2 < 0) return;
        isAnimating = true;

        flipPageImageL.sprite = pages[currentSpread];
        flipPageImageL.rectTransform.localScale = Vector3.one;
        flipPageImageL.gameObject.SetActive(true);

        flipPageImageR.sprite = pages[currentSpread - 1];
        flipPageImageR.rectTransform.localScale = new Vector3(0, 1, 1);
        flipPageImageR.gameObject.SetActive(true);

        if (currentSpread - 2 >= 0)
        {
            leftPageImage.sprite = pages[currentSpread - 2];
            leftPageImage.gameObject.SetActive(true);
        }
        source.Play();

        Sequence seq = DOTween.Sequence();
        seq.Append(flipPageImageL.rectTransform.DOScaleX(0, duration / 2).SetEase(Ease.InQuad));
        seq.Append(flipPageImageR.rectTransform.DOScaleX(1, duration / 2).SetEase(Ease.OutQuad));

        seq.OnComplete(() => {
            currentSpread -= 2;
            UpdateSpread();
            flipPageImageR.gameObject.SetActive(false);
            flipPageImageL.gameObject.SetActive(false);
            isAnimating = false;
        });
    }

    void UpdateSpread()
    {
        var pages = CurrentPageSet;
        if (pages.Length == 0) return;

        leftPageImage.sprite = pages[currentSpread];
        if (currentSpread + 1 < pages.Length)
        {
            rightPageImage.gameObject.SetActive(true);
            rightPageImage.sprite = pages[currentSpread + 1];
        }
        else
        {
            rightPageImage.gameObject.SetActive(false);
        }
    }
}
