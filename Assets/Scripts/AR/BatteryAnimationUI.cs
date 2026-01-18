
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BatteryAnimator : MonoBehaviour
{
    [Header("Battery UI")]
    public Image batteryFill;
    public TMP_Text batteryPercent;
    public TMP_Text explanation_text;

    [Header("Potential Energy Formula")]
    public TMP_InputField massInput;      // Mass (m) = Human's mass in kg
    public TMP_InputField heightInput;    // Height (h) in centimeters (will be converted to meters)
    public TMP_Text potentialEnergyDisplay;  // Display result (Ep)
    public Button calculateButton;        // Button to calculate
    public Button resetButton;            // Button to reset animation
    private const float gravity = 9.8f;   // g = 9.8 m/s²
    private float calculatedEnergy = 0f;  // Stores Ep value
    
    [Header("Energy Scale (Elite NBA Jump = 90 cm at 100kg)")]
    [Tooltip("Set based on elite 90cm jump at 100kg. Formula: 100*9.8*0.9=882J. This is the maximum reference.")]
    public float maxRealisticEnergy = 882f;  // Maximum: Elite NBA jump (100kg at 90cm) = 882J

    [Header("Result Window")]
    public GameObject resultWindowCanvas;  // ResultWindowCanvas to show when animation finishes
    public Button resultCloseButton;       // Close button on the result window
    public TMP_Text resultFeedbackText;    // Text field to display feedback in result window
    public Image feedbackImage;            // Image to display visual feedback (assign PNG sprites here)
    public Sprite[] feedbackSprites;       // Array of sprites: [0]=Very Low, [1]=Low, [2]=Medium, [3]=High, [4]=Maximum

    [Header("Glow")]
    public Image glow;                     // glow אחד בלבד
    public Gradient glowGradient;          // גרדיאנט מוכן: ירוק→צהוב→כתום→אדום
    public float glowAlpha = 0.45f;

    [Header("Green Group (0–50%)")]
    public RectTransform greenGroup;
    public RectTransform greenPosLow;
    public RectTransform greenPosMid;

    [Header("Red Group (50–100%)")]
    public RectTransform redGroup;
    public RectTransform redPosMid;
    public RectTransform redPosHigh;

    [Header("Animation Speeds")]
    public float fillSpeed = 0.5f;
    public float moveSpeed = 1.2f;

    private float currentPercent = 0;
    private float targetPercent = 0;
    private Coroutine routine;


    void Start()
    {
        SetInstant(0);
        
        // Setup button listeners
        if (calculateButton != null)
            calculateButton.onClick.AddListener(CalculatePotentialEnergy);
        
        if (resetButton != null)
            resetButton.onClick.AddListener(() =>
            {
                ResetSimulation();
                CloseResultWindow();
            });        
        if (resultCloseButton != null)
            resultCloseButton.onClick.AddListener(CloseResultWindow);

        // Start with result window hidden
        if (resultWindowCanvas != null)
            resultWindowCanvas.SetActive(false);

    }

    //----------------------------------------------------------
    // POTENTIAL ENERGY CALCULATION
    //----------------------------------------------------------
    public void CalculatePotentialEnergy()
    {
        Debug.Log("[BatteryAnimator] ===== CALCULATION START =====");
        
        if (massInput == null || heightInput == null)
        {
            Debug.LogWarning("[BatteryAnimator] Mass or Height input field is missing!");
            return;
        }

        // Parse inputs
        string massText = massInput.text;
        string heightCmText = heightInput.text;  // Height in centimeters
        
        Debug.Log($"[BatteryAnimator] Raw input - Mass text: '{massText}', Height (cm) text: '{heightCmText}'");
        
        if (!float.TryParse(massText, out float mass) || mass <= 0)
        {
            Debug.LogWarning("[BatteryAnimator] Invalid mass value. Please enter a positive number.");
            if (potentialEnergyDisplay != null)
                potentialEnergyDisplay.text = "Invalid mass!";
            return;
        }

        if (!float.TryParse(heightCmText, out float heightCm) || heightCm < 0)
        {
            Debug.LogWarning("[BatteryAnimator] Invalid height value. Please enter a non-negative number (in cm).");
            if (potentialEnergyDisplay != null)
                potentialEnergyDisplay.text = "Invalid height!";
            return;
        }

        // Convert height from centimeters to meters
        float height = heightCm / 100f;
        
        // Clamp height to maximum of 90 cm (elite jump)
        if (heightCm > 90f)
        {
            Debug.LogWarning($"[BatteryAnimator] Height {heightCm}cm exceeds maximum 90cm. Clamping to 90cm.");
            heightCm = 90f;
            height = 0.9f;
        }

        Debug.Log($"[BatteryAnimator] Parsed values - Mass: {mass}kg, Height: {heightCm}cm ({height}m), Gravity: {gravity}m/s²");
        
        // Calculate Potential Energy: Ep = m * g * h
        // where m is the HUMAN'S mass (not the model)
        calculatedEnergy = mass * gravity * height;
        
        Debug.Log($"[BatteryAnimator] Step 1 - Calculation: {mass} × {gravity} × {height} = {calculatedEnergy:F2}J");
        Debug.Log($"[BatteryAnimator] Step 2 - Max Energy set to: {maxRealisticEnergy}J");
        
        // Validate max energy is reasonable (should be 882J for 100kg at 90cm)
        if (maxRealisticEnergy < 800f || maxRealisticEnergy > 900f)
        {
            Debug.LogWarning($"[BatteryAnimator] ⚠️ WARNING: maxRealisticEnergy is {maxRealisticEnergy}J, but should be around 882J for elite 90cm jump!");
            Debug.LogWarning("[BatteryAnimator] Check Inspector → Energy Scale section and set maxRealisticEnergy to 882");
        }

        // Display result with max reference (90cm elite jump)
        if (potentialEnergyDisplay != null)
            potentialEnergyDisplay.text = $"{calculatedEnergy:F2}J(Ep) = \n{mass}(kg) * 9.8(g) * {heightCm}cm\nMax(90cm elite): {maxRealisticEnergy}J";

        // Map energy to animation percentage based on 100cm maximum jump
        float division = calculatedEnergy / maxRealisticEnergy;
        float animationPercent = division * 100f;
        float clampedPercent = Mathf.Clamp(animationPercent, 0, 100);
        
        Debug.Log($"[BatteryAnimator] Step 3 - Division: {calculatedEnergy:F2}J ÷ {maxRealisticEnergy}J = {division:F4}");
        Debug.Log($"[BatteryAnimator] Step 4 - Multiply by 100: {division:F4} × 100 = {animationPercent:F1}%");
        Debug.Log($"[BatteryAnimator] Step 5 - After Clamp(0,100): {clampedPercent:F1}%");
        Debug.Log($"[BatteryAnimator] ===== CALLING AnimateTo({clampedPercent:F1}%) =====");
        
        AnimateTo(clampedPercent);
    }

    //----------------------------------------------------------
    public void AnimateTo(float percent)
    {
        Debug.Log($"[BatteryAnimator] AnimateTo received: {percent}%");
        
        percent = Mathf.Clamp(percent, 0, 100);
        
        Debug.Log($"[BatteryAnimator] After Clamp in AnimateTo: {percent}%");
        
        targetPercent = percent;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(AnimateRoutine());
    }

    //----------------------------------------------------------
    public void OnStartButtonPressed()
    {
        //ResetSimulation();
        AnimateTo(90);   // אנימציה עד 90% לדוגמה
    }


    //----------------------------------------------------------
    public void SetInstant(float percent)
    {
        currentPercent = percent;
        UpdateUI(percent);
    }

    //----------------------------------------------------------
    IEnumerator AnimateRoutine()
    {
        float start = currentPercent;
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * fillSpeed;
            currentPercent = Mathf.Lerp(start, targetPercent, t);

            UpdateUI(currentPercent);
            yield return null;
        }

        currentPercent = targetPercent;
        UpdateUI(targetPercent);
        
        // Animation complete - show result window
        ShowResultWindow();
    }

    //----------------------------------------------------------
    void UpdateUI(float percent)
    {
        Debug.Log($"[BatteryAnimator] UpdateUI called with percent: {percent}%");
        
        float f = percent / 100f;

        // Battery fill
        batteryFill.fillAmount = f;
        batteryPercent.text = Mathf.RoundToInt(percent) + "%";

        Debug.Log($"[BatteryAnimator] Set batteryFill.fillAmount to: {f}, Text to: {Mathf.RoundToInt(percent)}%");

        // Glow צבע לפי גרדיאנט
        Color c = glowGradient.Evaluate(f);
        glow.color = new Color(c.r, c.g, c.b, glowAlpha);

        MoveGroups(percent);
        UpdateGlowPosition(percent);
        UpdateExplanation(percent);
    }

    //----------------------------------------------------------
    void MoveGroups(float percent)
    {
        if (percent <= 50f)
        {
            greenGroup.gameObject.SetActive(true);
            redGroup.gameObject.SetActive(false);

            float t = percent / 50f;

            greenGroup.anchoredPosition = Vector2.Lerp(
                greenPosLow.anchoredPosition,
                greenPosMid.anchoredPosition,
                t
            );
        }
        else
        {
            greenGroup.gameObject.SetActive(false);
            redGroup.gameObject.SetActive(true);

            float t = (percent - 50f) / 50f;

            redGroup.anchoredPosition = Vector2.Lerp(
                redPosMid.anchoredPosition,
                redPosHigh.anchoredPosition,
                t
            );
        }
    }

    //----------------------------------------------------------
    // ה-Glow נדבק תמיד לאותו Hub פעיל
    //----------------------------------------------------------
