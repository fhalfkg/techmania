﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoteDetailSideSheet : MonoBehaviour
{
    public GameObject noSelectionNotice;
    public GameObject contents;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeDisplay;
    public Slider panSlider;
    public TextMeshProUGUI panDisplay;
    public Button previewButton;
    public GameObject endOfScanOptions;
    public Toggle endOfScanToggle;
    public PatternPanel patternPanel;

    private HashSet<GameObject> selection;
    private List<Note> notes;

    private void OnEnable()
    {
        PatternPanel.SelectionChanged += OnSelectionChanged;
        OnSelectionChanged(patternPanel.selectedNoteObjects);
    }

    private void OnDisable()
    {
        PatternPanel.SelectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged(HashSet<GameObject> newSelection)
    {
        if (newSelection == null) return;
        if (newSelection.Count == 0)
        {
            noSelectionNotice.SetActive(true);
            contents.SetActive(false);
            return;
        }
        selection = newSelection;
        noSelectionNotice.SetActive(false);
        contents.SetActive(true);

        bool multiple = newSelection.Count > 1;
        notes = new List<Note>();
        foreach (GameObject o in newSelection)
        {
            notes.Add(o.GetComponent<NoteObject>().note);
        }

        if (!multiple)
        {
            volumeSlider.SetValueWithoutNotify(notes[0].volume * 100f);
            panSlider.SetValueWithoutNotify(notes[0].pan * 100f);
        }
        RefreshDisplays();
        previewButton.interactable = !multiple;

        bool allNotesOnScanDividers = true;
        int pulsesPerScan = Pattern.pulsesPerBeat *
            EditorContext.Pattern.patternMetadata.bps;
        foreach (Note n in notes)
        {
            if (n.pulse % pulsesPerScan != 0)
            {
                allNotesOnScanDividers = false;
                break;
            }
        }
        endOfScanOptions.SetActive(allNotesOnScanDividers);
        if (allNotesOnScanDividers)
        {
            endOfScanToggle.SetIsOnWithoutNotify(notes[0].endOfScan);
        }
    }

    // Assumes at least 1 note selected.
    private void RefreshDisplays()
    {
        bool sameValue = true;
        float volume = notes[0].volume;
        for (int i = 1; i < notes.Count; i++)
        {
            if (notes[i].volume != volume)
            {
                sameValue = false;
            }
        }
        if (sameValue)
        {
            volumeDisplay.text = Mathf.RoundToInt(volume * 100f) + "%";
        }
        else
        {
            volumeDisplay.text = "---";
        }

        sameValue = true;
        float pan = notes[0].pan;
        for (int i = 1; i < notes.Count; i++)
        {
            if (notes[i].pan != pan)
            {
                sameValue = false;
            }
        }
        if (sameValue)
        {
            panDisplay.text = Mathf.RoundToInt(pan * 100f) + "%";
        }
        else
        {
            panDisplay.text = "---";
        }
    }

    public void OnSliderValueChanged()
    {
        volumeDisplay.text = volumeSlider.value + "%";
        panDisplay.text = panSlider.value + "%";
    }

    public void OnVolumeSliderEndEdit(float newValue)
    {
        EditorContext.BeginTransaction();
        foreach (Note n in notes)
        {
            EditOperation op = EditorContext
                .BeginModifyNoteOperation();
            op.noteBeforeOp = n.Clone();
            n.volume = newValue * 0.01f;
            op.noteAfterOp = n.Clone();
        }
        EditorContext.EndTransaction();

        RefreshDisplays();
    }

    public void OnPanSliderEndEdit(float newValue)
    {
        EditorContext.BeginTransaction();
        foreach (Note n in notes)
        {
            EditOperation op = EditorContext
                .BeginModifyNoteOperation();
            op.noteBeforeOp = n.Clone();
            n.pan = newValue * 0.01f;
            op.noteAfterOp = n.Clone();
        }
        EditorContext.EndTransaction();

        RefreshDisplays();
    }

    public void OnPreviewButtonClick()
    {
        patternPanel.PlayKeysound(notes[0]);
    }

    public void OnEndOfScanToggleValueChanged(bool newValue)
    {
        EditorContext.BeginTransaction();
        foreach (Note n in notes)
        {
            EditOperation op = EditorContext
                .BeginModifyNoteOperation();
            op.noteBeforeOp = n.Clone();
            n.endOfScan = newValue;
            op.noteAfterOp = n.Clone();
        }
        EditorContext.EndTransaction();

        foreach (GameObject o in selection)
        {
            o.GetComponent<NoteInEditor>().UpdateEndOfScanIndicator();
        }
    }
}
