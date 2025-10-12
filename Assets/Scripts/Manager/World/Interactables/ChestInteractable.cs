using System.Collections;
using UnityEngine;

public class ChestInteractable : BaseInteractable
{
    [Header("Chest Sprites")]
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openedSprite;
    
    [Header("Item Drop Settings")]
    [SerializeField] private float dropRadius = 1f;
    [SerializeField] private float dropHeightOffset = 0.1f;
    
    [Header("Chest Settings")]
    [SerializeField] private bool startOpened = false;
    
    private SpriteRenderer spriteRenderer;
    private bool isOpened = false;
    
    protected override void Start()
    {
        // Standard-Einstellungen für Chest
        canBeUsedMultipleTimes = false; // Chest kann nur einmal geöffnet werden
        displayName = "Treasure Chest";
        interactPrompt = "Press [E] to open Chest";
        
        // Standard VFX/Audio Namen
        //if (string.IsNullOrEmpty(useSoundName))
        //    useSoundName = "ChestOpen";
        
        // Sprite Renderer finden
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInParent<SpriteRenderer>();
        }
        
        // Initialer Zustand setzen
        if (startOpened)
        {
            isOpened = true;
            SetOpenedSprite();
        }
        else
        {
            SetClosedSprite();
        }
        
        base.Start();
    }
    
    protected override bool CanInteract()
    {
        return base.CanInteract() && !isOpened;
    }
    
    protected override void OnInteract()
    {
        if (isOpened) return;
        
        OpenChest();
    }
    
    private void OpenChest()
    {
        isOpened = true;
        
        // Sprite zu geöffnet ändern
        SetOpenedSprite();
        
        // Item droppen
        DropGuaranteedItem();
        
        // Zustand speichern
        SaveState();
        
        Debug.Log("Chest opened and item dropped!");
    }
    
    private void SetClosedSprite()
    {
        if (spriteRenderer != null && closedSprite != null)
        {
            spriteRenderer.sprite = closedSprite;
        }
    }
    
    private void SetOpenedSprite()
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
    
    protected override string GetCustomSaveData()
    {
        return JsonUtility.ToJson(new ChestSaveData
        {
            isOpened = this.isOpened
        });
    }
    
    protected override void ApplyCustomSaveData(string data)
    {
        if (!string.IsNullOrEmpty(data))
        {
            try
            {
                var saveData = JsonUtility.FromJson<ChestSaveData>(data);
                
                this.isOpened = saveData.isOpened;
                
                // Sprite-Zustand wiederherstellen
                if (isOpened)
                {
                    SetOpenedSprite();
                }
                else
                {
                    SetClosedSprite();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to load Chest save data: {e.Message}");
            }
        }
    }
    
    [System.Serializable]
    private class ChestSaveData
    {
        public bool isOpened;
    }

    // Gizmo für den Drop-Radius im Editor anzeigen
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dropRadius);
    }
}
