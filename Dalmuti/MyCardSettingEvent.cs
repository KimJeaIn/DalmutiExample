using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// 현재 핸드에있는 카드를 설정할때 쓰이는 클래스
public class MyCardSettingEvent : MonoBehaviour {   

    public enum MyCardStat {Normal, Popup, Ready};

    static private MyCardSettingEvent Instance;
    static public MyCardSettingEvent instance
    {
        get
        {
            if (Instance == null)
            {
                Instance = FindObjectOfType<MyCardSettingEvent>();

                if (Instance == null)
                    Debug.LogError("No Find instance!");
            }

            return Instance;
        }
    }

    public float StartXPos;
    public float X_Gap;
    public List<CardInfomation> CardList = new List<CardInfomation>();
    public PlayerInfomation MyInfo;

    //161021 NextButton은 ReadyCardSetting에 있음
    public Button PassButton;

    //161021 카드 상태 추가..
    //카드 상태에따라 버튼 활성화가 틀려진다.
    private MyCardStat CurrentCardStat = MyCardStat.Normal;
    public MyCardStat GetCardStat() { return CurrentCardStat; }
    public void SetCardStat(MyCardStat stat) { CurrentCardStat = stat; }
    //..161021

    public GameObject RevolButtonObj;
    public Text RevolText;
    public Text RevolText2;

    private string m_revolText1 = "혁명은 모든 세금을 없앱니다";
    private string m_revolText2 = "대혁명은 랭크를 역순으로 바꿉니다";

    private int CurrentCardCount = 0;
	
	void Start ()
    {
        // 더미카드 셋팅
	    for(int i = 0; i < CardList.Count; i++)
        {
            CardList[i].CardPosSetting(new Vector2(StartXPos + (X_Gap * i), CardList[i].CardPos.anchoredPosition.y));
        }
	}   

    // 혁명관련 함수들...
    public void RevolutionActive(bool GraterRevolution)
    {
        RevolButtonObj.SetActive(true);
        if (!GraterRevolution)
        {
            RevolText.text = "혁명";
            RevolText2.text = m_revolText1;
        }
        else
        {
            RevolText.text = "대혁명";
            RevolText2.text = m_revolText2;
        }
    }
    public void RevolutionClick()
    {
        if (MyInfo.MyRank != GameManager.instance.PlayerCount)
        {
            if (OptionManager.Instance.Netgame)
                GameManager.instance.MultiRevolutionStart(false);

            GameManager.instance.Revolution = true;
        }
        else
        {
            if (OptionManager.Instance.Netgame)
                GameManager.instance.MultiRevolutionStart(true);

            GameManager.instance.SuperRevolution = true;
        }
    }
    // .... 혁명관련

    public void OnlyCardDelete()
    {
        for (int i = 0; i < CardList.Count; i++)
        {
            CardList[i].CardNumSetting(0);
            CardList[i].CardObj.SetActive(false);
        }

        CurrentCardCount = 0;
    }
    public void OnlyCardDeselect(bool Isjoker = false)
    {
        for (int i = 0; i < CardList.Count; i++)
        {
            if(!Isjoker)
                CardList[i].CardDeselectEvent();
            else
            {
                if(CardList[i].Cardnum != 13)
                    CardList[i].CardDeselectEvent();
            }
        }
    }
    public void SelectCardNum(byte num)
    {
        byte count = 0;
        byte joker = 0;

        CurrentCardStat = MyCardStat.Popup;

        for (int i = 0; i < CardList.Count; i++)
        {
            if (CardList[i].Cardnum == num)
            {
                if (GameManager.instance.StartCardCount == 0)
                {
                    CardList[i].CardSelectEvent();
                    count++;
                }
                else if(GameManager.instance.StartCardCount != 0 && count < GameManager.instance.StartCardCount)
                {
                    CardList[i].CardSelectEvent();
                    count++;
                }
                else
                {
                    CardList[i].CardDeselectEvent();
                }
            }
            // 조커 예외처리
            else if (CardList[i].Cardnum == 13 && GameManager.instance.StartCardCount != 1)
            {
                if (count >= GameManager.instance.StartCardCount)
                {
                    if(GameManager.instance.StartCardCount > 2)
                        CardList[i].CardSelectReset();
                }
                else
                {
                    CardList[i].CardSelectEvent();
                    count++;
                    joker++;
                }
            }
            else
                CardList[i].CardDeselectEvent();
        }
    }
    public void SelectCardReset()
    {
        for (int i = 0; i < CardList.Count; i++)
        {
            if (GameManager.instance.IsLastCardNum(CardList[i].Cardnum) &&
                GameManager.instance.IsStartCardCount(CardNumCount(CardList[i].Cardnum)))
                CardList[i].CardSelectReset();
            else
                CardList[i].CardDeselectEvent();
        }
        CurrentCardStat = MyCardStat.Normal;
    }
    public byte CardNumCount(byte num, bool Isjoker = true)
    {
        byte count = 0;

        for (int i = 0; i < CardList.Count; i++)
        {
            if (CardList[i].Cardnum == num)
                count++;
            else if (CardList[i].Cardnum == 13 && Isjoker)
                count++;
        }

        return count;
    }

