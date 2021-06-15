using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ProfileIconHandler : MonoBehaviour
{
    public static ProfileIconHandler instance;

    public RawImage userIcon;
    public RawImage iconImage;
    public RawImage profileImage;
    public Texture[] texture = new Texture[6];
   
    public int currentNum = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void profileIcon()
    {
        FirebaseManager.instance.GetIconNum();
        FirebaseManager.instance.editNickNameField.text = "";
    }

    public void nextButton()
    {
        currentNum++;
        if(currentNum == 6) currentNum = 0;
        iconImage.texture = texture[currentNum];
    }

    public void previousButton()
    {
        currentNum--;
        if (currentNum == -1) currentNum = 5;
        iconImage.texture = texture[currentNum];
    }

    public void EditIcon()
    {
        FirebaseManager.instance.ChangeIcon(currentNum);
        setIcon(currentNum);
    }

    public void setIcon(int num)
    {
        userIcon.texture = texture[num];
        profileImage.texture = texture[num];
        iconImage.texture = texture[num];
    }

}

