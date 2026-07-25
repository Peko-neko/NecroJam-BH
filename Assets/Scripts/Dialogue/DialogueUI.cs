using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private GameObject rootPanel;

    [SerializeField] private GameObject leftPortraitRoot;
    [SerializeField] private Image leftPortraitImage;
    [SerializeField] private GameObject rightPortraitRoot;
    [SerializeField] private Image rightPortraitImage;
    [SerializeField] private GameObject soloPortraitRoot;
    [SerializeField] private Image soloPortraitImage;

    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private GameObject advanceIndicator;

    [SerializeField] private Color activeTint = Color.white;
    [SerializeField] private Color inactiveTint = new Color(0.4f, 0.4f, 0.4f, 1f);

    private DialogueLine currentLine;
    private bool leftSeen;
    private bool rightSeen;

    private void OnEnable()
    {
        dialogueManager.OnDialogueStarted += HandleStarted;
        dialogueManager.OnLineChanged += HandleLineChanged;
        dialogueManager.OnTypewriterProgress += HandleProgress;
        dialogueManager.OnDialogueEnded += HandleEnded;
    }

    private void OnDisable()
    {
        dialogueManager.OnDialogueStarted -= HandleStarted;
        dialogueManager.OnLineChanged -= HandleLineChanged;
        dialogueManager.OnTypewriterProgress -= HandleProgress;
        dialogueManager.OnDialogueEnded -= HandleEnded;
    }

    private void HandleStarted()
    {
        leftSeen = false;
        rightSeen = false;
        leftPortraitRoot.SetActive(false);
        rightPortraitRoot.SetActive(false);
        soloPortraitRoot.SetActive(false);
        rootPanel.SetActive(true);
    }

    private void HandleLineChanged(DialogueLine line)
    {
        currentLine = line;
        speakerNameText.text = line.character != null ? line.character.displayName : string.Empty;
        advanceIndicator.SetActive(false);

        Sprite portrait = line.character != null ? line.character.GetPortrait(line.expression) : null;
        Color tint = line.character != null ? line.character.GetTint(line.expression) : Color.white;
        float scale = line.character != null ? line.character.GetScale(line.expression) : 1f;

        if (line.side == DialogueLine.Side.Solo)
        {
            leftPortraitRoot.SetActive(false);
            rightPortraitRoot.SetActive(false);
            soloPortraitRoot.SetActive(true);
            soloPortraitImage.sprite = portrait;
            soloPortraitImage.color = tint;
            soloPortraitImage.rectTransform.localScale = Vector3.one * scale;
            return;
        }

        soloPortraitRoot.SetActive(false);

        if (line.side == DialogueLine.Side.Left)
        {
            leftSeen = true;
            leftPortraitRoot.SetActive(true);
            leftPortraitImage.sprite = portrait;
            leftPortraitImage.color = tint * activeTint;
            leftPortraitImage.rectTransform.localScale = Vector3.one * scale;

            rightPortraitRoot.SetActive(rightSeen);
            if (rightSeen) rightPortraitImage.color = inactiveTint;
        }
        else
        {
            rightSeen = true;
            rightPortraitRoot.SetActive(true);
            rightPortraitImage.sprite = portrait;
            rightPortraitImage.color = tint * activeTint;
            rightPortraitImage.rectTransform.localScale = Vector3.one * scale;

            leftPortraitRoot.SetActive(leftSeen);
            if (leftSeen) leftPortraitImage.color = inactiveTint;
        }
    }

    private void HandleProgress(string revealed)
    {
        bodyText.text = revealed;

        if (currentLine != null && revealed.Length >= (currentLine.text?.Length ?? 0))
            advanceIndicator.SetActive(true);
    }

    private void HandleEnded()
    {
        rootPanel.SetActive(false);
    }
}
