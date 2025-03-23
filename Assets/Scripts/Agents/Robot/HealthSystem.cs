using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using UnityEngine.Rendering;

[System.Serializable]
public class HealthEvent : UnityEvent<float> { }

public class HealthSystem : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color zeroHealthColor = Color.red;
    [SerializeField] private bool NoNeedForUI = false;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float healingFactor = 1.0f;
    [SerializeField] private float healingCooldown = 2f;
    [SerializeField] private bool invincible = false;
    [Header("PlayerFallDamage")]
    [SerializeField] private bool ApplyFallDamage = false;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private float MinDamageVelocity = 4f;
    [SerializeField] private float ScalingFactor = 1.0f;
    [Header("DamageEffects")]
    [SerializeField] private Volume DamageVolume;
    [Header("Events")]
    public HealthEvent OnDamageTaken;
    public HealthEvent OnHeal;
    public UnityEvent OnDeath;

    private float _currentHealth;
    private float _healingCooldownTimer;
    private bool _isDead;
    private float DamageAmount;
    private void Start()
    {
        InitializeHealth();
    }

    private void InitializeHealth()
    {
        _currentHealth = maxHealth;
        if (NoNeedForUI) return;
        UpdateHealthUI();

        if (healthFillImage)
        {
            healthFillImage.color = fullHealthColor;
        }
    }

    private void Update()
    {


        if (_isDead) return;
        if(DamageVolume != null) DamageVolume.weight = (maxHealth - CurrentHealth)/maxHealth;
        HandleHealing();
        if (NoNeedForUI) return;
        UpdateHealthUI();
        if (!ApplyFallDamage) return;
        if (movement._isGrounded)
        {
            TakeDamage(DamageAmount);
            DamageAmount = 0;
        }
        else
        {
            CalculateApplyFallDamage();
        }
    }
    private void CalculateApplyFallDamage()
    {
        if(rb.linearVelocity.y < 0f)
        {
            if (rb.linearVelocity.y > -MinDamageVelocity) return;
            DamageAmount += -rb.linearVelocity.y * ScalingFactor;
        }
    }
    private void HandleHealing()
    {
        if (_healingCooldownTimer > 0)
        {
            _healingCooldownTimer -= Time.deltaTime;
            return;
        }

        if (_currentHealth < maxHealth)
        {
            Heal(healingFactor * Time.deltaTime);
        }
    }

    public void TakeDamage(float damage)
    {
        if (_isDead || invincible) return;

        _currentHealth = Mathf.Clamp(_currentHealth - damage, 0, maxHealth);
        _healingCooldownTimer = healingCooldown;

        OnDamageTaken?.Invoke(damage);
        if (_currentHealth <= 0)
        {
            Die();
        }

        if (NoNeedForUI) return;
        UpdateHealthUI();
    }

    public void Heal(float amount)
    {
        if (_isDead) return;

        _currentHealth = Mathf.Clamp(_currentHealth + amount, 0, maxHealth);
        OnHeal?.Invoke(amount);
        if (NoNeedForUI) return;
        UpdateHealthUI();
    }

    private void Die()
    {
        _isDead = true;
        OnDeath?.Invoke();
        // Add death behavior here (e.g., disable components, play animations)
    }

    private void UpdateHealthUI()
    {
        if (!healthSlider) return;

        healthSlider.value = _currentHealth;

        if (healthFillImage)
        {
            healthFillImage.color = Color.Lerp(zeroHealthColor, fullHealthColor, _currentHealth / maxHealth);
        }
    }

    public void SetInvincible(bool state)
    {
        invincible = state;
    }

    public void ResetHealth()
    {
        _isDead = false;
        _currentHealth = maxHealth;
        _healingCooldownTimer = 0;
        UpdateHealthUI();
    }

    // Public accessors
    public float CurrentHealth => _currentHealth;
    public float HealthPercentage => _currentHealth / maxHealth;
    public bool IsDead => _isDead;
#if UNITY_EDITOR
    // Custom Editor logic to add a button in the Unity Inspector
    [CustomEditor(typeof(HealthSystem))]
    public class HealthSystemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            HealthSystem healthSystem = (HealthSystem)target;
            
            // Add a button to take damage in the editor
            if (GUILayout.Button("Take Damage"))
            {
                healthSystem.TakeDamage(10);  // Call the TakeDamageEditor method
            }
        }
    }
#endif

}