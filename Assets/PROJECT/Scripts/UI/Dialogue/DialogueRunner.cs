using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogueRunner : MonoBehaviour
{
    [Header("Настройки диалога")]
    public string ownerName;
    public DialogueScriptableObject[] defaultDialogues;
    public DialogueScriptableObject[] letterDialogues;
    public bool random;

    // Публичные свойства для доступа из других скриптов
    public int CurrentDialogueIndex => currentDialogueIndex;
    public bool CurrentIsLetter => isLetter;

    [Header("UI и эмоции")]
    public DialogueUI dialogueUI;
    public Face face;

    [Header("Аудио")]
    public AudioSource audioSource;

    // Приватные поля
    private int currentDialogueIndex;
    private int currentPhraseIndex;
    private bool isLetter;
    private bool isRunning;
    private bool isChoosing = false;
    public AudioClip clip;
    public bool IsDialogueActive => isRunning;

    // Ссылки на сцены
    private treecastscene treeScene;
    private skamia benchScene;
    private FlowerTriggerHandler flowerHandler;

    // Для запуска дерева после фразы про яблоню
    private bool wasApplePhraseSpoken = false;

    void Start()
    {
        // Инициализация benchScene
        benchScene = GetComponent<skamia>();
        if (benchScene == null)
        {
            benchScene = GetComponentInChildren<skamia>();
        }

        // Инициализация flowerHandler
        flowerHandler = GetComponent<FlowerTriggerHandler>();
        if (flowerHandler == null)
        {
            flowerHandler = GetComponentInChildren<FlowerTriggerHandler>();
        }

        dialogueUI = GetComponentInChildren<DialogueUI>(true);
        dialogueUI.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => NextPhrase());
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Находим treecastscene на этом же объекте
        treeScene = GetComponent<treecastscene>();
        if (treeScene != null)
        {
            Debug.Log("Найден treecastscene");
        }
        else
        {
            treeScene = GetComponentInChildren<treecastscene>();
            if (treeScene != null)
            {
                Debug.Log("Найден treecastscene в дочерних объектах");
            }
            else
            {
                Debug.LogWarning("treecastscene не найден");
            }
        }
    }

    void Update()
    {
        if (!isRunning) return;

        var block = isLetter ? letterDialogues[currentDialogueIndex] : defaultDialogues[currentDialogueIndex];
        bool isAtChoicePoint = currentPhraseIndex >= block.phrases.Length;

        if (!isChoosing && !isAtChoicePoint &&
           (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.E) ||
            Input.GetMouseButtonDown(0)))
        {
            NextPhrase();
        }
    }

    public void StartDialogue(bool letter)
    {
        if (isRunning)
            ForceCloseDialogue();

        if (!isRunning)
        {
            // Сбрасываем все анимации при начале нового диалога
            StopAllAnimations(true); // true = полный сброс

            // Останавливаем NPC
            var npc = GetComponent<NPCBehaviour>();
            if (npc != null)
            {
                npc.dialogueActive = true;
                npc.Stop();
            }

            isLetter = letter;
            currentDialogueIndex = 0;
            currentPhraseIndex = 0;
            isChoosing = false;
            wasApplePhraseSpoken = false; // Сбрасываем флаг

            var dialogues = letter ? letterDialogues : defaultDialogues;
            if (dialogues.Length == 0)
            {
                Debug.LogError($"No dialogues found for type: letter={letter}");
                return;
            }

            dialogueUI.gameObject.SetActive(true);
            dialogueUI.nameText.text = LocalizationManager.Instance.Get(ownerName);

            ShowCurrentPhrase();
            isRunning = true;

            var player = GameManager.Instance.GetPlayer();
            if (player != null)
            {
                var playerManager = player.GetComponent<PlayerManager>();
                if (playerManager != null)
                {
                    playerManager.ShowCursor(true);
                    playerManager.CanMove = false;
                }
            }
        }
    }

    void ShowCurrentPhrase()
    {
        isChoosing = false;

        var block = isLetter ? letterDialogues[currentDialogueIndex] : defaultDialogues[currentDialogueIndex];

        if (currentPhraseIndex < block.phrases.Length)
        {
            string phrase = block.phrases[currentPhraseIndex];
            dialogueUI.ShowPhrase(ownerName, phrase);

            //AudioClip clip = block.voiceOver[currentPhraseIndex];
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }

            if (face != null)
            {
                if (block.emotions.Count != 0 && currentPhraseIndex < block.emotions.Count)
                {
                    face.SetFace(block.emotions[currentPhraseIndex]);
                }
            }

            // Проверяем фразу для запуска дерева
            CheckForTreeSequence(phrase);
        }
        else
        {
            isChoosing = true;
            dialogueUI.ShowChoices(block.choices);
        }
    }

    // Метод для проверки триггерных фраз
    void CheckForTreeSequence(string phrase)
    {
        phrase = LocalizationManager.Instance.Get(phrase);
        print("Check for phrase " + phrase);

        // Проверяем фразу про яблоню
        if (phrase.Contains("Яблоня") || phrase.Contains("яблоня") || phrase.Contains("Apple tree"))
        {
            Debug.Log($"DialogueRunner: Найдена фраза про яблоню: '{phrase}'");
            wasApplePhraseSpoken = true;

            if (treeScene != null)
            {
                StartCoroutine(DelayedTreeSequence());
            }
        }

        // Проверяем фразу про скамейку
        if (phrase.Contains("Скамейка") || phrase.Contains("скамейка") || phrase.Contains("bench"))
        {
            Debug.Log($"DialogueRunner: Найдена фраза про скамейку: '{phrase}'");

            if (benchScene != null)
            {
                benchScene.TriggerCameraSequence();
            }
            else
            {
                Debug.LogWarning("DialogueRunner: benchScene не найден!");
            }
        }
    }

    IEnumerator DelayedTreeSequence()
    {
        // Ждем 2 секунды, чтобы фраза полностью отобразилась
        yield return new WaitForSeconds(2f);

        if (treeScene != null && wasApplePhraseSpoken)
        {
            Debug.Log("DialogueRunner: Запускаем кинопоследовательность с деревом...");
            treeScene.TriggerCameraSequence();
        }
    }

    public void NextPhrase()
    {
        var block = isLetter ? letterDialogues[currentDialogueIndex] : defaultDialogues[currentDialogueIndex];

        currentPhraseIndex++;

        if (currentPhraseIndex < block.phrases.Length)
            ShowCurrentPhrase();
        else
        {
            isChoosing = true;
            dialogueUI.ShowChoices(block.choices);
        }
    }

    public void Choose(int index)
    {
        isChoosing = false;

        var block = isLetter ? letterDialogues[currentDialogueIndex] : defaultDialogues[currentDialogueIndex];

        if (index < 0 || index >= block.choices.Length) return;

        int next = block.choices[index].nextDialogueIndex;

        if (next < 0)
        {
            EndDialogue();
            return;
        }

        if (next >= (isLetter ? letterDialogues.Length : defaultDialogues.Length))
            return;

        currentDialogueIndex = next;
        currentPhraseIndex = 0;
        ShowCurrentPhrase();
    }

    public void EndDialogue()
    {
        dialogueUI.Hide();
        isRunning = false;
        isLetter = false;
        wasApplePhraseSpoken = false;

        // Только останавливаем анимации, но НЕ сбрасываем объекты
        StopAllAnimations(false); // false = только остановка, без сброса

        var npc = GetComponent<NPCBehaviour>();
        if (npc != null)
        {
            npc.dialogueActive = false;
            npc.Resume();
        }

        // Разблокируем игрока
        var player = GameManager.Instance.GetPlayer();
        if (player != null)
        {
            var playerManager = player.GetComponent<PlayerManager>();
            if (playerManager != null)
            {
                playerManager.ShowCursor(false);
                playerManager.CanMove = true;
            }
        }
    }

    public void ForceCloseDialogue()
    {
        if (!isRunning) return;

        isRunning = false;
        isLetter = false;
        isChoosing = false;
        currentDialogueIndex = 0;
        currentPhraseIndex = 0;
        wasApplePhraseSpoken = false;

        // Принудительное закрытие - сбрасываем всё
        StopAllAnimations(true); // true = полный сброс

        if (dialogueUI != null)
            dialogueUI.ForceHide();

        var npc = GetComponent<NPCBehaviour>();
        if (npc != null)
        {
            npc.dialogueActive = false;
            npc.Resume();
        }

        // Разблокируем игрока
        var player = GameManager.Instance.GetPlayer();
        if (player != null)
        {
            var playerManager = player.GetComponent<PlayerManager>();
            if (playerManager != null)
            {
                playerManager.ShowCursor(false);
                playerManager.CanMove = true;
            }
        }
    }

    // Исправленный метод для остановки всех анимаций
    private void StopAllAnimations(bool fullReset)
    {
        // Останавливаем treecastscene
        if (treeScene != null)
        {
            treeScene.StopAllCoroutines();
            if (fullReset)
                treeScene.ResetScene();
            else
                treeScene.StopScene();
        }

        // Останавливаем skamia
        if (benchScene != null)
        {
            benchScene.StopAllCoroutines();
            if (fullReset)
                benchScene.ResetScene();
            else
                benchScene.StopScene();
        }

        // Останавливаем flowerHandler
        if (flowerHandler != null)
        {
            flowerHandler.StopAllCoroutines();
            if (fullReset)
            {
                // Используем рефлексию для доступа к приватным полям
                var flowerField = flowerHandler.GetType().GetField("currentFlower",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (flowerField != null)
                {
                    GameObject currentFlower = (GameObject)flowerField.GetValue(flowerHandler);
                    if (currentFlower != null)
                        Destroy(currentFlower);
                }

                // Останавливаем VFX
                var sneezeVFXField = flowerHandler.GetType().GetField("sneezeVFX",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (sneezeVFXField != null)
                {
                    ParticleSystem sneezeVFX = (ParticleSystem)sneezeVFXField.GetValue(flowerHandler);
                    if (sneezeVFX != null && sneezeVFX.isPlaying)
                    {
                        sneezeVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                }
            }
        }
    }
}