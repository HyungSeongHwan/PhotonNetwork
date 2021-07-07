using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.EventSystems;


/*
 * 
 *  로비 정보
 */
public class LobbyInfo
{
    public enum ERoomState
    {
        eReady = 1,     // 게임 시작 전 상태
        eGame = 2,      // 게임중 상태
    }
    public enum EUserState
    {
        eEmpty = 0,     // 빈슬롯인 경우
        eEnter = 1,     // 룸 입장상태
        eReady = 2,     // 게임 시작 대기 상태
    }

    // 로비의 룸 정보
    public class SMyRoomInfo
    {
        public Room kRoom = null;
        public int roomState = 0;    // ERoomState
    }

    // 룸의 유저 정보
    public class SRoomSlotInfo
    {
        public int iSlot = 0;            // 슬롯 인덱스( 1부터 시작한다.) 
        public Player kPlayer = null;    // Player
        public int myState = 0;          // 유저의 상태 EUserState , 기본 empty 상태

        public void Clear()
        {
            kPlayer = null;
            myState = (int)EUserState.eEmpty;
        }
    }

    //--------------------------------------------
    public const int DMAX_PLAYER = 4;           // 최대 플레이어 숫자
    public const string DLOBBY_NAME = "TURRET"; // 이게임 로비 이름 설정

    public SMyRoomInfo m_MyRoomInfo = new SMyRoomInfo();

    public List<SRoomSlotInfo> m_listRoomSlot = new List<SRoomSlotInfo>();  // 4개를 강제로 만들어 사용한다.
    public SRoomSlotInfo m_MySlotInfo = new SRoomSlotInfo();

    //public string m_NickName = "";      // 유저 닉네임 ( 나 자신 꺼임 ..)

    public void Initialize(Room room)
    {
        m_MyRoomInfo.kRoom = room;
        m_MyRoomInfo.roomState = (int)ERoomState.eReady;
        Initialize_UserState(room);
        Initialize_MySlotInfo();
    }
    public void Initialize_UserState(Room kRoom)
    {
        m_listRoomSlot.Clear();
        for (int i = 0; i < DMAX_PLAYER; i++)
        {
            SRoomSlotInfo kInfo = new SRoomSlotInfo();
            kInfo.iSlot = i + 1;
            m_listRoomSlot.Add(kInfo);
        }

        int nCount = 0;
        Dictionary<int, Player> kPlayerList = kRoom.Players;
        foreach (KeyValuePair<int, Player> kItem in kPlayerList)
        {
            int key = kItem.Key;
            Player kPlayer = kItem.Value;
            if (nCount < DMAX_PLAYER)
            {
                if (kPlayer.NickName == PhotonNetwork.NickName)
                {
                    m_MySlotInfo.kPlayer = kPlayer;
                }
                else
                {
                    ExitGames.Client.Photon.Hashtable kHashtable = kPlayer.CustomProperties;
                    int iSlot = (int)kHashtable["iRoomSlot"];

                    SRoomSlotInfo kSlotInfo = GetRoomSlotInfo(iSlot);
                    if (kSlotInfo != null)
                    {
                        kSlotInfo.kPlayer = kPlayer;
                        kSlotInfo.myState = (int)kHashtable["userState"];
                    }
                }
            }
            nCount++;
        }
    }

    private void Initialize_MySlotInfo()
    {
        Player kPlayer = m_MySlotInfo.kPlayer;

        SRoomSlotInfo kSlotInfo = FindEmptySlot();
        kSlotInfo.kPlayer = kPlayer;
        kSlotInfo.myState = (int)EUserState.eEnter;


        ExitGames.Client.Photon.Hashtable kHashtable = kPlayer.CustomProperties;
        if (kHashtable == null)
            kHashtable = new ExitGames.Client.Photon.Hashtable();

        if (kHashtable.Count == 0)
        {
            kHashtable.Add("userState", (int)EUserState.eEnter);
            kHashtable.Add("iRoomSlot", kSlotInfo.iSlot);
            kPlayer.SetCustomProperties(kHashtable);
        }

        m_MySlotInfo.iSlot = kSlotInfo.iSlot;
    }

    public void SetCurRoomInfo(Room room)
    {
        m_MyRoomInfo.kRoom = room;
    }
    public void SetCurRoomState(ERoomState eState)
    {
        m_MyRoomInfo.roomState = (int)eState;
    }

    public int GetCurRoomState()
    {
        return m_MyRoomInfo.roomState;
    }

    public int GetMyUserState()
    {
        return m_MySlotInfo.myState;
    }

    public void SetMySlotInfo(Player kPlayer, int iSlot)
    {
        m_MySlotInfo.kPlayer = kPlayer;
        m_MySlotInfo.iSlot = iSlot;
    }
    //public void SetMySlotInfo( SRoomSlotInfo kSlotInfo )
    //{
    //    m_MySlotInfo = kSlotInfo;
    //}

    public void SetRoomUserState(string sName, int nState)
    {
        int idx = m_listRoomSlot.FindIndex(x => x.kPlayer.NickName == sName);
        if (idx != -1)
            m_listRoomSlot[idx].myState = nState;

        //SRoomSlotInfo kInfo = m_listRoomSlot.Find(x => x.kPlayer.NickName == sName);
        //if (kInfo != null)
        //    kInfo.myState = nState;

    }

    //public void SetNickName( string name)
    //{
    //    m_NickName = name;
    //}

    public void RemoveRoomUser(string name)
    {
        SRoomSlotInfo kSlotInfo = GetRoomSlotInfo(name);
        if (kSlotInfo != null)
        {
            kSlotInfo.Clear();
        }
    }

    public SRoomSlotInfo GetRoomSlotInfo(string name)
    {
        return m_listRoomSlot.Find(x => x.kPlayer.NickName == name);
    }
    public SRoomSlotInfo GetRoomSlotInfo(int iSlotID)
    {
        SRoomSlotInfo kInfo = m_listRoomSlot.Find(x => x.iSlot == iSlotID);
        if (kInfo != null)
            return kInfo;

        return FindEmptySlot();
    }

    public SRoomSlotInfo FindEmptySlot()
    {
        return m_listRoomSlot.Find(x => x.myState == 0);
    }

    public int FindMasterClinetIndex()
    {
        int idx = m_listRoomSlot.FindIndex(x => {
            if (x.kPlayer != null)
            {
                return x.kPlayer.IsMasterClient;
            }
            return false;
        });
        return idx;
    }

    // 내슬롯 인가/.
    public bool IsMineSlot(Player kPlayer)
    {
        return (kPlayer.NickName == PhotonNetwork.NickName); // m_NickName);
    }

    public void OnUpdate(float fElpaseTime)
    {

    }
}
