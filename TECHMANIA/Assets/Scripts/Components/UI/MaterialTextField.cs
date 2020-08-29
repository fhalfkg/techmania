﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MaterialTextField : MonoBehaviour
{
    // These stop BackButtons from responding to the Cancel key.
    public static bool editingAnyTextField;
    // Events are processed earlier than components. Without this hack,
    // when user presses cancel while editing a text field, the text
    // field will set editingAnyTextField=false, and then BackButton
    // will think the user is not editing any text field, and respond
    // to the cancel key.
    //
    // With this hack, BackButton can realize that a text field ended
    // its editing on the same frame user pressed cancel, and thus
    // will not respond to the cancel key.
    public static int frameOfLastEndEdit;

    public Color miniLabelColor;
    public Color labelColor;
    public Color inputTextColor;
    public Color disabledColor;

    public GameObject miniLabelObject;
    public TextMeshProUGUI miniLabel;
    public TextMeshProUGUI label;
    public TextMeshProUGUI inputText;

    private TMP_InputField text;
    private bool interactable;
    private bool emptyText;

    static MaterialTextField()
    {
        editingAnyTextField = false;
        frameOfLastEndEdit = -1;
    }

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMP_InputField>();
        interactable = true;
        emptyText = true;
        miniLabelObject.SetActive(false);

        text.onSelect.AddListener((string s) =>
        {
            // Editing automatically starts when a text field gets focus.
            editingAnyTextField = true;
        });
        text.onEndEdit.AddListener((string s) =>
        {
            editingAnyTextField = false;
            frameOfLastEndEdit = Time.frameCount;
        });
    }

    // Update is called once per frame
    void Update()
    {
        bool newInteractable = text.IsInteractable();
        if (newInteractable != interactable)
        {
            miniLabel.color = newInteractable ? miniLabelColor :
                disabledColor;
            label.color = newInteractable ? labelColor :
                disabledColor;
            inputText.color = newInteractable ? inputTextColor :
                disabledColor;
        }
        interactable = newInteractable;
    }

    public void OnValueChanged()
    {
        bool newEmptyText = text.text == "";
        if (newEmptyText != emptyText)
        {
            miniLabel.text = label.text;
            miniLabelObject.SetActive(!newEmptyText);
        }
        emptyText = newEmptyText;
    }
}