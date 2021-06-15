using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DropdownHandler : MonoBehaviour
{
    public static DropdownHandler instance;

    public TMP_Dropdown dropdown;
    public TMP_Text gamename;

    public static string dirPath;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            UnityEngine.Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
        string pathPrefix = System.Environment.SpecialFolder.Desktop.ToString();

        string os = SystemInfo.operatingSystem;
        if (os.StartsWith("Windows"))
        {
            dirPath = @"C:/Welvi/Map";
        }
        if (os.StartsWith("Mac"))
        {
            dirPath = @"/Users/apple/Desktop/Welvi/Map";
        }

    }

    public void Start()
    {
        dropdown.options.Clear();
        List<string> gameNames = new List<string>();
        List<TMP_Dropdown.OptionData> optionList = new List<TMP_Dropdown.OptionData>();

        if (System.IO.Directory.Exists(dirPath))
        {
            System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(dirPath);

            foreach (var item in directoryInfo.GetDirectories())
            {
                gameNames.Add(item.Name);
            }

            foreach (string item in gameNames)
            {
                optionList.Add(new TMP_Dropdown.OptionData(item));
            }

            dropdown.AddOptions(optionList);
            dropdown.value = 0;

            gamename.text = gameNames[0];

            dropdown.onValueChanged.AddListener(delegate
            {
                DropdownSelected(dropdown);
            });
        }
    }

    void DropdownSelected(TMP_Dropdown dropdown)
    {
        int index = dropdown.value;
        gamename.text = dropdown.options[index].text;
        if (gamename.text == "")
        {
            gamename.text = "Select a Theme..";
        }
    }

    public string GetDirPath()
    {
        return dirPath;
    }
}

