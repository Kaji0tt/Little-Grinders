using UnityEngine;

public class Int_Chest : MonoBehaviour
{
    [Header("Chest Settings")]
    [SerializeField] private bool isOpened = false;
    [SerializeField] private Sprite openedSprite;
    
    [Header("Item Drop Settings")]
    [SerializeField] private float dropRadius = 1f;
    [SerializeField] private float dropHeightOffset = 0.1f;
    
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInParent<SpriteRenderer>();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Prüfe ob es der Player ist
        if (other == PlayerManager.instance.player.gameObject.GetComponentInChildren<Collider>() && !isOpened)
        {
            // Hole die Pick-Taste aus dem KeyManager
            //KeyCode pickKey = KeyManager.MyInstance.Keybinds["PICK"];
            
            if (Input.GetKeyDown(KeyCode.F))
            {
                OpenChest();
            }
        }
    }

    private void OpenChest()
    {
        if (isOpened) return;
        
        isOpened = true;
        
        // Sprite ändern wenn vorhanden
        ChangeToOpenedSprite();
        
        // Item droppen
        DropGuaranteedItem();
        
        Debug.Log("Chest opened and item dropped!");
    }

    private void ChangeToOpenedSprite()
    {
        if (spriteRenderer != null && openedSprite != null)
        {
            spriteRenderer.sprite = openedSprite;
        }
    }

    private void DropGuaranteedItem()
    {
        if (ItemDatabase.instance == null)
        {
            Debug.LogError("ItemDatabase instance is null!");
            return;
        }

        // Berechne Drop-Position in der Nähe der Chest
        Vector3 dropPosition = CalculateDropPosition();
        
        // Verwende die GetWeightDrop Methode der ItemDatabase um ein Item zu droppen
        ItemDatabase.instance.GetWeightDrop(dropPosition);
    }

    private Vector3 CalculateDropPosition()
    {
        Vector3 basePosition = transform.position;
        
        // Füge zufällige Offset hinzu für natürlicheres Dropping
        float randomX = Random.Range(-dropRadius, dropRadius);
        float randomZ = Random.Range(-dropRadius, dropRadius);
        
        return new Vector3(
            basePosition.x + randomX,
            basePosition.y + dropHeightOffset,
            basePosition.z + randomZ
        );
    }

    // Optional: Gizmo für den Drop-Radius im Editor anzeigen
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dropRadius);
    }
}
