using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class InteractiveButton : MonoBehaviour
{
    public float WinTime = 32f;
    public GameObject buttonTop;
    public float pressDistance = 0.1f;
    public UnityEvent OnClick;

    [Header("Camera Settings")]
    public Camera mainCamera;
    private Vector3 originalCameraPosition;
    private float originalFOV;
    private Coroutine fovCoroutine;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioSource ButtonClick;
    public bool playSoundOnce = true;

    [Header("New Audio Settings")]
    public AudioSource introAudioSource;
    public AudioSource delayedRingingAudio; // New audio source for delayed ringing

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

    [Header("Blackout Effects")]
    public GameObject blackoutPanel; // A simple opaque black quad or image canvas
    private Color originalCameraColor;

    [Header("Incremental Effects")]
    public Color clickOneFogColor = Color.gray;
    public Color clickTwoLightColor = Color.red;

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
    private bool hasScheduledDelayedAudio = false; // Track if delayed audio
    private Color originalColor;

    void Start()
    {
        CancelInvoke();

        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            originalCameraColor = mainCamera.backgroundColor;
            originalCameraPosition = mainCamera.transform.localPosition;
            originalFOV = mainCamera.fieldOfView;
        }
        // Ensure the blackout panel starts invisible
        if (blackoutPanel != null)
        {
            blackoutPanel.SetActive(false);
        }

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

        // Schedule delayed ringing audio only on first click
        if (clickCount == 1 && !hasScheduledDelayedAudio && delayedRingingAudio != null)
        {
            Invoke("PlayDelayedRinging", 22f);
            hasScheduledDelayedAudio = true;
            Debug.Log("Delayed ringing audio scheduled to play in 22 seconds");
        }

        if (clickCount == 2 && delayedRingingAudio != null)
        {
            StopDelayedRinging();
            Debug.Log("Delayed ringing audio stopped on second click");
        }

        
        ButtonClick.Play();
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
                    if (countdownText != null)
                    {
                        countdownText.text = "You should not have done that";
                        Invoke("ClearWarningText", 5.0f);
                    }
                    Debug.Log("You should not have done that");
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

    void PlayDelayedRinging()
    {
        if (delayedRingingAudio != null && gameIsActive)
        {
            delayedRingingAudio.Play();
            Debug.Log("Delayed ringing audio started playing after 10 seconds");
        }
    }

    void StopDelayedRinging()
    {
        CancelInvoke("PlayDelayedRinging");

        if (delayedRingingAudio != null && delayedRingingAudio.isPlaying)
        {
            delayedRingingAudio.Stop();
            Debug.Log("Delayed ringing audio stopped");
        }

        hasScheduledDelayedAudio = false;
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
                // Stop delayed ringing audio at second click
                StopDelayedRinging();

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
                StartScreenShake(4.0f);
                Debug.Log("Click 3: Screen Shake Effect");
                break;

            case 4:
                QuickBlackout(1.5f);
                Debug.Log("Click 4: Quick Blackout Effect!");
                break;
            case 5:
                if (targetLight != null)
                {
                    targetLight.color = new Color(0.5f, 0f, 1f); // Brighter Purple
                }
                RenderSettings.fogColor = new Color(0.2f, 0.8f, 0.2f); // Neon green fog
                RenderSettings.fogDensity = 0.07f;
                Debug.Log("Click 5: Brighter purple light and neon green fog (Acid Trip begins).");
                break;
            case 6:
                StartColorTrip();
                Debug.Log("Click 6: ACID TRIP MODE ENGAGED!");
                break;
            case 7:
                StopColorTrip();
                fovCoroutine = StartCoroutine(ExecuteFOVSequence(30f, 90f, 1f, 2f));
                if (targetParticles != null)
                {
                    targetParticles.Stop();
                }
                Debug.Log("Click 8: Particles stop flowing.");
                break;
            
            default:
                if (targetLight != null)
                {
                    targetLight.enabled = true;
                    targetLight.color = new Color(Random.value, Random.value, Random.value);
                    targetLight.intensity = Random.Range(3000f, 20000f); // Constant flickering
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

        // Cancel any pending delayed audio if the game is won
        StopDelayedRinging();
        introAudioSource.Stop();
        SceneManager.LoadScene("END");       
    }

    public void ClearWarningText()
    {
        if (countdownText != null)
        {
            // Reset text to empty string or dashes
            countdownText.text = null;
            Debug.Log("Warning text cleared after 5 seconds.");
        }
    }
    void QuickBlackout(float duration = 0.5f)
    {
        if (blackoutPanel != null)
        {
            blackoutPanel.SetActive(true);
            // Use Invoke to schedule turning it off
            Invoke("EndBlackout", duration);
        }
    }
    private void EndBlackout()
    {
        if (blackoutPanel != null)
        {
            blackoutPanel.SetActive(false);
        }
    }
    void RestoreCameraBg()
    {
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = originalColor;
        }
    }
    void StartColorTrip()
    {
        InvokeRepeating("CycleColors", 0f, 0.1f); // Change color every 0.1 seconds
    }

    void CycleColors()
    {
        Color randomColor = new Color(Random.value, Random.value, Random.value);

        RenderSettings.fogColor = randomColor;

        if (targetLight != null)
        {
            targetLight.color = randomColor;
        }
    }

    void StopColorTrip()
    {
        CancelInvoke("CycleColors");
    }
    public void StartScreenShake(float duration = 1.0f)
    {
        // Use InvokeRepeating to continuously apply random displacement
        InvokeRepeating("ShakeCamera", 0f, 0.01f);

        // Schedule the stop after the desired duration
        Invoke("StopScreenShake", duration);
    }
    private void ShakeCamera()
    {
        if (mainCamera != null)
        {
            // Randomly displace the camera
            float x = Random.Range(-0.05f, 0.05f);
            float y = Random.Range(-0.05f, 0.05f);

            mainCamera.transform.localPosition = originalCameraPosition + new Vector3(x, y, 0);
        }
    }
    private void StopScreenShake()
    {
        CancelInvoke("ShakeCamera");
        if (mainCamera != null)
        {
            // Return the camera to its original, steady position
            mainCamera.transform.localPosition = originalCameraPosition;
        }
    }
    private IEnumerator AnimateFOV(float targetFOV, float duration)
    {
        if (mainCamera == null) yield break;

        float startFOV = mainCamera.fieldOfView;
        float time = 0;

        while (time <= duration)
        {
            mainCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, time / duration);
            time += Time.deltaTime;
            yield return null; // Wait until the next frame
        }
        mainCamera.fieldOfView = targetFOV; // Ensure it ends exactly at targetFOV
    }
    private IEnumerator ExecuteFOVSequence(float LowFOVBound, float HighFOVBound, float transitionDuration, float times)
    {
        float n = 0;
        while (n <= times)
        {
            // 1. Transition UP: From current FOV (Original) to High Bound
            Debug.Log("FOV Sequence 1: Going UP to High Bound.");
            yield return StartCoroutine(AnimateFOV(HighFOVBound, transitionDuration));

            // 2. Transition DOWN: From High Bound back to Low Bound
            Debug.Log("FOV Sequence 2: Going DOWN to Low Bound.");
            yield return StartCoroutine(AnimateFOV(LowFOVBound, transitionDuration*2));

            // 3. Transition RESET: From Low Bound back to Original FOV
            // Note: We use originalFOV, which was stored in Start().
            Debug.Log("FOV Sequence 3: Returning to Normal.");
            yield return StartCoroutine(AnimateFOV(originalFOV, transitionDuration));
            n++;
        }
    }
}