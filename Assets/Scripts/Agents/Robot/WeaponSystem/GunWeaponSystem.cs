using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GunWeaponSystem : MonoBehaviour
{

    [HideInInspector] public int CurrentWeaponIndex = 0;
    [SerializeField] private Animator[] Animators;
    [Header("Weapons")]
    [SerializeField] private GunWeapon[] Weapons;
    [Header("Preset")]
    [SerializeField] private Transform Camera;
    [SerializeField] private Transform AttackPoint;
    [Header("Inputs")]
    [SerializeField] private InputActionReference FireAction;
    [Header("IK")]
    [SerializeField] private RigBuilder[] rigBuilders;
    [Header("UI Animation")]
    [SerializeField] private RawImage[] Images;
    [SerializeField] private AnimationCurve SizeTransitionCurve;
    [SerializeField] private float TransitionDuration = 0.5f;
    [SerializeField] private Vector2 ExpandedSize = new Vector2(150, 150);
    [SerializeField] private Vector2 NormalSize = new Vector2(100, 100);
    private Coroutine[] sizeCoroutines;
    private int ReloadingAnimation;
    private int SingleShotAnimation;
    private int MultiShotAnimations;
    private int FireRate;

    [HideInInspector]
    public int[] NumberOfBulletsLeft;
    float timer;
    bool SBHD = false;
    private CameraMovement cameraMovement;
    public PlayerMovement playerMovement;
    private void Awake()
    {
        //Initializing UI
        sizeCoroutines = new Coroutine[Images.Length];
     
        //Setting Bullets left to magazine size
        NumberOfBulletsLeft = new int[Weapons.Length];
        for (int i = 0; i < Weapons.Length; i++)
        {
            NumberOfBulletsLeft[i] = Weapons[i].MagazineSize;
        }

        cameraMovement = Camera.GetComponent<CameraMovement>();

        ReloadingAnimation = Animator.StringToHash("isReloading");
        SingleShotAnimation = Animator.StringToHash("SingleShot");
        MultiShotAnimations = Animator.StringToHash("HoldShot");
        FireRate = Animator.StringToHash("FireRate");
        SetCurrentWeaponIndex(0);
    }
    public void Update()
    {
        if (SBHD)
        {
            if (timer > Weapons[CurrentWeaponIndex].TimeBetweenShots)
            {
                if (NumberOfBulletsLeft[CurrentWeaponIndex] > 0)
                {
                    SetAnimatorBool(MultiShotAnimations, true);
                    float FireRateC = (1f / Weapons[CurrentWeaponIndex].TimeBetweenShots) / 20f;
                    SetAnimatorFloat(FireRate, FireRateC);
                    Shoot();
                    timer = 0f;
                }
                else
                {
                    Reload(new InputAction.CallbackContext());
                }
            }
        }
        else
        {
            SetAnimatorBool(MultiShotAnimations, false);
            SetAnimatorFloat(FireRate, 0f);
        }
        timer += Time.deltaTime;
    }
    public void ShootWithInput(InputAction.CallbackContext context)
    {
        if (Weapons[CurrentWeaponIndex].AllowHold)
        {

            if (context.performed)
            {
                SBHD = true;
            }
            else
            {
                SBHD = false;
            }
        }
        else
        {
            if (context.performed)
            {
                if (NumberOfBulletsLeft[CurrentWeaponIndex] > 0)
                {
                    StartCoroutine(SingleShot());
                    timer = 0f;
                }
                else
                {
                    if (playerMovement._isGrounded)
                    {

                        Reload(new InputAction.CallbackContext());
                    }
                }
            }
        }

    }
    public void STGG(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Gravity Gun");
            SetCurrentWeaponIndex(1);
        }
    }

    public void STRG(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Regular Gun");
            SetCurrentWeaponIndex(0);
        }
    }

    private void SetCurrentWeaponIndex(int index)
    {
        CurrentWeaponIndex = index;

        // Adjust sizes of the images
        for (int i = 0; i < Images.Length; i++)
        {
            if (sizeCoroutines[i] != null)
            {
                StopCoroutine(sizeCoroutines[i]);
            }
            sizeCoroutines[i] = StartCoroutine(AdjustImageSize(Images[i], i == CurrentWeaponIndex));
        }
    }
    private IEnumerator AdjustImageSize(RawImage image, bool expand)
    {
        RectTransform rectTransform = image.rectTransform;
        Vector2 targetSize = expand ? ExpandedSize : NormalSize;
        Vector2 initialSize = rectTransform.localScale;

        float elapsed = 0f;
        while (elapsed < TransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / TransitionDuration;
            float curveValue = SizeTransitionCurve.Evaluate(t);
            rectTransform.localScale = Vector2.Lerp(initialSize, targetSize, curveValue);
            yield return null;
        }
        rectTransform.localScale = targetSize;
    }


    public IEnumerator SingleShot()
    {
        SetAnimatorBool(SingleShotAnimation, true);
        Shoot();
        yield return new WaitForSeconds(1f);
        SetAnimatorBool(SingleShotAnimation, false);
    }

    public void Reload(InputAction.CallbackContext context)
    {
        if (!playerMovement._isGrounded) return;
        // Start the coroutine to reload the weapon
        StartCoroutine(ReloadWeapon());
    }

    public IEnumerator ReloadWeapon()
    {
        playerMovement.AllowMovement = false;
        //Turning off IK
        foreach (RigBuilder rb in rigBuilders)
        {
            rb.layers[0].active = false;
        }

        //Animations
        SetAnimatorBool(ReloadingAnimation, true);

        // Wait for the reload time of the current weapon
        yield return new WaitForSeconds(Weapons[CurrentWeaponIndex].ReloadTime);

        // Reset bullets to the weapon's magazine size
        NumberOfBulletsLeft[CurrentWeaponIndex] = Weapons[CurrentWeaponIndex].MagazineSize;

        //Animations
        SetAnimatorBool(ReloadingAnimation, false);

        //Turning on IK
        foreach (RigBuilder rb in rigBuilders)
        {
            rb.layers[0].active = true;
        }
        playerMovement.AllowMovement = true;
    }

    public void Shoot()
    {

        //Getting Weapon

        GunWeapon weapon = Weapons[CurrentWeaponIndex];

        //Set number of bullets
        NumberOfBulletsLeft[CurrentWeaponIndex] -= 1;

        //Camera Shake
        cameraMovement.StartCameraShake(weapon.CameraShakeDuration, weapon.CameraShakeMagnitude, weapon.CameraShakeCurve);

        //Spawn the Bullet & Muzzle Flash
        GameObject Bullet = Instantiate(weapon.Bullet, AttackPoint.position, AttackPoint.rotation);
        GameObject MuzzleFlash = Instantiate(weapon.MuzzleFlash, AttackPoint.position, AttackPoint.rotation);
        //Apply Force if the rigidbody available

        Rigidbody bulletRigidbody = Bullet.GetComponent<Rigidbody>();
        bulletRigidbody.AddForce(Camera.forward * weapon.BulletForwardAcceleration * bulletRigidbody.mass, ForceMode.Impulse);
        bulletRigidbody.AddForce(Vector3.up * weapon.BulletUpAcceleration * bulletRigidbody.mass, ForceMode.Impulse);

        //Destory Muzzle Flash Objects
        Destroy(Bullet, weapon.BulletDestroyTime);
        Destroy(MuzzleFlash, weapon.MuzzleFlashDestroyTime);

    }

    private void SetAnimatorBool(int parameter, bool value)
    {
        if (Animators.Length > CurrentWeaponIndex && Animators[CurrentWeaponIndex] != null)
        {
            Animators[CurrentWeaponIndex].SetBool(parameter, value);
        }
    }

    private void SetAnimatorFloat(int parameter, float value)
    {
        if (Animators.Length > CurrentWeaponIndex && Animators[CurrentWeaponIndex] != null)
        {
            Animators[CurrentWeaponIndex].SetFloat(parameter, value);
        }
    }
}
