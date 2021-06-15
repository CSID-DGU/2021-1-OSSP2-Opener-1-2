using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LibaryHandler : MonoBehaviour
{
    public static ProfileIconHandler instance;

    public Transform titleContent;
    public GameObject title;

    public void Start()
    {
        string path = @"C:/welvi/Map";

        if (System.IO.Directory.Exists(path))
        {
            System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(path);

            foreach (var item in directoryInfo.GetDirectories())
            {
                GameObject titleElement = Instantiate(title, titleContent);
                titleElement.GetComponentInChildren<TMP_Text>().text = item.Name;
            }
        }
    }

}

