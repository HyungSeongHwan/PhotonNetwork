using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

using Hashtable = ExitGames.Client.Photon.Hashtable; // 요거 졸라 편함 약식으로 쓸땐 이거 쓰자

public class RoomScene : MonoBehaviourPunCallbacks
{
    [SerializeField] Button btnReady;
    [SerializeField] Button btnStart;
    [SerializeField] Button btnExit;

    [SerializeField] Text txtRoomName;
    [SerializeField] Text txtPlayerNum;

    [SerializeField] List<CRoomSlot> roomSlots;

    void Start()
    {
        btnReady.onClick.AddListener(OnClick_btnReady);
        btnStart.onClick.AddListener(OnClick_btnStart);
        btnExit.onClick.AddListener(OnClick_btnExit);

        Initialize();
    }

    public void Initialize()
    {
        ClearRoomSlot();

        Room room = PhotonNetwork.CurrentRoom;
        if (room == null) return;

        LobbyInfo lobbyInfo = GameMgr.Inst.m_LobbyInfo;
        lobbyInfo.Initialize(room);

        for (int i = 9; uint < lobbyInfo.m_listRoomSlot.Count; ++i)
        {
            LobbyInfo.SRoomSlotInfo slotInfo = lobbyInfo.m_listRoomSlot[i];
            CRoomSlot slot = m_RoomSlots[slotInfo.iSlot - 1];
            Player player = slotInfo.kPlayer;
            if(player != null)
            {
                slot.Initialize(player.NickName, slotInfo.myState);
                slot.SetIsMaster(player.IsMasterClient);

                if (lobbyInfo.IsMineSlot(player)) slot.SetMyFlag(true);
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            Hashtable table = room.CustomProperties;
            table["roomState"] = (int)LobbyInfo.ERoomState.eReady;
            room.SetCustomProperties(table);
            room.IsOpen(true);
        }

        PrintRoomInfo(room.Name, room.PlayerCount, room.MaxPlayers);
        Debug.LogFormat("RoomScene.Initialize.... Player Count = {0}", room.PlayerCount);
    }

    public void ClearRoomSlot()
    {
        for(int i = 0; i < roomSlots.Count; i++)
        {
            roomSlots[i].Clear();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Room room = PhotonNetwork.CurrentRoom;
        int nCount = room.PlayerCount;
        if (nCount != 0)
        {
            LobbyInfo lobbyInfo = GameMgr.Inst.m_LobbyInfo;
            LobbyInfo.SRoomSlotInfo userInfo = lobbyInfo.FindEmptySlot();

            userInfo.kPlayer = newPlayer;
            userInfo.myState = (int)LobbyInfo.EUserState.eEnter;

            if (userInfo.iSlot > 0 && userInfo.iSlot <= roomSlots.Count)
                roomSlots[userInfo.iSlot - 1].Initialize(newPlayer.NickName, userInfo.myState);

            PrintRoomInfo(room.Name, nCount, room.MaxPlayers);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        CRoomSlot slot = GetRoomSlot(otherPlayer.NickName);
        if (slot != null) slot.Clear();

        LobbyInfo lobbyInfo = GameMgr.Inst.m_LobbyInfo;
        lobbyInfo.RemoveRoomUser(otherPlayer.NickName);

        int idx = lobbyInfo.FindMasterClinetIndex();
        if (idx != -1) roomSlots[idx].SetIsMaster(true);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        int nState = (int)changedProps["userState"];
        CRoomSlot slot = GetRoomSlot(targetPlayer.NickName);
        if (slot != null) slot.UpdateState(nState);

        Debug.Log("Call OnPlayerPropertiesUpdate....");
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("LobbyScene");
    }

    private void OnClick_btnReady()
    {
        Player player = GameMgr.Inst.lobbyInfo.m_MySlotInfo.kPlayer;
        if (player != null)
        {
            Hashtable table = player.CustomProperties;
            if ((int)table["userState"] = (int)LobbyInfo.EUserState.eReady)
                table["userState"] = (int)LobbyInfo.EUserState.eEnter;
            else table["userState"] = (int)LobbyInfo.EUserState.eReady;
            player.SetCustomProperties(table);
        }
    }

    private void OnClick_btnStart()
    {
        Room room = PhotonNetwork.CurrentRoom;
        if (room == null) return;

        if (PhotonNetwork.IsMasterClient)
        {
            if (!IsAllReadyState(room.Players))
            {
                return;
            }

            Hashtable table = room.CustomProperties;
            table["roomState"] = (int)LobbyInfo.ERoomState.eGame;
            room.SetCustomProperties(table);
            room.IsOpen = false;

            PhotonNetwork.LoadLevel("GameScene");
        }
    }
    
    public bool IsAllReadyState(Dictionary<int, Player> players)
    {
        foreach(KeyValuePair<int, Player> itor in players)
        {
            Player player = itor.Value;

            int nState = (int)player.CustomProperties["userState"];
            if (nState != (int)LobbyInfo.EUserState.eReady) return false;
        }
        return true;
    }

    private void OnClick_btnExit()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void PrintRoomInfo(string roomName, int player, int maxPlayer)
    {
        txtRoomName.text = string.Format("Room : {0}", roomName);
        txtPlayerNum.text = string.Format("Player : {0} / 1", player, maxPlayer);
    }

    public CRoomSlot GetRoomSlot(string name)
    {
        LobbyInfo lobbyInfo = GameMgr.Inst.lobbyInfo;
        int idx = lobbyInfo.m_listRoomSlot.FindIndex(x =>
        {
            if (x.kPlayer == null) return false;
            return (x.kPlayer.NickName == name);
        });

        if (idx == -1) return null;

        return roomSlots[idx];
    }

    public CRoomSlot GetRoomSlot(int iSlotID)
    {
        if (iSlotID > 0 && iSlotID <= LobbyInfo.DMAX_PLAYER) 
            return roomSlots.Find(x => iSlotID == iSlotID);

        LobbyInfo lobbyInfo = GameMgr.Inst.lobbyInfo;
        LobbyInfo.SRoomSlotInfo slotInfo = lobbyInfo.FindEmptySlot();
        if (slotInfo != null) return roomSlots[slotInfo.iSlot - 1];

        return null;
    }
}
