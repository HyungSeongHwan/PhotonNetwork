using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CRoomSlot : MonoBehaviour
{
    [SerializeField] Text m_State = null;           // 0: Enter, 1: Ready  -> LobbyInfo.ERoomState
    [SerializeField] Text m_NickName = null;
    [SerializeField] Image m_ReadyBg = null;       // Ready상태의 BG
    [SerializeField] Image m_MyFlag = null;        // 나 인지 체크 표시
    [SerializeField] Image m_MasterFlag = null;    // 방장 인지 체크 표시
    public int m_SlotIdx = 0;                       // 슬롯 id-  0부터 시작

    public void Initialize(string nickName, int nState)
    {
        m_NickName.text = nickName;
        UpdateState(nState);
    }


    public void UpdateState(int nState)
    {

        string sState;
        if (nState == (int)LobbyInfo.EUserState.eReady)
        {
            sState = "Ready";
            m_State.color = GetStateTextColor(nState);
            m_ReadyBg.gameObject.SetActive(true);
        }
        else if (nState == (int)LobbyInfo.EUserState.eEnter)
        {
            sState = "Enter";
            m_State.color = GetStateTextColor(nState);
            m_ReadyBg.gameObject.SetActive(false);
        }
        else
        {
            sState = "Empty";
            m_State.color = GetStateTextColor(nState);
            m_ReadyBg.gameObject.SetActive(false);
        }
        m_State.text = sState;
    }

    public void Clear()
    {
        m_NickName.text = "None";
        m_State.text = "Empty";
        m_ReadyBg.gameObject.SetActive(false);
        SetMyFlag(false);
        SetIsMaster(false);
        m_State.color = GetStateTextColor(0);
    }

    public Color GetStateTextColor(int nState)
    {
        Color kColor = Color.black;
        switch (nState)
        {
            case (int)LobbyInfo.EUserState.eReady:
                kColor = new Color((float)245 / 255, (float)117 / 255, (float)88 / 255);
                break;
            case (int)LobbyInfo.EUserState.eEnter:
                kColor = new Color((float)150 / 255, (float)245 / 255, (float)78 / 255);
                break;
        }

        return kColor;
    }

    // 나 자신인지 체크하기
    public void SetMyFlag(bool bMyFlag)
    {
        m_MyFlag.gameObject.SetActive(bMyFlag);
    }

    public void SetIsMaster(bool bMaster)
    {
        m_MasterFlag.gameObject.SetActive(bMaster);
    }

}
