using System;
using UnityEngine;

public class SanitySystem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxSanity = 100f;
    [SerializeField] private float startingSanity = 100f;

    public float CurrentSanity { get; private set; }
    public float MaxSanity => maxSanity;
    public float Percent => CurrentSanity / MaxSanity;

    public event Action<float, float> OnSanityChanged;
    public event Action OnSanityEmpty;
    public event Action OnSanityFull;
    public event Action<SanityStage> OnStageChanged;

    private SanityStage currentStage;

    public enum SanityStage
    {
        Stable,
        Uneasy,
        Disturbed,
        Insane
    }

    private void Awake()
    {
        CurrentSanity = Mathf.Clamp(startingSanity, 0, maxSanity);
        NotifyChanged();
    }

    public void Gain(float amount)
    {
        if (amount <= 0) return;

        CurrentSanity = Mathf.Min(CurrentSanity + amount, maxSanity);

        if (CurrentSanity >= maxSanity)
            OnSanityFull?.Invoke();

        NotifyChanged();
    }

    public void Lose(float amount)
    {
        if (amount <= 0) return;

        CurrentSanity = Mathf.Max(CurrentSanity - amount, 0);

        if (CurrentSanity <= 0)
            OnSanityEmpty?.Invoke();

        NotifyChanged();
    }

    public bool Spend(float amount)
    {
        if (CurrentSanity < amount)
            return false;

        CurrentSanity -= amount;

        if (CurrentSanity <= 0)
            OnSanityEmpty?.Invoke();

        NotifyChanged();
        return true;
    }

    public void SetSanity(float value)
    {
        CurrentSanity = Mathf.Clamp(value, 0, maxSanity);
        NotifyChanged();
    }

    private void NotifyChanged()
    {
        UpdateStage();

        OnSanityChanged?.Invoke(CurrentSanity, maxSanity);
    }

    private void UpdateStage()
    {
        SanityStage stage;

        if (Percent > 0.75f)
            stage = SanityStage.Stable;
        else if (Percent > 0.5f)
            stage = SanityStage.Uneasy;
        else if (Percent > 0.25f)
            stage = SanityStage.Disturbed;
        else
            stage = SanityStage.Insane;

        if (stage != currentStage)
        {
            currentStage = stage;
            OnStageChanged?.Invoke(stage);
        }
    }
}