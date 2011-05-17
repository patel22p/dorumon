using UnityEngine;
using System.Collections;

[System.Serializable]
public class DialogText
{
    public void ResetDialog()
    {
        Timer = TimeBeforeChange;
    }

    public float TimeBeforeChange;
    public float Timer { get; set; }
    public string Text;   
}

public class Dialog : MonoBehaviour
{
    public DialogText[] dialog;
    public DialogText currentDialog { get; set; }
    private int dialogStep = 0;
    public bool isActive = false;

    private string StartText = "Press (Mouse 1) to talk.";
    private bool started = false;
    private TextMesh txtMsh;
    public Transform TextTargetPosition;
    public float heightOffset;

    public void Start()
    {
        renderer.enabled = false;
        txtMsh = gameObject.GetComponent(typeof(TextMesh)) as TextMesh;
        if (!txtMsh) Destroy(this.gameObject);
        if (dialog.Length <= 0)
        {
            isActive = false;

        }
        else
        {
            currentDialog = dialog[0];
        }
    }

    public void Update()
    {
        if (isActive)
        {
            if(TextTargetPosition) transform.position = new Vector3(TextTargetPosition.position.x, TextTargetPosition.position.y + heightOffset, TextTargetPosition.position.z);
            transform.LookAt(transform.position + (transform.position - Camera.mainCamera.transform.position), Vector3.up);
            if (!started)
            {
                txtMsh.text = StartText;
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    started = true;
                    txtMsh.text = dialog[dialogStep].Text;
                }
            }
            else
            {
                if (currentDialog.Timer < 0)
                {
                    currentDialog.ResetDialog();
                    if (dialogStep < dialog.Length - 1)
                    {
                        dialogStep++;
                        currentDialog = dialog[dialogStep];
                        txtMsh.text = dialog[dialogStep].Text;
                    }
                }
                else
                {
                    if(Input.GetKeyDown(KeyCode.Mouse0) && dialogStep == dialog.Length - 1)
                    {
                        Deactivate();
                        Activate();
                    }
                    currentDialog.Timer -= Time.deltaTime;
                }
            }
        }
    }

    public void Activate()
    {
        renderer.enabled = true;
        currentDialog = dialog[0];
        currentDialog.ResetDialog();
        isActive = true;
    }

    public void Deactivate()
    {
        started = false;
        isActive = false;
        currentDialog.ResetDialog();
        dialogStep = 0;
        txtMsh.text = StartText;
        renderer.enabled = false;
    }
}