using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public enum Side { Left, Right, Solo }

    public string speakerName;

    [TextArea(2, 5)]
    public string text;

    public Sprite portrait;
    public Color portraitTint = Color.white;
    public Side side = Side.Left;

    [Tooltip("Characters revealed per second for this line. -1 uses the sequence default.")]
    public float typingSpeedOverride = -1f;
}
