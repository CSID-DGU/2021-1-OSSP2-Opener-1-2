using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using System.Linq;
using System.Diagnostics;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;

    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;
    public DatabaseReference DBreference;

    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;

    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;

    [Header("Friend")]
    public TMP_InputField UsernameField;
    public Transform OnlineFriendContent;
    public Transform OfflineFriendContent;
    public GameObject OnlineFriend;
    public GameObject OfflineFriend;

    [Header("UserInfo")]
    public TMP_Text nickName;
    public TMP_Text nickNameProfile;
    public int icon;
    public TMP_InputField editNickNameField;
    public string userId;

    private float TimeLeft = 2.0f;
    private float nextTime = 0.0f;

    void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                UnityEngine.Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            UnityEngine.Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void InitializeFirebase()
    {
        UnityEngine.Debug.Log("Setting up Firebase Auth");
        auth = FirebaseAuth.DefaultInstance;
        DBreference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void ClearLoginFeilds()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
    }

    public void ClearWarningText()
    {
        warningLoginText.text = "";
        warningRegisterText.text = "";
    }

    public void ClearRegisterFeilds()
    {
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
    }

    public void ClearFriendFields()
    {
        UsernameField.text = "";
    }

    public void LoginButton()
    {
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }

    public void RegisterButton()
    {
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }

    public void SendRequestButton()
    {
        StartCoroutine(Request(UsernameField.text));
    }

    public void RemoveRequestButton()
    {
        StartCoroutine(Remove(UsernameField.text));
    }
    public void FriendListPrintButton()
    {
        StartCoroutine(FList());
    }

    public void ChangeNickNameButton()
    {
        StartCoroutine(UpdateNickName(editNickNameField.text));
    }

    public void ChangeIcon(int num)
    {
        StartCoroutine(UpdateIcon(num));
    }

    public void GetIconNum()
    {
        StartCoroutine(LoadIconNum());
    }

    public void Update()
    {
        if (Time.time > nextTime)
        {
            nextTime = Time.time + TimeLeft;
            FriendListPrintButton();
        }
    }

    public void OnApplicationQuit()
    {
        StartCoroutine(UpdateOnAndOff());
#if UNITY_EDITOR
        StartCoroutine(UpdateOnAndOff());
#endif
//#if !UNITY_EDITOR
//    System.Diagnostics.Process.GetCurrentProcee().kill();
//#endif
    }

    private IEnumerator Login(string _email, string _password)
    {
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            UnityEngine.Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            UnityEngine.Debug.Log("Failed Login");
            warningLoginText.text = message;
        }
        else
        {
            //User is now logged in
            //Now get the result
            
            user = LoginTask.Result;
            DBreference.Child("users").Child(user.UserId).Child("onoff").SetValueAsync(1);

            StartCoroutine(LoadUserData());

            yield return new WaitForSeconds(2);

            StartCoroutine(LoadIconNum());

            ClearLoginFeilds();
            ClearRegisterFeilds();

            UIManager.instance.MainCanvasScreen();
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            //If the username field is blank show a warning
            warningRegisterText.text = "Missing Username";
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //If the password does not match show a warning
            warningRegisterText.text = "Password Does Not Match!";
        }
        else
        {
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                UnityEngine.Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
                warningRegisterText.text = message;
            }
            else
            {
                //User has now been created
                //Now get the result
                user = RegisterTask.Result;

                if (user != null)
                {
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    var ProfileTask = user.UpdateUserProfileAsync(profile);
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        UnityEngine.Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        warningRegisterText.text = "Username Set Failed!";
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen

                        var DBTask = DBreference.Child("users").Child(user.UserId).Child("username").SetValueAsync(_username);

                        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

                        if (DBTask.Exception != null)
                        {
                            UnityEngine.Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
                        }
                        else
                        {
                            //Database username is now updated
                        }

                        var DBTask2 = DBreference.Child("users").Child(user.UserId).Child("id").SetValueAsync(user.UserId);

                        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);

                        if (DBTask2.Exception != null)
                        {
                            UnityEngine.Debug.LogWarning(message: $"Failed to register task with {DBTask2.Exception}");
                        }
                        else
                        {
                            //Database id is now updated
                        }

                        var DBTask3 = DBreference.Child("users").Child(user.UserId).Child("onoff").SetValueAsync(0);

                        yield return new WaitUntil(predicate: () => DBTask3.IsCompleted);

                        if (DBTask3.Exception != null)
                        {
                            UnityEngine.Debug.LogWarning(message: $"Failed to register task with {DBTask3.Exception}");
                        }
                        else
                        {
                            //Database id is now updated
                           
                        }

                        var DBTask4 = DBreference.Child("users").Child(user.UserId).Child("icon").SetValueAsync(0);

                        yield return new WaitUntil(predicate: () => DBTask3.IsCompleted);

                        if (DBTask3.Exception != null)
                        {
                            UnityEngine.Debug.LogWarning(message: $"Failed to register task with {DBTask3.Exception}");
                        }
                        else
                        {
                            //Database id is now updated

                        }

                        UIManager.instance.WelcomScreen();
                        warningRegisterText.text = "";
                        ClearRegisterFeilds();
                        ClearLoginFeilds();
                    }
                }
            }
        }
    }

    private IEnumerator LoadUserData()
    {
        var DBTask = DBreference.Child("users").Child(user.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            UnityEngine.Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            nickName.text = "unknown";
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;
            nickName.text = snapshot.Child("username").Value.ToString();
            nickNameProfile.text = nickName.text;
            userId = snapshot.Child("id").Value.ToString();
        }

    }

    private IEnumerator LoadIconNum()
    {
        var DBTask = DBreference.Child("users").Child(user.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            UnityEngine.Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            icon = 0;
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;
            icon = System.Int32.Parse(snapshot.Child("icon").Value.ToString());
            UnityEngine.Debug.Log(icon);
            ProfileIconHandler.instance.setIcon(icon);
        }
    }

    private IEnumerator UpdateNickName(string nickname)
    {
        var DBTask = DBreference.Child("users").Child(user.UserId).Child("username").SetValueAsync(nickname);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            UnityEngine.Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {

        }

        nickName.text = nickname;
        nickNameProfile.text = nickname;
    }

    private IEnumerator UpdateOnAndOff()
    {
        var DBTask = DBreference.Child("users").Child(user.UserId).Child("onoff").SetValueAsync(0);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            UnityEngine.Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {

        }
    }

    private IEnumerator UpdateIcon(int num)
    {
        var DBTask = DBreference.Child("users").Child(user.UserId).Child("icon").SetValueAsync(num);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            UnityEngine.Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {

        }
    }

    private IEnumerator Request(string username)
    {

        string Friend_uid = "None";
        var DBTask = DBreference.Child("users").OrderByChild("username").GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            UnityEngine.Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {

            DataSnapshot snapshot = DBTask.Result;

            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                string name = childSnapshot.Child("username").Value.ToString();
                if (name == username)
                {
                    Friend_uid = childSnapshot.Child("id").Value.ToString();
                    break;
                }
            }
        }

        var DBTask2 = DBreference.Child("FriendList").Child(user.UserId).Child(Friend_uid).Child("Nickname").SetValueAsync(username);

        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);

        if (DBTask2.Exception != null)
        {
            UnityEngine.Debug.LogWarning(message: $"Failed to register task with {DBTask2.Exception}");
        }
        else
        {
            DBreference.Child("FriendList").Child(user.UserId).Child(Friend_uid).Child("id").SetValueAsync(Friend_uid);
        }


        string Log_name = " ";
        var DBTask4 = DBreference.Child("users").OrderByChild("id").GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask4.IsCompleted);

        if (DBTask4.Exception != null)
        {
            UnityEngine.Debug.LogWarning(message: $"Failed to register task with {DBTask4.Exception}");
        }
        else
        {

            DataSnapshot snapshot2 = DBTask4.Result;

            foreach (DataSnapshot childSnapshot in snapshot2.Children.Reverse<DataSnapshot>())
            {
                string Find_id = childSnapshot.Child("id").Value.ToString();
                if (Find_id == user.UserId)
                {
                    Log_name = childSnapshot.Child("username").Value.ToString();
                    break;
                }
            }
        }

        DBreference.Child("FriendList").Child(Friend_uid).Child(user.UserId).Child("Nickname").SetValueAsync(Log_name);
        var DBTask3 = DBreference.Child("FriendList").Child(Friend_uid).Child(user.UserId).Child("id").SetValueAsync(user.UserId);

        yield return new WaitUntil(predicate: () => DBTask3.IsCompleted);

        if (DBTask3.Exception != null)
        {
            UnityEngine.Debug.LogWarning(message: $"Failed to register task with {DBTask3.Exception}");
        }
        else
        {
            UnityEngine.Debug.Log("Add Friend Success");
        }
        ClearFriendFields();
    }

    private IEnumerator Remove(string username)
    {
        string Friend_uid = "None";
        var DBTask = DBreference.Child("FriendList").Child(user.UserId).OrderByChild("Nickname").GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            UnityEngine.Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {

            DataSnapshot snapshot = DBTask.Result;

            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                string name = childSnapshot.Child("Nickname").Value.ToString();
                if (name == username)
                {
                    Friend_uid = childSnapshot.Child("id").Value.ToString();
                    break;
                }
            }
        }

        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference.Child("FriendList").Child(user.UserId).Child(Friend_uid);
        reference.RemoveValueAsync();

        DatabaseReference reference2 = FirebaseDatabase.DefaultInstance.RootReference.Child("FriendList").Child(Friend_uid).Child(user.UserId);
        reference2.RemoveValueAsync();

        ClearFriendFields();

    }

    private IEnumerator FList()
    {
        var DBTask1 = DBreference.Child("FriendList").Child(user.UserId).OrderByChild("Nickname").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);

        var DBTask2 = DBreference.Child("users").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);

        if (DBTask1.Exception != null)
        {
            UnityEngine.Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
        }
        else if (DBTask2.Exception != null)
        {
            UnityEngine.Debug.LogWarning(message: $"Failed to register task with {DBTask2.Exception}");
        }
        else
        {
            DataSnapshot snapshot1 = DBTask1.Result;
            DataSnapshot snapshot2 = DBTask2.Result;

            foreach (Transform child in OnlineFriendContent.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in OfflineFriendContent.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (DataSnapshot childSnapshot1 in snapshot1.Children.Reverse<DataSnapshot>())
            {
                string username = childSnapshot1.Child("Nickname").Value.ToString();
                string userid1 = childSnapshot1.Child("id").Value.ToString();
                
                foreach (DataSnapshot childSnapshot2 in snapshot2.Children.Reverse<DataSnapshot>()) 
                { 
                    string userid2 = childSnapshot2.Child("id").Value.ToString();
                    string userstate = childSnapshot2.Child("onoff").Value.ToString();

                    if (userid1.Equals(userid2))
                    {
                        if (userstate.Equals("1")) 
                        {
                            GameObject OnlineFriendElement = Instantiate(OnlineFriend, OnlineFriendContent);
                            OnlineFriendElement.GetComponentInChildren<TMP_Text>().text = username;
                        }
                        else if (userstate.Equals("0"))
                        {
                            GameObject OfflineFriendElement = Instantiate(OfflineFriend, OfflineFriendContent);
                            OfflineFriendElement.GetComponentInChildren<TMP_Text>().text = username;
                        }
                        else
                        {
                            UnityEngine.Debug.Log("error");
                        }
                    }
                }

            }
        }

    }
}
