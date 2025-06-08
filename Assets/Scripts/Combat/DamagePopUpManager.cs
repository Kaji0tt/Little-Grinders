using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance;

    [SerializeField] private GameObject damagePopupPrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        GameEvents.Instance.OnEnemyWasAttacked += ShowDamagePopup;
    }

    private void OnDisable()
    {
        if (GameEvents.Instance != null)
            GameEvents.Instance.OnEnemyWasAttacked -= ShowDamagePopup;
    }

    public void ShowDamagePopup(float damage, Transform enemyTransform)
    {
        // Lokale Offset-Position relativ zum Enemy
        //Vector3 localOffset = enemyTransform.GetComponent<EnemyController>().hpBar.transform.position;

        //Setze Slider als Parent
        Transform slideTransform = enemyTransform.GetComponent<EnemyController>().enemyHpSlider.transform;

        // Instanziiere das Popup als Kind des Enemies
        GameObject popup = Instantiate(damagePopupPrefab, slideTransform);

        // Setze die lokale Position des Popups auf den Offset (z. B. hpBar Position)
        popup.transform.localPosition = slideTransform.position;

        // Setze die lokale Position
        //popup.transform.localPosition = localOffset;
        popup.transform.localRotation = Quaternion.identity; // Optional: Falls du Rotation resetten willst
        popup.transform.localScale = Vector3.one;            // Optional: Falls das Prefab gestretched wirkt

        // Setup
        popup.GetComponent<DamagePopup>().Setup(damage);
    }

}