void UpdateGlowPosition(float percent)
{
    RectTransform hubRef;

    // קבוצה ירוקה
    if (percent <= 50f)
    {
        hubRef = greenGroup.Find("hub").GetComponent<RectTransform>();

        // glow צריך להיות ילד של greenGroup
        if (glow.transform.parent != greenGroup)
            glow.transform.SetParent(greenGroup, false);
    }
    // קבוצה אדומה
    else
    {
        hubRef = redGroup.Find("hub").GetComponent<RectTransform>();

        // glow צריך להיות ילד של redGroup
        if (glow.transform.parent != redGroup)
            glow.transform.SetParent(redGroup, false);
    }

    // מיקום מדויק
    glow.rectTransform.anchoredPosition = hubRef.anchoredPosition;
}

    //----------------------------------------------------------
    void UpdateExplanation(float percent)
    {
        string baseText = "";
        
        if (percent <= 50)
            baseText = "Lower jump = less potential energy";
        else if (percent < 100)
            baseText = "Higher jump = more potential energy";
        else
            baseText = "Maximum human jump energy reached!";

        // Add PE formula info if calculated
        if (calculatedEnergy > 0)
        {
            string energyPercent = ((calculatedEnergy / maxRealisticEnergy) * 100f).ToString("F1");
            baseText += $"\n\nHuman Jumping with Kettlebell Model\nEp = m × g × h = {calculatedEnergy:F2} J\n({energyPercent}% of typical jump capacity)";
        }

        explanation_text.text = baseText;
    }

    //----------------------------------------------------------
    private int GetFeedbackSpriteIndex(float percent)
    {
        // Return sprite index based on energy level
        if (percent < 25)
            return 0;  // Very Low
        else if (percent < 50)
            return 1;  // Low
        else if (percent < 75)
            return 2;  // Medium
        else if (percent < 100)
            return 3;  // High
        else
            return 4;  // Maximum
    }

    //----------------------------------------------------------
    private string GetFeedbackForPercent(float percent)
    {
        // Energy level feedback for kids
        if (percent < 25)
        {
            return "\nTry jumping higher to get more energy!";
        }
        else if (percent < 50)
        {
            return "\nGood start! Jump a bit higher for more!";
        }
        else if (percent < 75)
        {
            return "\nNice! You're getting stronger!";
        }
        else if (percent < 100)
        {
            return "\nAwesome! That's a powerful jump!";
        }
        else
        {
            return "\nYou're jumping like a champion!";
        }
    }

   /* public void ResetSimulation()
    {
        // נתחיל מאפס
        currentPercent = 0;
        targetPercent = 0;

        // לעצור אנימציה קודמת אם רצה
        if (routine != null)
            StopCoroutine(routine);

        // להחזיר ל-0 מיידית
        SetInstant(0);
    }
   */
    public void ResetSimulation()
    {
        Debug.Log("[BatteryAnimator] ResetSimulation called");
        
        // Stop animation
        if (routine != null)
            StopCoroutine(routine);

        // Reset animation values
        currentPercent = 0;
        targetPercent = 0;
        
        // Reset energy calculation
        calculatedEnergy = 0f;
        
        // Clear input fields
        if (massInput != null)
            massInput.text = "";
        if (heightInput != null)
            heightInput.text = "";

        // Clear displays
        if (potentialEnergyDisplay != null)
            potentialEnergyDisplay.text = "Enter mass and height";

        // Update UI to 0%
        SetInstant(0);
        
        Debug.Log("[BatteryAnimator] Animation reset to 0%");
    }

    private void ShowResultWindow()
    {
        Debug.Log("[BatteryAnimator] Showing result window");
        if (resultWindowCanvas != null)
        {
            resultWindowCanvas.SetActive(true);
            
            // Display feedback text in result window
            if (resultFeedbackText != null)
            {
                string feedback = GetFeedbackForPercent(currentPercent);
                resultFeedbackText.text = feedback;
                Debug.Log($"[BatteryAnimator] Result window feedback set: {feedback}");
            }
            else
            {
                Debug.LogWarning("[BatteryAnimator] resultFeedbackText is not assigned!");
            }
            
            // Display feedback image in result window
            if (feedbackImage != null && feedbackSprites != null && feedbackSprites.Length > 0)
            {
                int spriteIndex = GetFeedbackSpriteIndex(currentPercent);
                if (spriteIndex < feedbackSprites.Length && feedbackSprites[spriteIndex] != null)
                {
                    feedbackImage.sprite = feedbackSprites[spriteIndex];
                    Debug.Log($"[BatteryAnimator] Feedback image set to index {spriteIndex}");
                }
                else
                {
                    Debug.LogWarning($"[BatteryAnimator] Feedback sprite at index {spriteIndex} is not assigned!");
                }
            }
            else
            {
                Debug.LogWarning("[BatteryAnimator] feedbackImage or feedbackSprites is not assigned!");
            }
        }
        else
        {
            Debug.LogWarning("[BatteryAnimator] resultWindowCanvas is not assigned!");
        }
    }

    private void CloseResultWindow()
    {
        Debug.Log("[BatteryAnimator] Closing result window");
        if (resultWindowCanvas != null)
        {
            resultWindowCanvas.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[BatteryAnimator] resultWindowCanvas is not assigned!");
        }
    }

}
