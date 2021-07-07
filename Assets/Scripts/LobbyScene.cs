using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class LobbyScene : MonoBehaviourPunCallbacks
{
    public const string DGAME_VERSION = "1.0";

    [SerializeField] LoginDlg loginDlg;

    [SerializeField] InputField inputRoom;
    [SerializeField] Button btnJoin;
    [SerializeField] Button btnCreate;

    [SerializeField] Text txtState;
    [SerializeField] RoomListUI roomListUI;
    [SerializeField] GameObject UIGroup;

    private CRoomItem SelectedRoomItem;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        //LocalSave.Inst().Load();
    }

    private void Start()
    {
        btnJoin.onClick.AddListener(OnClick_btnJoin);
        btnCreate.onClick.AddListener(OnClick_btnCreate);

        Initialize();
    }

    public void Initialize()
    {
        roomListUI.Initialize(OnCallback_SelectedItem);

        if (!PhotonNetwork.InLobby)
        {
            loginDlg.OpenUI(OnCallback_Login);
            txtState.text = "";
            UIGroup.SetActive(false);
        }
    }

    private void OnCallback_SelectedItem(CRoomItem item)
    {
        SelectedRoomItem = item;
    }

    public void OnCallback_Login(string nickName)
    {
        PhotonNetwork.GameVersion = DGAME_VERSION;
        PhotonNetwork.NickName = nickName;
        PhotonNetwork.ConnectUsingSettings();

        txtState.text = "마스터 서버에 접속중...";
    }

    public override void OnConnectedToMaster()
    {
        string state = "마스터 서버에 접속되었습니다.";

        btnJoin.interactable = true;
        txtState.text = state;
        loginDlg.CloseUI();

        TypedLobby lobby = new TypedLobby(LobbyInfo.DLOBBY_NAME, LobbyType.Default);
        PhotonNetwork.JoinLobby(lobby);

        UIGroup.SetActive(false);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        btnJoin.interactable = false;
        btnCreate.interactable = false;

        txtState.text = "마스터 서버에 연결되지 않았습니다.";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnJoinedLobby()
    {
        string sLobbyName = PhotonNetwork.CurrentLobby.Name;
        string str = string.Format("{0} 로비에 참가 했습니다.", sLobbyName);
        txtState.text = str;
    }

    private void OnClick_btnJoin()
    {
        JoinRoom();
    }

    private void CreateRoom(string roomName, byte maxCount)
    {
        ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
        hashtable.Add("roomState", 1);

        RoomOptions option = new RoomOptions();
        option.MaxPlayers = maxCount;
        option.IsOpen = true;
        option.IsVisible = true;
        option.CleanupCacheOnLeave = true;
        option.BroadcastPropsChangeToAll = true;

        option.CustomRoomProperties = hashtable;
        option.CustomRoomPropertiesForLobby = new string[] { "roomState" };

        TypedLobby lobby = new TypedLobby(LobbyInfo.DLOBBY_NAME, LobbyType.Default);

        PhotonNetwork.CreateRoom(roomName, option, lobby);
    }

    private void OnClick_btnCreate()
    {
        byte maxCount = (byte)LobbyInfo.DMAX_PLAYER;
        string roomName = inputRoom.text;
        CreateRoom(roomName, maxCount);
    }

    public void JoinRoom()
    {
        btnJoin.interactable = false;
        btnCreate.interactable = false;

        if (PhotonNetwork.IsConnected)
        {
            string sMsg = "룸에 접속...";

            if (SelectedRoomItem != null)
            {
                RoomInfo roomInfo = SelectedRoomItem.m_RoomInfo;
                string sRoomName = roomInfo.Name;
                ExitGames.Client.Photon.Hashtable table = roomInfo.CustomProperties;
                int state = (int)table["roomState"];
                if (state == (int)LobbyInfo.ERoomState.eGame)
                {
                    sMsg = "게임중에는 조인 할 수 없습니다. \n 랜덤으로 조인합니다.";
                    txtState.text = sMsg;
                    PhotonNetwork.JoinRandomRoom();
                    return;
                }
                if(roomInfo.PlayerCount >= roomInfo.MaxPlayers)
                {
                    sMsg = "방에 더이상 인원을 받을 수가 없습니다. \n랜덤으로 조인합니다.";
                    txtState.text = sMsg;
                    PhotonNetwork.JoinRandomRoom();
                    return;
                }
                PhotonNetwork.JoinRoom(sRoomName);
            }
            else
            {
                PhotonNetwork.JoinRandomRoom();
            }
            txtState.text = sMsg;
        }
        else
        {
            txtState.text = "오프라인 : 마스터 서버와 연결되지 않음\n접속 재시도 중...";
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        txtState.text = "빈 방이 없ㅅ브닏.. 새로운 방 생성..";

        string sRoomName = "RoomOf" + PhotonNetwork.NickName;
        CreateRoom(sRoomName, LobbyInfo.DMAX_PLAYER);
    }

    public override void OnJoinedRoom()
    {
        string sRoomName = PhotonNetwork.CurrentRoom.Name;
        string state = string.Format("방( {0} ) 참가 성공", sRoomName);

        txtState.text = state;

        PhotonNetwork.LoadLevel("RoomScene");
    }    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        txtState.text = "방 생성 실패";
    }

    private void OnApplicationQuit()
    {
        if (PhotonNetwork.InLobby) PhotonNetwork.LeaveLobby();
    }
}