    //161021 추가..
    //카드를 내기위해 준비칸에 셋팅할때
    public void PopupCardReadySetting()
    {
        byte cardnum = 0;
        byte cardcount = 0;
        byte jokercount = 0;

        for(int i = 0; i < CardList.Count; i++)
        {
            if (!CardList[i].CardObj.activeSelf)
                break;

            if(CardList[i].GetCardStat() == CardInfomation.CardStat.Select &&
               CardList[i].CardObj.activeSelf)
            {
                if(CardList[i].Cardnum != 13)
                {
                    if (cardnum == 0)
                        cardnum = CardList[i].Cardnum;                    

                    cardcount++;
                }
                else
                {
                    if (cardnum == 0)
                        cardnum = CardList[i].Cardnum;

                    jokercount++;
                }
            }
        }

        for(int i = 0; i < cardcount; i++)
        {
            CardRemove(cardnum);
        }
        for (int i = 0; i < jokercount; i++)
        {
            CardRemove(13);
        }

        ReadyCardSetting.instance.CardNumSetting(cardnum, (byte)(cardcount + jokercount), jokercount, true);
    }
    //세금을 내기위해 준비칸에 셋팅할때
    public void TaxPopupCardReadySetting()
    {
        Stack<byte> numlist = new Stack<byte>();

        for (int i = 0; i < CardList.Count; i++)
        {
            if (!CardList[i].CardObj.activeSelf)
                break;

            if (CardList[i].GetCardStat() == CardInfomation.CardStat.Select &&
                CardList[i].CardObj.activeSelf)
            {
                numlist.Push(CardList[i].Cardnum);
            }
        }

        while (numlist.Count != 0)
        {
            byte num = numlist.Pop();

            ReadyCardSetting.instance.TaxCardSetting(num);
            CardRemove(num);
        }

        ReadyCardSetting.instance.IsReady = true;
    }
    // 현재 팝업된 카드를 취소할때
    public void CardSetDeselect(byte cardnum, int deselectcount)
    {
        int count = 0;
        for(int i = CardList.Count - 1; i >= 0; i--)
        {
            if (count >= deselectcount)
                break;

            if (!CardList[i].CardObj.activeSelf)
                continue;

            if(CardList[i].Cardnum == cardnum && CardList[i].GetCardStat() == CardInfomation.CardStat.Select)
            {
                CardList[i].CardSelectReset();
                count++;
            }
        }
    }
    public byte GetLastSelectCardnum(bool IsJoker)
    {
        for (int i = CardList.Count - 1; i >= 0; i--)
        {
            if (!CardList[i].CardObj.activeSelf)
                continue;

            if (!IsJoker)
            {
                if (CardList[i].GetCardStat() == CardInfomation.CardStat.Select &&
                    CardList[i].Cardnum != 13)
                {
                    return CardList[i].Cardnum;
                }
            }
            else
            {
                if (CardList[i].GetCardStat() == CardInfomation.CardStat.Select)
                {
                    return CardList[i].Cardnum;
                }
            }         
        }

        return 0;
    }
    public void LastCardSelectEvent(byte cardnum)
    {
        for(int i = 0; i < CardList.Count; i++)
        {
            if (CardList[i].Cardnum == cardnum &&
                CardList[i].CardObj.activeSelf &&
                CardList[i].GetCardStat() == CardInfomation.CardStat.Normal)
            {
                CardList[i].CardSelectEvent();
                break;
            }
        }
    }
    public byte GetSelectCardCount()
    {
        byte count = 0;

        for (int i = 0; i < CardList.Count; i++)
        {
            if (CardList[i].GetCardStat() == CardInfomation.CardStat.Select && CardList[i].CardObj.activeSelf)
                count++;
        }

        return count;
    }
    //..161021

