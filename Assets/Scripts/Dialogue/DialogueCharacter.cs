using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterExpression
{
    public string name;
    public Sprite sprite;
    public bool overrideTint;
    public Color tint = Color.white;
    public bool overrideScale;
    public float scale = 1f;
}

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Dialogue/Character")]
public class DialogueCharacter : ScriptableObject
{
    public string displayName;
    public Sprite portrait;
    public Color portraitTint = Color.white;

    [Tooltip("Uniform size multiplier applied to this character's portrait (1 = normal size).")]
    public float portraitScale = 1f;

    [Tooltip("Optional named expressions (e.g. Happy, Angry). A line with a matching Expression uses these instead of the base portrait/tint/scale.")]
    public List<CharacterExpression> expressions = new List<CharacterExpression>();

    public Sprite GetPortrait(string expressionName)
    {
        CharacterExpression match = FindExpression(expressionName);
        if (match != null && match.sprite != null) return match.sprite;
        return portrait;
    }

    public Color GetTint(string expressionName)
    {
        CharacterExpression match = FindExpression(expressionName);
        if (match != null && match.overrideTint) return match.tint;
        return portraitTint;
    }

    public float GetScale(string expressionName)
    {
        CharacterExpression match = FindExpression(expressionName);
        if (match != null && match.overrideScale) return match.scale;
        return portraitScale;
    }

    private CharacterExpression FindExpression(string expressionName)
    {
        if (string.IsNullOrEmpty(expressionName)) return null;

        for (int i = 0; i < expressions.Count; i++)
        {
            if (expressions[i].name == expressionName) return expressions[i];
        }

        return null;
    }
}
