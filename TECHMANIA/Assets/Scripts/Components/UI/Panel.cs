﻿using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class Panel : MonoBehaviour
{
    public static Panel current;
    public Selectable defaultSelectable;
    public TransitionToPanel backButton;
    private GameObject selectedBeforeDisable;

    // Start is called before the first frame update
    void Start()
    {
        if (current == null) current = this;
    }

    private void OnEnable()
    {
        if (selectedBeforeDisable != null)
        {
            EventSystem.current.SetSelectedGameObject(selectedBeforeDisable);
        }
        else if (defaultSelectable != null)
        {
            EventSystem.current.SetSelectedGameObject(defaultSelectable.gameObject);
        }
    }

    private void OnDisable()
    {
        selectedBeforeDisable = EventSystem.current.currentSelectedGameObject;
    }

    private void Update()
    {

    }
}