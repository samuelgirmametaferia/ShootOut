using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine;
public class CameraMovement : MonoBehaviour
{
    public InputActionReference Look;
    [Header("Settings")]
    public Transform PlayerBody; // Reference to the player's body
    public Transform LockPointWeight; // Target position for the camera

    public float Sensitivity
    {
        get { return sensitivity; }
        set { sensitivity = value; }
    }

    [Range(0f, 1000f)][SerializeField] private float sensitivity = 2f;
    [Tooltip("Limits vertical camera rotation. Prevents the flipping that happens when rotation goes above 90.")]
    [Range(0f, 90f)][SerializeField] private float yRotationLimit = 88f;

  
    [SerializeField] private float lerpSpeed = 5f; // Speed of position interpolation
    [SerializeField] private AnimationCurve DamageCurve;

    public Transform MainCamera;
    public Vector2 rotation = Vector2.zero;

    public float turnValue = 0.5f;

    [HideInInspector] public Vector2 mouseDelta;

    private Vector3 defaultCameraOffset;


    private Coroutine currentShakeCoroutine;
 
    void Start()
    {
        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Store the initial offset between the camera and LockPointWeight
        defaultCameraOffset = MainCamera.localPosition - LockPointWeight.localPosition;
    }
 
    void Update()
    {
        HandleMouseInput();
        HandleCameraRotation();
        HandleCameraPosition();
    }

    private void HandleMouseInput()
    {
        // Read mouse input
        mouseDelta = Look.action.ReadValue<Vector2>();

        if (mouseDelta.x == 0)
        {
            turnValue = 0.5f;
        }
        else if (mouseDelta.x < 0)
        {
            turnValue = 0f;
        }
        else if (mouseDelta.x > 0)
        {
            turnValue = 1f;
        }
    }

    private void HandleCameraRotation()
    {
        // Adjust input based on sensitivity and frame time
        float mouseX = mouseDelta.x * Time.deltaTime;
        float mouseY = mouseDelta.y * Time.deltaTime;

        rotation.x += mouseX * sensitivity;
        rotation.y += mouseY * sensitivity;
        rotation.y = Mathf.Clamp(rotation.y, -yRotationLimit, yRotationLimit);

        var xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
        var yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);

        transform.localRotation = xQuat * yQuat;

        // Rotate the player body horizontally (yaw)
        PlayerBody.rotation = Quaternion.Euler(0f, transform.localRotation.eulerAngles.y, 0f);
    }

    private void HandleCameraPosition()
    {
        // Calculate the desired camera position
        Vector3 desiredCameraPosition = LockPointWeight.localPosition + defaultCameraOffset;

        

        // Smoothly move the camera to the desired position
        MainCamera.localPosition = Vector3.Lerp(MainCamera.localPosition, desiredCameraPosition, Time.deltaTime * lerpSpeed);
    }
    public void StartCameraShake(float duration, float magnitude,AnimationCurve ShakeCurve)
    {
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
        }

        currentShakeCoroutine = StartCoroutine(ShakeCamera(duration, magnitude, ShakeCurve));
    }
    public void StartCameraHealth(float damage)
    {
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
        }

        currentShakeCoroutine = StartCoroutine(ShakeCamera(damage/10f, damage*0.1f, DamageCurve));
    }
    private IEnumerator ShakeCamera(float duration, float magnitude,AnimationCurve ShakeCurve)
    {
        float elapsed = 0f;
        Vector3 originalPosition = MainCamera.localPosition;

        while (elapsed < duration)
        {
            // Evaluate the curve to adjust shake intensity over time
            float curveValue = ShakeCurve.Evaluate(elapsed / duration);

            // Smoothly interpolate the offset
            float xOffset = Mathf.PerlinNoise(elapsed * 10f, 0f) * 2f - 1f; // Perlin noise for smooth randomness
            float yOffset = Mathf.PerlinNoise(0f, elapsed * 10f) * 2f - 1f;
            float zOffset = Mathf.PerlinNoise(elapsed * 10f, elapsed * 10f) * 2f - 1f;

            xOffset *= magnitude * curveValue;
            yOffset *= magnitude * curveValue;
            zOffset *= magnitude * curveValue;

            // Apply the offset to the camera position
            MainCamera.localPosition = originalPosition + new Vector3(xOffset, yOffset, zOffset);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset the camera to its original position
        MainCamera.localPosition = originalPosition;
    }
   
}
