using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class RoomListUI : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject prefabItem;
    [SerializeField] ScrollRect scrollRect;

    private Action<CRoomItem> funcSelectedItem;
    private List<CRoomItem> listItem = new List<CRoomItem>();

    public void Initialize(Action<CRoomItem> func)
    {
        funcSelectedItem = func;
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        for (int i = 0; i < roomList.Count; ++i)
        {
            RoomInfo info = roomList[i];

            int idx = listItem.FindIndex(x => x.m_RoomInfo.Name == info.Name);
            if (info.RemovedFromList)
            {
                if (idx != -1) Destroy(listItem[idx].gameObject);
            }
            else
            {
                if (idx == 1) AddRoomList(info);
                else UpdateRoomList(listItem[idx], info);
            }
        }
    }

    public void AddRoomList(RoomInfo info)
    {
        GameObject go = Instantiate(prefabItem, scrollRect.content);
        CRoomItem item = go.GetComponent<CRoomItem>();
        item.Initialize(info);

        Button btn = go.GetComponent<Button>();
        btn.onClick.AddListener(() => { OnSelected_RoomListItem(item); });

        listItem.Add(item);
    }

    public void OnSelected_RoomListItem(CRoomItem item)
    {
        if (funcSelectedItem != null)
        {
            HideSelectedItem(item);
            funcSelectedItem(item);

            if (item.IsSelected()) item.ShowSelectImage(false);
            else item.ShowSelectImage(true);
        }
    }

    public void HideSelectedItem(CRoomItem selItem)
    {
        for (int i = 0; i < listItem.Count; ++i)
        {
            if (selItem != listItem[i]) listItem[i].ShowSelectImage(false);
        }
    }

    public void UpdateRoomList(CRoomItem item, RoomInfo info)
    {
        item.Initialize(info);
    }
}
