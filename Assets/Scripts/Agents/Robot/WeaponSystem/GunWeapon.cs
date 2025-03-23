using UnityEngine;
[CreateAssetMenu(fileName = "New Gun",menuName = "Create a Gun", order = 0)]

public class GunWeapon : ScriptableObject
{
    public string WeaponName = "Regular Gun";

    [Header("Preset Gameobjects")]
    public GameObject Bullet;
    public GameObject MuzzleFlash;
    [Header("Weapon Settings")]
    public int MagazineSize = 25;
    public float TimeBetweenShots = 0.1f;
    public bool AllowHold = true;
    public float MuzzleFlashDestroyTime = 0.1f;
    public float ReloadTime = 2f;

    [Header("Camera Settings")]
    public AnimationCurve CameraShakeCurve;
    public float CameraShakeDuration = 0.1f;
    public float CameraShakeMagnitude = 0.2f;

    [Header("Bullet Settings")]
    public float BulletForwardAcceleration = 100f;
    public float BulletUpAcceleration = 100f;
    public float BulletDestroyTime = 5f;
    public float BulletSpread = 0.1f;


}
