using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SanityUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SanitySystem sanitySystem;

    [SerializeField] private Slider sanitySlider;
    [SerializeField] private TMP_Text sanityText;

    [Header("Animation")]
    [SerializeField] private float smoothSpeed = 8f;

    private float targetValue;

    private void Start()
    {
        sanitySlider.maxValue = sanitySystem.MaxSanity;
        targetValue = sanitySystem.CurrentSanity;

        UpdateText();
    }

    private void OnEnable()
    {
        sanitySystem.OnSanityChanged += UpdateUI;
    }

    private void OnDisable()
    {
        sanitySystem.OnSanityChanged -= UpdateUI;
    }

    private void Update()
    {
        sanitySlider.value = Mathf.Lerp(
            sanitySlider.value,
            targetValue,
            Time.deltaTime * smoothSpeed);
    }

    private void UpdateUI(float current, float max)
    {
        sanitySlider.maxValue = max;
        targetValue = current;

        UpdateText();
    }

    private void UpdateText()
    {
        sanityText.text =
            $"{Mathf.CeilToInt(targetValue)} / {Mathf.CeilToInt(sanitySystem.MaxSanity)}";
    }
}