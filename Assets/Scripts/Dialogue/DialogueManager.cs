using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private PauseController pauseController;

    public event Action<DialogueLine> OnLineChanged;
    public event Action<string> OnTypewriterProgress;
    public event Action OnDialogueStarted;
    public event Action OnDialogueEnded;

    public bool IsActive { get; private set; }

    private enum State { Idle, Typing, WaitingForAdvance }
    private State state = State.Idle;

    private DialogueSequence currentSequence;
    private int currentLineIndex;
    private Coroutine typeCoroutine;
    private Action pendingOnComplete;

    private void Update()
    {
        if (!IsActive) return;

        bool advancePressed = false;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame))
            advancePressed = true;

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            advancePressed = true;

        if (!advancePressed) return;

        if (state == State.Typing)
        {
            CompleteCurrentLine();
        }
        else if (state == State.WaitingForAdvance)
        {
            AdvanceLine();
        }
    }

    public void StartDialogue(DialogueSequence sequence, Action onComplete = null)
    {
        if (sequence == null || sequence.lines == null || sequence.lines.Length == 0) return;

        currentSequence = sequence;
        currentLineIndex = 0;
        pendingOnComplete = onComplete;
        IsActive = true;
        Time.timeScale = 0f;

        OnDialogueStarted?.Invoke();
        ShowLine(currentLineIndex);
    }

    private void ShowLine(int index)
    {
        DialogueLine line = currentSequence.lines[index];
        OnLineChanged?.Invoke(line);

        if (typeCoroutine != null) StopCoroutine(typeCoroutine);
        typeCoroutine = StartCoroutine(TypeLine(line));
    }

    private IEnumerator TypeLine(DialogueLine line)
    {
        state = State.Typing;

        float cps = line.typingSpeedOverride > 0f ? line.typingSpeedOverride : currentSequence.defaultCharsPerSecond;
        float interval = cps > 0f ? 1f / cps : 0f;

        string text = line.text ?? string.Empty;
        int shown = 0;

        while (shown < text.Length)
        {
            shown++;
            OnTypewriterProgress?.Invoke(text.Substring(0, shown));

            if (interval > 0f)
                yield return new WaitForSecondsRealtime(interval);
        }

        typeCoroutine = null;
        state = State.WaitingForAdvance;
    }

    private void CompleteCurrentLine()
    {
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
        }

        string fullText = currentSequence.lines[currentLineIndex].text ?? string.Empty;
        OnTypewriterProgress?.Invoke(fullText);
        state = State.WaitingForAdvance;
    }

    private void AdvanceLine()
    {
        currentLineIndex++;

        if (currentLineIndex >= currentSequence.lines.Length)
        {
            EndDialogue();
        }
        else
        {
            ShowLine(currentLineIndex);
        }
    }

    private void EndDialogue()
    {
        state = State.Idle;
        IsActive = false;

        if (pauseController == null || !pauseController.IsPaused)
            Time.timeScale = 1f;

        currentSequence = null;

        OnDialogueEnded?.Invoke();

        Action callback = pendingOnComplete;
        pendingOnComplete = null;
        callback?.Invoke();
    }
}
