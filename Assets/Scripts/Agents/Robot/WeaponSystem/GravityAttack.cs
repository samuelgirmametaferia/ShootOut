using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


public class GravityAttack : MonoBehaviour
{
    [SerializeField] private GameObject GravitySpherePreset;
    [SerializeField] private LayerMask Interactable;
    [SerializeField] private float EffectTime = 5f;
    [SerializeField] private float SphereExpansionTime = 2f;
    [SerializeField] private float ExtraRadius;
    [SerializeField] private AnimationCurve EasingCurve;

    private GameObject GravitySphere;


    void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & Interactable) != 0)
        {
            Destroy(GetComponent<MeshRenderer>());
            Destroy(GetComponent<MeshFilter>());
            Destroy(GetComponent<SphereCollider>());
            Destroy(GetComponent<Rigidbody>());
            StartCoroutine(GravityAttackEffect(collision));
        }
    }

    IEnumerator GravityAttackEffect(Collision collision)
    {
        GameObject Object = collision.gameObject;

        // Ensure the object is non-static
        if (Object.isStatic)
        {
            Object.isStatic = false;
        }

        float TargetRadius;
        Vector3 center;

        if (Object.TryGetComponent(out MeshRenderer meshRenderer))
        {
            center = meshRenderer.bounds.center;
            Vector3 maxPoint = meshRenderer.bounds.max;
            TargetRadius = (maxPoint - center).magnitude + ExtraRadius;
        }
        else
        {
            center = collision.collider.bounds.center;
            Vector3 maxPoint = collision.collider.bounds.max;
            TargetRadius = (maxPoint - center).magnitude + ExtraRadius;
        }
        Debug.Log(TargetRadius);

        GravitySphere = Instantiate(GravitySpherePreset, center, Quaternion.identity);

        // Expand the sphere

        StartCoroutine(ExpandToSphereRadius(GravitySphere, TargetRadius, SphereExpansionTime));
        Rigidbody objectB = Object.GetComponent<Rigidbody>();

        if (objectB == null)
        {
            objectB = Object.AddComponent<Rigidbody>();
        }

        objectB.useGravity = false;

        yield return new WaitForSeconds(EffectTime);

        objectB.useGravity = true;

        // Shrink the sphere
        StartCoroutine(ExpandToSphereRadius(GravitySphere, 0f, SphereExpansionTime));

        // Destroy the sphere after shrinking
        Destroy(GravitySphere, SphereExpansionTime + EffectTime);
    }

    IEnumerator ExpandToSphereRadius(GameObject sphere, float targetRadius, float expandTime)
    {
        Debug.Log(":> Fun! It is running 0");
        float timer = 0f;
        float initialRadius = sphere.transform.localScale.x;

        while (timer < expandTime)
        {
            Debug.Log(":> Fun! It is running 1");
            float t = timer / expandTime;
            float easedT = EasingCurve.Evaluate(t); // Apply easing function
            float newRadius = Mathf.Lerp(initialRadius, targetRadius, easedT);
            sphere.transform.localScale = new Vector3(newRadius, newRadius, newRadius);

            timer += Time.deltaTime;
            yield return null;
        }
        Debug.Log(":> Fun! It is running 2");
        sphere.transform.localScale = new Vector3(targetRadius, targetRadius, targetRadius);
    }
}
