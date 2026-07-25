using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public enum Side { Left, Right, Solo }

    public DialogueCharacter character;

    [TextArea(2, 5)]
    public string text;

    [Tooltip("Optional expression name matching one of the character's Expressions. Leave blank to use the character's base portrait.")]
    public string expression;

    public Side side = Side.Left;

    [Tooltip("Characters revealed per second for this line. -1 uses the sequence default.")]
    public float typingSpeedOverride = -1f;
}
