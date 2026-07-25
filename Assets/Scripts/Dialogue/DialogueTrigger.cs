using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueSequence sequence;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool triggerOnce = true;

    private bool hasFired;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasFired && triggerOnce) return;
        if (!other.CompareTag(playerTag)) return;
        if (dialogueManager == null || sequence == null) return;

        hasFired = true;
        dialogueManager.StartDialogue(sequence);

        if (triggerOnce)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }
}
