﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ResourcePanel : MonoBehaviour
{
    private static ResourcePanel instance;
    private static ResourcePanel GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<Canvas>().GetComponentInChildren<ResourcePanel>();
        }
        return instance;
    }

    public Text list;

    // These all contain full paths.
    private List<string> audioFiles;
    private List<string> imageFiles;
    private List<string> videoFiles;

    public static event UnityAction resourceRefreshed;

    public static List<string> GetAudioFiles()
    {
        return GetInstance().audioFiles;
    }
    public static List<string> GetImageFiles()
    {
        return GetInstance().imageFiles;
    }
    public static List<string> GetVideoFiles()
    {
        return GetInstance().videoFiles;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        string folder = new FileInfo(Navigation.GetCurrentTrackPath()).DirectoryName;

        audioFiles = new List<string>();
        imageFiles = new List<string>();
        videoFiles = new List<string>();
        string listText = "";

        foreach (string file in Directory.EnumerateFiles(folder, "*.wav"))
        {
            audioFiles.Add(file);
            listText += new FileInfo(file).Name + "\n";
        }
        foreach (string file in Directory.EnumerateFiles(folder, "*.png"))
        {
            imageFiles.Add(file);
            listText += new FileInfo(file).Name + "\n";
        }
        foreach (string file in Directory.EnumerateFiles(folder, "*.mp4"))
        {
            videoFiles.Add(file);
            listText += new FileInfo(file).Name + "\n";
        }

        list.text = listText.TrimEnd('\n');
        resourceRefreshed?.Invoke();
    }

    public void Import()
    {
        string folder = new FileInfo(Navigation.GetCurrentTrackPath()).DirectoryName;

        foreach (string file in SFB.StandaloneFileBrowser.OpenFilePanel(
            "Select resource to import", "", "wav;*.png;*.mp4", multiselect: true))
        {
            FileInfo fileInfo = new FileInfo(file);
            if (fileInfo.DirectoryName == folder) continue;

            try
            {
                File.Copy(file, $"{folder}\\{fileInfo.Name}", overwrite: true);
            }
            catch (Exception e)
            {
                MessageDialog.Show(e.Message);
                return;
            }
        }

        Refresh();
    }
}
