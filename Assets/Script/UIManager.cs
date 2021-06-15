using Photon.Pun;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Login")]
    public GameObject LoginCanvas;
    public GameObject LoginUI;
    public GameObject RegisterUI;
    public GameObject WelcomeUI;

    [Header("Main")]
    public GameObject MainCanvas;
    public GameObject MainUI;
    public GameObject RoomListUI;
    public GameObject LibaryUI;
    public GameObject SettingRoomUI;
    public GameObject RoomUI;
    public GameObject FriendUI;
    public GameObject InGameUI;

    [Header("Profile")]
    public GameObject ProfileCanvas;
    public GameObject ProfileUI;
    public GameObject EditUI;

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

    public void ClearScreenInLogin()
    {
        LoginUI.SetActive(false);
        RegisterUI.SetActive(false);
        WelcomeUI.SetActive(false);
    }

    public void ClearScreenInMain()
    {
        MainUI.SetActive(false);
        RoomListUI.SetActive(false);
        LibaryUI.SetActive(false);
    }

    public void ClearScreenInRoom()
    {
        RoomListUI.SetActive(false);
        SettingRoomUI.SetActive(false);
        RoomUI.SetActive(false);
    }

    public void ClearCanvas()
    {
        LoginCanvas.SetActive(false);
        MainCanvas.SetActive(false);
        ProfileCanvas.SetActive(false);
    }

    #region Canvas
    public void MainCanvasScreen()
    {
        ClearCanvas();
        MainCanvas.SetActive(true);
        PhotonNetwork.JoinLobby();
    }

    public void LoginCanvasScreen()
    {
        ClearCanvas();
        LoginCanvas.SetActive(true);
    }

    public void ProfileCanvasScreen()
    {
        ClearCanvas();
        ProfileCanvas.SetActive(true);
    }
    #endregion

    #region Login
    public void LoginScreen()
    {
        ClearScreenInLogin();
        FirebaseManager.instance.ClearWarningText();
        FirebaseManager.instance.ClearLoginFeilds();
        LoginUI.SetActive(true);
    }

    public void RegisterScreen()
    {
        ClearScreenInLogin();
        FirebaseManager.instance.ClearWarningText();
        FirebaseManager.instance.ClearRegisterFeilds();
        RegisterUI.SetActive(true);
    }

    public void WelcomScreen()
    {
        ClearScreenInLogin();
        FirebaseManager.instance.ClearWarningText();
        FirebaseManager.instance.ClearRegisterFeilds();
        WelcomeUI.SetActive(true);
    }
    #endregion

    #region Main
    public void MainScreen()
    {
        ClearScreenInMain();
        MainUI.SetActive(true);

    }

    public void RoomListScreen()
    {
        ClearScreenInMain();
        RoomListUI.SetActive(true);
    }

    public void LibaryScreen()
    {
        ClearScreenInMain();
        LibaryUI.SetActive(true);
    }

    #endregion

    #region Room
    public void SettingRoomScreen()
    {
        ClearScreenInRoom();
        SettingRoomUI.SetActive(true);
    }

    public void RoomScreen()
    {
        ClearScreenInRoom();
        RoomUI.SetActive(true);
        FriendUI.SetActive(false);
    }

    public void SettingRoomToLobby()
    {
        ClearScreenInRoom();
        SettingRoomUI.SetActive(true);
    }

    public void RoomToLobby()
    { 
        ClearScreenInRoom();
        RoomListUI.SetActive(true);
        FriendUI.SetActive(true);
        FriendUI.SetActive(true);
        NetworkManager.instance.ClearSettingRoomInfo();
    }

    public void EnterRoom()
    {
        RoomListUI.SetActive(false);
        RoomUI.SetActive(true);
        FriendUI.SetActive(false);
    }

    public void StartGame()
    {
        ClearScreenInRoom();
        InGameUI.SetActive(true);
    }

    public void ExitGame()
    {
        ClearScreenInRoom();
        NetworkManager.instance.LeaveRoom();
        InGameUI.SetActive(false);
        FriendUI.SetActive(true);
        MainUI.SetActive(true);
    }
    #endregion

    #region Profile

    public void EditProfile()
    {
        ProfileUI.SetActive(false);
        EditUI.SetActive(true);
        ProfileIconHandler.instance.profileIcon();
    }

    public void Profile()
    {
        EditUI.SetActive(false);
        ProfileUI.SetActive(true);
    }

    #endregion
}
