using UnityEngine;
using System.Collections;

public class DialogTrigger : MonoBehaviour
{
    private bool initialized = false; // initializing procedure, dunno what should be done here.. hmm?
    public string PlayerTag = "Player";
    public GameObject player { get; set; }

    public MeshRenderer HighlightTarget;
    private Color colorRimmed, colorUnrimmed;
    public float RimHilightValue = 255.0f;
    public bool triggerActive = false;

    // WEIRDNESS ACCORDING TO MADNESS
    public Dialog aDialog;

    public bool Init()
    {
        player = GameObject.FindWithTag("Player");
        if (!player)
        {
            Debug.LogError("Could not find player, tagged correctly?");
            DestroyImmediate(gameObject);
        }
        if (HighlightTarget)
        {
            colorRimmed = HighlightTarget.material.GetColor("_Rimlight_Color");
            colorRimmed.a = RimHilightValue;
            colorUnrimmed = HighlightTarget.material.GetColor("_Rimlight_Color");
        }
        return true;
    }

    public void Update()
    {
        if(!initialized) initialized = Init();
        if (HighlightTarget && triggerActive)
        {
            HighlightTarget.material.SetColor("_Rimlight_Color", colorRimmed);
        }
        else if (HighlightTarget)
        {
            HighlightTarget.material.SetColor("_Rimlight_Color", colorUnrimmed);
        }
    }

    public void OnTriggerEnter(Collider col)
    {
        if (col.gameObject == player)
        {
            if (aDialog) aDialog.Activate();
            triggerActive = true;
        }
    }

    public void OnTriggerExit(Collider col)
    {
        if (col.gameObject == player)
        {
            aDialog.Deactivate();
            triggerActive = false;
        }
    }
}