using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimulationSpeedController : MonoBehaviour
{
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TextMeshProUGUI speedText;

    private void Start()
    {
        speedSlider.minValue = 1;
        speedSlider.maxValue = 100;
        speedSlider.wholeNumbers = true;

        speedSlider.value = 1;

        ApplySpeed(1);

        speedSlider.onValueChanged.AddListener(ApplySpeed);
    }

    private void ApplySpeed(float value)
    {
        Time.timeScale = value;

        if (speedText != null)
        {
            speedText.text = $"Speed: x{value:0}";
        }
    }

    private void OnDestroy()
    {
        speedSlider.onValueChanged.RemoveListener(ApplySpeed);
    }
}