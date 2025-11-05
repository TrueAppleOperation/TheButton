using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class InteractiveButton : MonoBehaviour
{
    public float WinTime = 30f;
    public GameObject buttonTop;
    public float pressDistance = 0.1f;
    public UnityEvent OnClick;


    [Header("Audio Settings")]
    public AudioSource audioSource;
    public bool playSoundOnce = true;

    [Header("New Audio Settings")]
    public AudioSource introAudioSource; 

    [Header("UI Settings")]
    public TMP_Text countdownText; 

    [Header("Light Settings")]
    public Light targetLight;
    private Color initialLightColor = Color.white;
    //public bool turnLightOff = true;

    [Header("Particle Settings")]
    public ParticleSystem targetParticles;
    //public bool turnOffParticles = true;

    [Header("Fog Settings")]
    public bool enableFogOnPress = true;
    public FogMode fogMode = FogMode.Exponential;
    public Color fogColor = Color.gray;
    public float fogDensity = 0.01f;
    public float linearFogStart = 0.0f;
    public float linearFogEnd = 300.0f;

    [Header("Incremental Effects")]
    public Color clickOneFogColor = Color.gray;
    public Color clickTwoLightColor = Color.red;
    public Color clickThreeAmbientColor = Color.black; // For a screen darkening effect

    private Vector3 originalPosition;
    private bool isPressed = false;
    private bool hasPlayedSound = false;
    private bool lightWasOn = false;
    private bool canPress = true;

    private int clickCount = 0;
    private float winTimer = 0f;
    private bool gameIsActive = true;
    private bool gameIsWon = false;
    private Color originalAmbientColor;
    private bool buttonWasPressed = false; // Track if button was ever pressed

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
        originalAmbientColor = RenderSettings.ambientLight;
        if (targetLight != null)
        {
            initialLightColor = targetLight.color;
        }
        RenderSettings.fog = false; // Start with fog disabled
        winTimer = 0f;

        // Initialize countdown text
        if (countdownText != null)
        {
            countdownText.text = WinTime.ToString("F0");
        }
        else
        {
            Debug.LogWarning("Countdown Text (TMP) is not assigned!");
        }

        // Play intro audio at the beginning
        if (introAudioSource != null)
        {
            introAudioSource.Play();
            Debug.Log("Intro audio started playing");
        }
        else
        {
            Debug.LogWarning("IntroAudioSource is null - no intro audio will play");
        }
    }

    void Update()
    {
        if (gameIsActive && !gameIsWon)
        {
            if (Input.GetMouseButtonDown(0) && canPress)
            {
                CheckForClick();
            }

            if (Input.GetMouseButtonUp(0) && isPressed)
            {
                ReleaseButton();
            }

            winTimer += Time.deltaTime;

            // Update countdown text
            if (countdownText != null && !buttonWasPressed)
            {
                float timeRemaining = Mathf.Max(0f, WinTime - winTimer);
                countdownText.text = timeRemaining.ToString("F0");
            }

            if (winTimer >= WinTime)
            {
                WinGame();
            }
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
        if (!gameIsActive) return;
        canPress = false;
        isPressed = true;
        buttonWasPressed = true;

        clickCount++;
        winTimer = 0f; // Reset timer on click
        Debug.Log("Button pressed! Click count: " + clickCount);

        if (introAudioSource != null && introAudioSource.isPlaying)
        {
            introAudioSource.Stop();
            Debug.Log("Intro audio stopped");
        }

        if (countdownText != null)
        {
            countdownText.text = "You should not have done that";
            countdownText.fontSize = 24;
        }
        Debug.Log("You should not have done that");

        ApplyIncrementalEffect(clickCount);

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

    void ApplyIncrementalEffect(int count)
    {
        switch (count)
        {
            case 1:
                if (targetLight != null)
                {
                    targetLight.enabled = false;
                }
                // Effect 1: Fog Appears(The original effect)
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Exponential;
                RenderSettings.fogColor = clickOneFogColor;
                RenderSettings.fogDensity = 0.01f;
                Debug.Log("Click 1: Fog is enabled.");
                break;

            case 2:
                // Effect 2: Light Changes Color
                if (targetLight != null)
                {
                    ToggleLight();
                    targetLight.color = clickTwoLightColor; // Red light
                }
                RenderSettings.ambientLight = originalAmbientColor;
                Debug.Log("Click 2: Quick, intense RED light flash.");
                break;

            case 3:
                // Effect 3: Darken the World (Ambient Light)
                RenderSettings.ambientLight = clickThreeAmbientColor; // Makes everything very dark
                if (targetLight != null)
                {
                    targetLight.color = new Color(0f, 1f, 1f); // Cyan
                }
                Debug.Log("Click 3: Ambient light darkens + Magenta Flash.");
                break;

            case 4:
                // Effect 4: Stop Particles
                if (targetParticles != null)
                {
                    targetParticles.Stop();
                }
                Debug.Log("Click 4: Particles stop flowing.");
                break;

            case 5:
                // Effect 5: Reset Light Color, Increase Fog Density 
                if (targetLight != null)
                {
                    targetLight.color = new Color(0.5f, 0f, 1f); // Brighter Purple
                }
                RenderSettings.fogColor = new Color(0.2f, 0.8f, 0.2f); // Neon green fog
                RenderSettings.fogDensity = 0.07f;
                Debug.Log("Click 5: Brighter purple light and neon green fog (Acid Trip begins).");
                break;

            default:
                if (targetLight != null)
                {
                    targetLight.enabled = true;
                    targetLight.color = new Color(Random.value, Random.value, Random.value);
                    targetLight.intensity = Random.Range(7000f, 10000f); // Constant flickering
                }
                Debug.Log("Click " + count + ": System overload effect!");
                break;
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

    void WinGame()
    {
        gameIsWon = true;
        gameIsActive = false;

        if (countdownText != null)
        {
            if (!buttonWasPressed)
            {
                countdownText.text = "Congratulations! You won!";
                countdownText.fontSize = 24; 
                Debug.Log("Congratulations! You have won the game by surviving " + WinTime + " seconds without pressing the button!");
            }
            else
            {
                countdownText.text = "You should not have done that";
                Debug.Log("Game over - you pressed the button!");
            }
        }

        DisableFog();
        if (targetLight != null) targetLight.enabled = true;

        if (introAudioSource != null && introAudioSource.isPlaying)
        {
            introAudioSource.Stop();
            Debug.Log("Intro audio stopped (game won)");
        }
    }
}