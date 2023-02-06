using UnityEngine;


[DisallowMultipleComponent]
public class LightTorch : MonoBehaviour
{
    private UnityEngine.Rendering.Universal.Light2D light2D;
    private float lightTorchTimer;
    #region Tooltip
    [Tooltip("The mininum intensity the light will have. " +
        "The light will switch constantly between the min and max values")]
    #endregion
    [SerializeField] private float minLightIntensity;
    #region Tooltip
    [Tooltip("The maximum intensity the light will have. " +
        "The light will switch constantly between the min and max values")]
    #endregion
    [SerializeField] private float maxLightIntensity;

    #region Tooltip
    [Tooltip("The min time it will take for the light to switch its intensity")]
    #endregion
    [SerializeField] private float lightTorchMinTime;
    #region Tooltip
    [Tooltip("The max time it will take for the light to switch its intensity")]
    #endregion
    [SerializeField] private float lightTorchMaxTime;

    private void Awake()
    {
        light2D = GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>();
    }

    private void Start()
    {
        lightTorchTimer = Random.Range(lightTorchMinTime, lightTorchMaxTime);
    }

    private void Update()
    {
        if (light2D == null) return;

        lightTorchTimer -= Time.deltaTime;

        if (lightTorchTimer < 0f)
        {
            lightTorchTimer = Random.Range(lightTorchMinTime, lightTorchMaxTime);

            RandomiseLightIntensity();
        }
    }

    private void RandomiseLightIntensity()
    {
        light2D.intensity = Random.Range(minLightIntensity, maxLightIntensity);
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(minLightIntensity), minLightIntensity, nameof(maxLightIntensity), maxLightIntensity, false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(lightTorchMinTime), lightTorchMinTime, nameof(lightTorchMaxTime), lightTorchMaxTime, false);
    }
#endif
    #endregion
}
