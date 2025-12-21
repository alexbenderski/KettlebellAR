using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RobotPhysicsController : MonoBehaviour
{
    [Header("Robot Settings")]
    public Transform robot;
    public Transform lowPosition;
    public Transform highPosition;
    public float mass = 5f;         // ק"ג, אפשר לשנות
    public float gravity = 9.8f;    // m/s^2

    [Header("UI Elements")]
    public Slider heightSlider;
    public TMP_Text heightText;
    public BatteryAnimator batteryAnimator; // הקוד שלך

    private void Start()
    {
        // Listener לשינוי בסליידר
        heightSlider.onValueChanged.AddListener(OnHeightChanged);

        // אפס התחלה
        OnHeightChanged(heightSlider.value);
    }

    public void OnHeightChanged(float value)
    {
        // עדכון טקסט
        heightText.text = "Height: " + value.ToString("F2") + " m";

        // חישוב אנרגיה פוטנציאלית
        float potentialEnergy = mass * gravity * value; // Joules

        // המרה ל-% לצורך הסוללה
        float percent = Mathf.Clamp((potentialEnergy / MaxEnergy()) * 100f, 0f, 100f);

        // עדכון גרף + glow
        batteryAnimator.SetInstant(percent);

        // תזוזת רובוט
        robot.position = Vector3.Lerp(lowPosition.position, highPosition.position, value / heightSlider.maxValue);
    }

    private float MaxEnergy()
    {
        // אנרגיה פוטנציאלית מקסימלית בהתאם לגובה מקסימלי
        return mass * gravity * heightSlider.maxValue;
    }
}
