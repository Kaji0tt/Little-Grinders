using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LogScript : MonoBehaviour
{
    public static LogScript instance;

    private Text text;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        text = GetComponent<Text>();
        text.text = "";
        text.canvasRenderer.SetAlpha(0f);
    }

    public void ShowLog(string logtxt, float duration = -1f)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        if (duration <= 0f)
            duration = Mathf.Clamp(logtxt.Length * 0.1f, 3f, 8f); // Dynamische Dauer

        fadeCoroutine = StartCoroutine(ShowLogRoutine(logtxt, duration));
    }

    private IEnumerator ShowLogRoutine(string logtxt, float duration)
    {
        if (text == null)
        {
            text = GetComponent<Text>();
        }
        text.text = logtxt;

        text.canvasRenderer.SetAlpha(0f);              // Unsichtbar starten
        text.CrossFadeAlpha(1f, 0.5f, false);          // Sanftes Einfaden

        yield return new WaitForSeconds(duration);

        text.CrossFadeAlpha(0f, 2f, false);            // Langsam ausfaden
    }
}
