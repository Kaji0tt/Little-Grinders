using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    #region Singleton
    public static Tooltip instance;
    private void Awake()
    {
        Tooltip[] sceneInstances = FindObjectsOfType<Tooltip>();
        if (sceneInstances.Length >= 2)
        {
            Destroy(sceneInstances[0]);
        }
        instance = this;
    }
    #endregion

    public Canvas canvas;
    private RectTransform rect;
    private Text tooltipText;
    
    [Header("Positioning")]
    [SerializeField] private float offsetX = 15f;
    [SerializeField] private float offsetY = 15f;
    [SerializeField] private float edgeMargin = 20f;
    
    private void Start()
    {
        rect = GetComponent<RectTransform>();
        tooltipText = GetComponentInChildren<Text>();
        
        // Ensure Tooltip starts hidden
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }

    public void Update()
    {
        if (!gameObject.activeSelf) return;
        PositionTooltip();
    }

    private void PositionTooltip()
    {
        // Convert mouse position to canvas space
        Vector2 mousePos = Input.mousePosition;
        Vector2 tooltipSize = rect.sizeDelta;
        
        // Alternative size calculation for ContentSizeFitter compatibility
        Vector2 alternativeSize = new Vector2(rect.rect.width, rect.rect.height);
        
        // Use positive size if sizeDelta is invalid
        if (tooltipSize.x <= 0 || tooltipSize.y <= 0)
        {
            tooltipSize = alternativeSize;
        }
        
        // Canvas boundaries for Screen Space Overlay
        float canvasWidth = Screen.width;
        float canvasHeight = Screen.height;
        
        // Default position (right of cursor)
        rect.pivot = new Vector2(0, 1);
        Vector2 position = mousePos + new Vector2(offsetX, offsetY);
        
        // Check if tooltip would go off right edge
        if (position.x + tooltipSize.x > canvasWidth - edgeMargin)
        {
            // Position to the left of cursor
            rect.pivot = new Vector2(1, 1);
            position.x = mousePos.x - offsetX;
        }
        
        // Check if tooltip would go off bottom edge
        if (position.y - tooltipSize.y < edgeMargin)
        {
            // Adjust Y pivot to show above cursor
            rect.pivot = new Vector2(rect.pivot.x, 0);
            position.y = mousePos.y + offsetY;
        }
        
        // Check if tooltip would go off top edge
        if (position.y > canvasHeight - edgeMargin)
        {
            // Adjust Y pivot to show below cursor
            rect.pivot = new Vector2(rect.pivot.x, 1);
            position.y = mousePos.y - offsetY;
        }
        
        // Apply position
        transform.position = new Vector3(position.x, position.y, 0);
    }

    public void SetText(string newText)
    {
        tooltipText = GetComponentInChildren<Text>();
        tooltipText.text = newText;
    }
}