    public void CardRemove(byte num)
    {
        // 인포 내부의 카드리스트에서 카드를 삭제
        MyInfo.RemoveCard(num);

        for (int i = 0; i < CurrentCardCount; i++)
        {
            if(CardList[i].Cardnum == num || CardList[i].Cardnum == 13)
            {
                for (int z = i; z < CurrentCardCount; z++)
                {
                    if (z < CurrentCardCount - 1)
                    {
                        CardList[z].CardNumSetting(CardList[z + 1].Cardnum);

                        if (CardList[z].Cardnum != num && CardList[z].Cardnum != 13)
                        {
                            CardList[z].CardDeselectEvent();
                        }
                        else
                        {
                            CardList[z].CardSelectEvent();
                        }
                    }
                    else if (z == CurrentCardCount - 1)
                    {
                        CardList[z].CardNumSetting(0);
                        CardList[z].CardObj.SetActive(false);
                    }
                }

                CurrentCardCount--;

                break;
            }
        }
    }
    public void CardAdd(byte num)
    {
        if (CurrentCardCount + 1 > CardList.Count)
            return;

        MyInfo.AddCard(num);
        
        for (int i = CurrentCardCount - 1; i >= 0; i--)
        {
            if (CardList[i].Cardnum <= num)
            {
                i++;

                byte temp = CardList[i].Cardnum;
                for (int z = i; z < CurrentCardCount + 1; z++)
                {
                    if (z == i)
                    {
                        CardList[z].CardNumSetting(num);
                    }
                    else
                    {
                        byte temp2 = CardList[z].Cardnum;
                        CardList[z].CardNumSetting(temp);
                        temp = temp2;
                    }
                }
                CardList[CurrentCardCount].CardObj.SetActive(true);
                CurrentCardCount++;

                break;
            }

            if(i == 0)
            {
                byte temp = CardList[i].Cardnum;
                for (int z = i; z < CurrentCardCount + 1; z++)
                {
                    if (z == i)
                    {
                        CardList[z].CardNumSetting(num);
                    }
                    else
                    {
                        byte temp2 = CardList[z].Cardnum;
                        CardList[z].CardNumSetting(temp);
                        temp = temp2;
                    }
                }
                CardList[CurrentCardCount].CardObj.SetActive(true);
                CurrentCardCount++;

                break;
            }
        }

        SelectCardReset();
    }
    public void CardSetting(List<byte> cardlist, PlayerInfomation info)
    {
        MyInfo = info;
        GameManager.instance.MainPlayerSetting(info);

        if (cardlist.Count <= CardList.Count)
        {
            for (int i = 0; i < CardList.Count; i++)
            {
                if (i < cardlist.Count)
                {
                    CardList[i].CardObj.SetActive(true);
                    CardList[i].CardNumSetting(cardlist[i]);
                }
                else
                    CardList[i].CardObj.SetActive(false);
            }
        }
        else
        {
            // 만약 더미카드숫자보다 받아온 카드리스트의 숫자가 더 많을때 나타나는 에러
            Debug.LogError("List Count Not Match!!");
        }

        CurrentCardCount = cardlist.Count;

        SelectCardReset();
    }    
}