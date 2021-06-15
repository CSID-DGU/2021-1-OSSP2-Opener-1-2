using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text;
// for generate room id; hash(host_id + timestamp)

using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Hashtable = ExitGames.Client.Photon.Hashtable;



public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager instance;
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [Header("DisconnectPanel")]
    public TMP_Text NickNameInput;

    [Header("LobbyPanel")]
    public Button[] CellBtn;
    public Button PreviousBtn;
    public Button NextBtn;

    [Header("RoomPanel")]
    public GameObject RoomUI;
    public Text[] ChatText;
    public InputField ChatInput;
    public TMP_Text RoomInfoText;
    public TMP_Text[] PlayerList;
    public TMP_InputField inputRoomName;
    public TMP_Text warningtext;
    public TMP_Text GameNameInRoom;

    [Header("SetRoom")]
    public TMP_InputField RoomName;
    public TMP_InputField MaxPlayer;
    public TMP_Text GameName;

    [Header("ETC")]
    public PhotonView PV;

    public List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple;

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

    public void Start()
    {
        Connect();
        inputRoomName.text = "";
    }

    public void ClearSettingRoomInfo()
    {
        RoomName.text = "";
        GameName.text = "";
        MaxPlayer.text = "";
    }

    public void BackToRoomList()
    {
        LeaveRoom();
        UIManager.instance.RoomToLobby();
        ClearSettingRoomInfo();
    }

    public void MakeRoom()
    {
        MyListRenewal();
        CreateRoom();
        UIManager.instance.RoomScreen();
    }

    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();

    public void Disconnect() => PhotonNetwork.Disconnect();

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("disconncected");
    }

    public override void OnJoinedLobby()
    {
        Debug.Log(NickNameInput.text + " joined lobby");
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        myList.Clear();
    }

    #region RoomList
    public void MyListClick(int num)
    {
        if (num == -2) --currentPage;
        else if (num == -1) ++currentPage;
        else
        {
            if(myList[multiple + num].MaxPlayers == myList[multiple + num].PlayerCount)
            {
                warningtext.text = "You can't enter the room";
            }
            PhotonNetwork.JoinRoom(myList[multiple + num].Name);
            Debug.Log("myList[multiple + num].Name" + myList[multiple + num].Name);
        }

        inputRoomName.text = "";
        MyListRenewal();
    }

    public void MyListRenewal()
    {
        maxPage = (myList.Count % CellBtn.Length == 0) ? myList.Count / CellBtn.Length : myList.Count / CellBtn.Length + 1;

        PreviousBtn.interactable = (currentPage <= 1) ? false : true;
        NextBtn.interactable = (currentPage >= maxPage) ? false : true;

        multiple = (currentPage - 1) * CellBtn.Length;

        for (int i = 0; i < CellBtn.Length; i++)
        {
            CellBtn[i].interactable = (multiple + i < myList.Count) ? true : false;
            CellBtn[i].transform.GetChild(0).GetComponent<TMP_Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
            CellBtn[i].transform.GetChild(1).GetComponent<TMP_Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers.ToString() : "";
        }
    }

    public void SearchRoom()
    {
        warningtext.text = "";
        for(int i = 0; i < myList.Count; i++)
        {
            string[] result = myList[i].Name.Split(new char[] { '/' });
            CellBtn[i].interactable = false;
            if (result[0].Equals(inputRoomName.text + " "))
            {
                if ((multiple + i) % 4 == 0)
                {
                    CellBtn[0].interactable = true;
                    CellBtn[0].transform.GetChild(0).GetComponent<TMP_Text>().text = myList[multiple + i].Name;
                    CellBtn[0].transform.GetChild(1).GetComponent<TMP_Text>().text = myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers.ToString();
                }
                else 
                {
                    CellBtn[0].interactable = true;
                    CellBtn[0].transform.GetChild(0).GetComponent<TMP_Text>().text = myList[multiple + i].Name;
                    CellBtn[0].transform.GetChild(1).GetComponent<TMP_Text>().text = myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers.ToString();
                    CellBtn[i].interactable = false;
                    CellBtn[i].transform.GetChild(0).GetComponent<TMP_Text>().text = "";
                    CellBtn[i].transform.GetChild(1).GetComponent<TMP_Text>().text = "";

                    myList[0] = myList[multiple + i];
                }
            }
            else if (inputRoomName.text.Equals(""))
            {
                warningtext.text = "Spaces are not allowed";
                MyListRenewal();
            }
            else
            {
                CellBtn[i].interactable = false;
                CellBtn[i].transform.GetChild(0).GetComponent<TMP_Text>().text = "";
                CellBtn[i].transform.GetChild(1).GetComponent<TMP_Text>().text = "";
            }
        }
    }

    public void Refresh()
    {
        inputRoomName.text = "";
        warningtext.text = "";
        MyListRenewal();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("OnRoomListUpdate");
        int roomCount = roomList.Count;
        Debug.Log("roomList.Count" + roomList.Count);
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
        MyListRenewal();
    }
    #endregion


    #region Room
    public void CreateRoom()
    {
        GameName.text = DropdownHandler.instance.gamename.text;
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = Convert.ToByte(System.Int32.Parse(MaxPlayer.text));
        roomOptions.CustomRoomProperties = new Hashtable() { { "GameName", GameName.text} };
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "GameName" };

        GameNameInRoom.text = GameName.text;
        PhotonNetwork.CreateRoom(RoomName.text + " / " + GameName.text, roomOptions);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnJoinedRoom()
    {
        UIManager.instance.EnterRoom();
        RoomRenewal();
        ChatInput.text = "";
        for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = "";
    }

    public override void OnCreateRoomFailed(short returnCode, string message) { RoomName.text = "test"; CreateRoom(); }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + newPlayer.NickName + " entered the room</color>");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + otherPlayer.NickName + " left the room</color>");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + newMasterClient.NickName + "is a New Master Client</color=yellow>");
    }

    void RoomRenewal()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            PlayerList[i].text = PhotonNetwork.PlayerList[i].NickName;
        for (int i = 5; i > PhotonNetwork.PlayerList.Length; i--)
            PlayerList[i - 1].text = "";

        Room room = PhotonNetwork.CurrentRoom;

        RoomInfoText.text = room.Name + " / " + room.PlayerCount + " / " + room.MaxPlayers;
    }
    #endregion

    #region Chat
    public void Send()
    {
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text);
        Debug.Log(ChatInput.text);
        ChatInput.text = "";
    }

    [PunRPC]
    void ChatRPC(string msg)
    {
        bool isInput = false;
        for (int i = 0; i < ChatText.Length; i++)
            if (ChatText[i].text == "")
            {
                isInput = true;
                ChatText[i].text = msg;
                break;
            }
        if (!isInput)
        {
            for (int i = 1; i < ChatText.Length; i++) ChatText[i - 1].text = ChatText[i].text;
            ChatText[ChatText.Length - 1].text = msg;
        }
    }
    #endregion

    #region GameStart
    public void StartGame()
    {
        string _userId = FirebaseManager.instance.userId;
        Debug.Log("userID : " + _userId);
        string plainRoomName = _userId + getTimeStampString();
        string hashRoomName = sha256_hash(plainRoomName);
        PV.RPC("StartRPC", RpcTarget.All, hashRoomName);

        UIManager.instance.StartGame();
        ShowWindow(GetActiveWindow(), 2);
    }


    [PunRPC]
    void StartRPC(string roomName)
    {
        string _nickName = FirebaseManager.instance.nickName.text;

        if (string.IsNullOrEmpty(_nickName))
        {
            _nickName = "default_NickName";
        }

        ExecuteTheme(_nickName, roomName);
    }

    void ExecuteTheme(string nickName, string roomName)
    {
        System.Diagnostics.Process process = new System.Diagnostics.Process();

        string os = SystemInfo.operatingSystem;
        string separator = "/";

        string pathPrefix = "C:/welvi/Map/" + GameName.text +" /";
        string exeDirName = GameName.text;
        string exeFileName = exeDirName + ".exe";
        string entirePath = pathPrefix + separator + exeDirName + separator + exeFileName;

        // for debugging in mac
        if (os.StartsWith("Mac"))
        {
            entirePath = pathPrefix + exeDirName + "/Contents/MacOS/" + exeDirName;
        }

        process.StartInfo.FileName = "C:/welvi/Map/" + exeDirName + "/" + exeFileName;
        process.StartInfo.UseShellExecute = true;

        if (String.IsNullOrEmpty(roomName))
        {
            nickName = "defaultRoomName";
        }

        process.StartInfo.Arguments = nickName + " " + roomName;

        process.Start();
    }

    string getTimeStampString()
    {
        var timeStamp = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
        return timeStamp.ToString();
    }
    public static string sha256_hash(string value)
    {
        StringBuilder Sb = new StringBuilder();
        using (SHA256 hash = SHA256Managed.Create())
        {
            Encoding enc = Encoding.UTF8;
            Byte[] result = hash.ComputeHash(enc.GetBytes(value));

            foreach (Byte b in result)
                Sb.Append(b.ToString("x2"));
        }
        return Sb.ToString();
    }
    #endregion
}