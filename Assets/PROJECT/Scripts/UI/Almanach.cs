using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Almanach : MonoBehaviour
{
    public Image leftPageImage;
    public Image rightPageImage;

    [Header("Animation Pages")]
    public Image flipPageImageR; // Объект справа (Pivot: 0, 0.5)
    public Image flipPageImageL; // Объект слева (Pivot: 1, 0.5)

    public Sprite[] pages;

    private int currentSpread = 0;
    private bool isAnimating = false;
    public float duration = 0.5f;

    void Start()
    {
        flipPageImageR.gameObject.SetActive(false);
        flipPageImageL.gameObject.SetActive(false);
        UpdateSpread();
    }

    public void NextSpread()
    {
        if (isAnimating || currentSpread + 2 >= pages.Length) return;
        isAnimating = true;

        // 1. Подготовка: ставим на правую анимированную страницу текущий правый спрайт
        flipPageImageR.sprite = pages[currentSpread + 1];
        flipPageImageR.rectTransform.localScale = Vector3.one;
        flipPageImageR.gameObject.SetActive(true);

        // Готовим левую анимированную (она пока невидима, ScaleX = 0)
        flipPageImageL.sprite = pages[currentSpread + 2];
        flipPageImageL.rectTransform.localScale = new Vector3(0, 1, 1);
        flipPageImageL.gameObject.SetActive(true);

        // Обновляем "подложку" справа заранее
        if (currentSpread + 3 < pages.Length)
            rightPageImage.sprite = pages[currentSpread + 3];
        else
            rightPageImage.gameObject.SetActive(false);

        // АНИМАЦИЯ:
        Sequence seq = DOTween.Sequence();
        // Схлопываем правую
        seq.Append(flipPageImageR.rectTransform.DOScaleX(0, duration / 2).SetEase(Ease.InQuad));
        // Разворачиваем левую
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
        if (isAnimating || currentSpread - 2 < 0) return;
        isAnimating = true;

        // 1. ПОДГОТОВКА СТРАНИЦ
        // Анимированная ЛЕВАЯ (та, что сейчас видна)
        flipPageImageL.sprite = pages[currentSpread];
        flipPageImageL.rectTransform.localScale = Vector3.one;
        flipPageImageL.gameObject.SetActive(true);

        // Анимированная ПРАВАЯ (та, которая "прилетит")
        // Это будет страница с индексом currentSpread - 1
        flipPageImageR.sprite = pages[currentSpread - 1];
        flipPageImageR.rectTransform.localScale = new Vector3(0, 1, 1); // Скрыта (ребром)
        flipPageImageR.gameObject.SetActive(true);

        // Подложка СЛЕВА: показываем то, что будет под летящей страницей
        if (currentSpread - 2 >= 0)
        {
            leftPageImage.sprite = pages[currentSpread - 2];
            leftPageImage.gameObject.SetActive(true);
        }

        // 2. АНИМАЦИЯ
        Sequence seq = DOTween.Sequence();

        // Схлопываем текущую левую к центру
        seq.Append(flipPageImageL.rectTransform.DOScaleX(0, duration / 2).SetEase(Ease.InQuad));

        // Разворачиваем новую правую из центра
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
