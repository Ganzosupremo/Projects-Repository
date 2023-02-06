using Cinemachine;
using System.Collections;
using UnityEngine;

public class MyCameraShake : MonoBehaviour
{
    public static MyCameraShake Instance { get; private set; }
    public CinemachineVirtualCamera virtualCamera;
    public Transform cameraTransform;

    private float shakeTimer;
    private float shakeTimerTotal;
    private float startIntensity;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private void Awake()
    {
        Instance = this; 
        if (virtualCamera == null)
        {
            virtualCamera = GameObject.FindGameObjectWithTag("MainVirtualCamera").GetComponent<CinemachineVirtualCamera>();
        }

        if (cameraTransform == null)
        {
            cameraTransform = transform;
        }

        originalPosition = cameraTransform.localPosition;
        originalRotation = cameraTransform.localRotation;
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
                virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain =
                Mathf.Lerp(startIntensity, 0f, 1 - (shakeTimer / shakeTimerTotal));
        }
        else
        {
            StartCoroutine(ResetPosition());
        }
    }

    /// <summary>
    /// Makes the camera shake
    /// </summary>
    /// <param name="intensity">The intensity of which the camera will shake</param>
    /// <param name="time">The time the camera will shake</param>
    /// <param name="enable">Enables the camera shake</param>
    public void ShakeCamera(float intensity, float time, bool enable = false)
    {
        if (enable)
        {
            CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = 
                virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;

            shakeTimer = time;
            shakeTimerTotal = time;
            startIntensity = intensity;
        }
        else
        {
            StartCoroutine(ResetPosition());
        }
    }

    private IEnumerator ResetPosition()
    {
        yield return new WaitForEndOfFrame();
        cameraTransform.localPosition = originalPosition;
        cameraTransform.localRotation = originalRotation;
        yield return new WaitForEndOfFrame();
    }
}
