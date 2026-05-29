using UnityEngine;

public class EmissionPulse : MonoBehaviour
{
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 1.5f;

    [SerializeField] private float pulseSpeed = 3f;

    private Material materialInstance;
    private Color baseEmissionColor;

    private void Start()
    {
        Renderer renderer = GetComponent<Renderer>();

        materialInstance = renderer.material;

        materialInstance.EnableKeyword("_EMISSION");

        baseEmissionColor =
            materialInstance.GetColor("_EmissionColor");
    }

    private void Update()
    {
        float t = Mathf.PingPong(
            Time.time * pulseSpeed,
            1f);

        float intensity = Mathf.Lerp(
            minIntensity,
            maxIntensity,
            t);

        materialInstance.SetColor(
            "_EmissionColor",
            baseEmissionColor * intensity);
    }
}