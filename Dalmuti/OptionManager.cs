using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetPlayerInfo
{
    public string name = "";
    public int num = 0;
    public PhotonPlayer NetPlayer = null;

    public void Reset()
    {
        name = "";
        num = -1;
        NetPlayer = null;
    }
}

// OptionManager는 게임의 옵션을 저장하며 싱글톤으로 항상 호출할수있게 만든다.
public class OptionManager : MonoBehaviour {

    static private OptionManager instance;
    static public OptionManager Instance
    {
        get
        {
            if(instance == null)
            {
                GameObject ins = new GameObject();

                instance = ins.AddComponent<OptionManager>();
                instance.name = "(Singleton)" + typeof(OptionManager).ToString();

                DontDestroyOnLoad(ins);

                if (instance == null)
                {
                    Debug.LogError("Option Manager Instance Error!!");
                }
            }

            return instance;
        }
    }    

    public string PlayerName;
    public int GameDifficulty;
    public float WaitTime;
    public int GameCount;

    public int MultiRoomNum = 10000;

    public bool Netgame = false;
    public int MyNetID = 0;

    public NetPlayerInfo[] NetPlayerList = new NetPlayerInfo[8];

    private bool init = false;

    public void NetPlayerSetting(List<PlayerInfomation> infolist)
    {
        for(int i = 0; i < infolist.Count; i++)
        {
            infolist[i].MyName = NetPlayerList[i].name;
            infolist[i].NetPlayer = NetPlayerList[i].NetPlayer;
        }
    }

    public void Init()
    {
        if (!init)
        {
            PlayerName = "PlayerName";
            GameDifficulty = 2;
            WaitTime = 15f;
            GameCount = 6;
            Netgame = false;

            init = true;

            for(int i = 0; i < NetPlayerList.Length; i++)
            {
                if(NetPlayerList[i] != null)
                    NetPlayerList[i].Reset();
            }
        }
        else
        {
            OptionControl.Instance.PlayernameInput.text = PlayerName;
            OptionControl.Instance.PlayerNameChange();

            OptionControl.Instance.GameDifficultySlider.value = GameDifficulty;
            OptionControl.Instance.DifficultyChange();

            OptionControl.Instance.WaitSlider.value = WaitTime;
            OptionControl.Instance.WaitChange();

            OptionControl.Instance.CountSlider.value = GameCount;
            OptionControl.Instance.CountChange();

            for (int i = 0; i < NetPlayerList.Length; i++)
            {
                NetPlayerList[i].Reset();
            }
        }
    }
}
