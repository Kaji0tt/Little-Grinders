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
        }
    }

    private void UnsubscribeFromEvent()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnEnemyWasAttacked -= ShowDamagePopup;
        }
    }

    public void ShowDamagePopup(float damage, Transform enemyTransform, bool crit)
    {
        // Lokale Offset-Position relativ zum Enemy
        //Vector3 localOffset = enemyTransform.GetComponent<EnemyController>().hpBar.transform.position;

        //Setze Slider als Parent
        Transform slideTransform = enemyTransform.GetComponent<EnemyController>().enemyHpSlider.transform;

        Debug.Log("Manager here:" + gameObject.name);
        // Instanziiere das Popup als Kind des Enemies
        GameObject popup = Instantiate(damagePopupPrefab);

        // Setze die lokale Position des Popups auf den Offset (z. B. hpBar Position)
        popup.transform.position = slideTransform.position;

        // Setze die lokale Position
        //popup.transform.localPosition = localOffset;
        popup.transform.localRotation = Quaternion.identity; // Optional: Falls du Rotation resetten willst
        popup.transform.localScale = Vector3.one;            // Optional: Falls das Prefab gestretched wirkt

        if(!crit)
            popup.GetComponent<DamagePopup>().Setup(damage);

        if(crit)
            popup.GetComponent<DamagePopup>().SetupCrit(damage);
    }

}