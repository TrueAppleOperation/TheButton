using UnityEngine;
using UnityEngine.Events;

public class InteractiveButton : MonoBehaviour
{
    public GameObject buttonTop;
    public float pressDistance = 0.1f;
    public UnityEvent OnClick;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public bool playSoundOnce = true;

    [Header("Light Settings")]
    public Light targetLight;
    public bool turnLightOff = true;

    [Header("Particle Settings")]
    public ParticleSystem targetParticles;
    public bool turnOffParticles = true;

    [Header("Fog Settings")]
    public bool enableFogOnPress = true;
    public FogMode fogMode = FogMode.Exponential;
    public Color fogColor = Color.gray;
    public float fogDensity = 0.01f;
    public float linearFogStart = 0.0f;
    public float linearFogEnd = 300.0f;

    private Vector3 originalPosition;
    private bool isPressed = false;
    private bool hasPlayedSound = false;
    private bool lightWasOn = false;
    private bool canPress = true;

    void Start()
    {
        if (buttonTop != null)
        {
            originalPosition = buttonTop.transform.localPosition;
            Debug.Log("ButtonTop reference: " + buttonTop.name);
        }
        else
        {
            Debug.LogError("BUTTONTOP REFERENCE IS NULL!");
        }

        // Store initial light state
        if (targetLight != null)
        {
            lightWasOn = targetLight.enabled;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canPress)
        {
            CheckForClick();
        }

        if (Input.GetMouseButtonUp(0) && isPressed)
        {
            ReleaseButton();
        }
    }

    void CheckForClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.collider.gameObject == buttonTop)
            {
                if (!isPressed && canPress)
                {
                    PressButton();
                }
            }
        }
    }

    void PressButton()
    {
        canPress = false;
        isPressed = true;

        // Move button
        if (buttonTop != null)
        {
            buttonTop.transform.localPosition = originalPosition - new Vector3(0, pressDistance, 0);
            Debug.Log("Button should be moving. New position: " + buttonTop.transform.localPosition);
        }
        else
        {
            Debug.LogError("Cannot move - buttonTop is null!");
        }

        // Play sound only once
        if (audioSource != null)
        {
            if (playSoundOnce)
            {
                if (!hasPlayedSound)
                {
                    audioSource.Play();
                    hasPlayedSound = true;
                    Debug.Log("Sound played (once only). hasPlayedSound is now: " + hasPlayedSound);
                }
                else
                {
                    Debug.Log("Sound NOT played - hasPlayedSound is: " + hasPlayedSound);
                }
            }
            else
            {
                audioSource.Play();
                Debug.Log("Sound played (multiple times allowed)");
            }
        }
        else
        {
            Debug.LogWarning("AudioSource is null!");
        }

        // Turn off light
        if (targetLight != null && turnLightOff)
        {
            lightWasOn = targetLight.enabled;
            targetLight.enabled = false;
            Debug.Log("Light turned off");
        }

        // Turn off particles
        if (targetParticles != null && turnOffParticles)
        {
            targetParticles.Stop();
            Debug.Log("Particles turned off");
        }

        // Enable fog
        if (enableFogOnPress)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogColor = fogColor;

            switch (fogMode)
            {
                case FogMode.Linear:
                    RenderSettings.fogStartDistance = linearFogStart;
                    RenderSettings.fogEndDistance = linearFogEnd;
                    break;
                case FogMode.Exponential:
                case FogMode.ExponentialSquared:
                    RenderSettings.fogDensity = fogDensity;
                    break;
            }

            Debug.Log("Fog enabled with mode: " + fogMode);
        }

        // Invoke events
        if (OnClick != null)
        {
            Debug.Log("OnClick event exists with " + OnClick.GetPersistentEventCount() + " listeners");
            OnClick.Invoke();
        }
        else
        {
            Debug.LogError("OnClick event is null!");
        }
    }

    void ReleaseButton()
    {
        Debug.Log("Releasing button");
        isPressed = false;

        // Add cooldown before allowing another press
        Invoke("EnablePress", 0.2f);

        if (buttonTop != null)
        {
            buttonTop.transform.localPosition = originalPosition;
        }
    }

    void EnablePress()
    {
        canPress = true;
    }

    public void ResetSound()
    {
        hasPlayedSound = false;
        Debug.Log("Sound reset - can play again");
    }

    public void ToggleLight()
    {
        if (targetLight != null)
        {
            targetLight.enabled = !targetLight.enabled;
            Debug.Log("Light toggled to: " + targetLight.enabled);
        }
    }

    public void RestoreLightState()
    {
        if (targetLight != null)
        {
            targetLight.enabled = lightWasOn;
            Debug.Log("Light restored to original state: " + lightWasOn);
        }
    }

    public void ToggleParticles()
    {
        if (targetParticles != null)
        {
            if (targetParticles.isPlaying)
            {
                targetParticles.Stop();
            }
            else
            {
                targetParticles.Play();
            }
            Debug.Log("Particles toggled. Playing: " + targetParticles.isPlaying);
        }
    }

    public void RestartParticles()
    {
        if (targetParticles != null)
        {
            targetParticles.Play();
            Debug.Log("Particles restarted");
        }
    }

    // Fog control
    public void ToggleFog()
    {
        RenderSettings.fog = !RenderSettings.fog;
        Debug.Log("Fog toggled to: " + RenderSettings.fog);
    }

    public void SetFogDensity(float density)
    {
        RenderSettings.fogDensity = density;
        Debug.Log("Fog density set to: " + density);
    }

    public void SetFogColor(Color color)
    {
        RenderSettings.fogColor = color;
        Debug.Log("Fog color changed");
    }

    public void DisableFog()
    {
        RenderSettings.fog = false;
        Debug.Log("Fog disabled");
    }
}