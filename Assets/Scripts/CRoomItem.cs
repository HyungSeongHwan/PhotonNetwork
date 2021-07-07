using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class CRoomItem : MonoBehaviour
{
    [SerializeField] Text m_txtName = null;
    [SerializeField] Text m_txtState = null;
    [SerializeField] Image m_imgSelect = null;

    [HideInInspector] public RoomInfo m_RoomInfo = null;


    public void Initialize(RoomInfo kInfo)
    {
        m_RoomInfo = kInfo;
        m_txtName.text = string.Format("{0} ( {1}/{2} )", kInfo.Name, kInfo.MaxPlayers, kInfo.PlayerCount);

        string sState = "";
        ExitGames.Client.Photon.Hashtable kHashTable = kInfo.CustomProperties;
        if (kHashTable.ContainsKey("roomState"))
        {
            int nState = (int)kHashTable["roomState"];
            sState = nState == 2 ? "game" : "ready";
        }
        m_txtState.text = sState;

    }

    public bool IsSelected()
    {
        return m_imgSelect.gameObject.activeSelf;
    }

    public void ShowSelectImage(bool bShow)
    {
        m_imgSelect.gameObject.SetActive(bShow);
    }
}
