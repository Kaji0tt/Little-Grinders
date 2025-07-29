using UnityEngine;
using UnityEngine.SceneManagement;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance;

    [SerializeField] private GameObject damagePopupPrefab;

    private void Awake()
    {
        // Überprüfen, ob eine Instanz bereits existiert
        if (Instance == null)
        {
            Instance = this; // Setze die aktuelle Instanz
            DontDestroyOnLoad(gameObject); // Optional: Behalte dieses Objekt bei Szenenwechseln
        }
        else if (Instance != this)
        {
            // Falls eine andere Instanz bereits existiert, zerstöre diese neue Instanz
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeFromEvent();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UnsubscribeFromEvent();

        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnEnemyWasAttacked += ShowDamagePopup;
            GameEvents.Instance.OnEnemyTookDirectDamage += ShowDirectDamagePopup; // NEU
        }
    }

    private void UnsubscribeFromEvent()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnEnemyWasAttacked -= ShowDamagePopup;
            GameEvents.Instance.OnEnemyTookDirectDamage -= ShowDirectDamagePopup; // NEU
        }
    }

    public void ShowDamagePopup(float damage, Transform enemyTransform, bool crit)
    {
        // Lokale Offset-Position relativ zum Enemy
        //Vector3 localOffset = enemyTransform.GetComponent<EnemyController>().hpBar.transform.position;

        //Setze Slider als Parent
        Transform slideTransform = enemyTransform.GetComponent<EnemyController>().enemyHpSlider.transform;

        //Debug.Log("Manager here:" + gameObject.name);
        // Instanziiere das Popup als Kind des Enemies
        GameObject popup = Instantiate(damagePopupPrefab);

        // Setze die lokale Position des Popups auf den Offset (z. B. hpBar Position)
        Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0f, 0.3f), 0);
        popup.transform.position = slideTransform.position + randomOffset;

        // Setze die lokale Position
        //popup.transform.localPosition = localOffset;
        popup.transform.localRotation = Quaternion.identity; // Optional: Falls du Rotation resetten willst
        popup.transform.localScale = Vector3.one;            // Optional: Falls das Prefab gestretched wirkt

        if (!crit)
            popup.GetComponent<DamagePopup>().Setup(damage);

        if (crit)
            popup.GetComponent<DamagePopup>().SetupCrit(damage);
    }
    
    // NEU: Methode für direkten Schaden
    public void ShowDirectDamagePopup(float damage, EnemyController enemyC, bool isCrit)
    {
        Transform slideTransform = enemyC.enemyHpSlider.transform;
        GameObject popup = Instantiate(damagePopupPrefab);
        Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0f, 0.3f), 0);
        popup.transform.position = slideTransform.position + randomOffset;
        popup.transform.localRotation = Quaternion.identity;
        popup.transform.localScale = Vector3.one;

        popup.GetComponent<DamagePopup>().SetupDirect(damage); // NEU
    }

}