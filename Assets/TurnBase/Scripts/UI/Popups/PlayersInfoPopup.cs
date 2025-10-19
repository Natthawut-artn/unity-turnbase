using UnityEngine;
using DelegateManager;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PlayersInfoPopup : IUIElement
{
    public class Data
    {
        public Delegate.CallBack Close;
        public Data(Delegate.CallBack close)
        {
            Close = close;
        }
    }
    public Data data;

    public GameObject SelfGameObject
    {
        get { return _selfGameObject; }
    }

    private GameObject _selfGameObject;
    private IUIManager _uiManager;

    public void Init()
    {
        _uiManager = ServiceLocator.Get<IUIManager>();
        ServiceLocator.Register(this);
        _uiManager.Register(this);
    }

    public void Dispose()
    {

    }

    public void Hide()
    {
        if (_selfGameObject == null)
            return;

        _selfGameObject.SetActive(false);
        Object.Destroy(_selfGameObject);
        _selfGameObject = null;
    }

    public void Show()
    {
        Show(new object());
    }

    public void Show(object rawData)
    {
        data = (Data)rawData;
        _selfGameObject = GameObject.Instantiate
          (
              Resources.Load<GameObject>("Prefabs/UI/Popups/PlayersInfoPopup")
        );
        _selfGameObject.transform.SetParent(CanvasManager.Canvas3.transform, false);

        Button CloseBtn = _selfGameObject.transform.Find("Close").GetComponent<Button>();
        CloseBtn.onClick.AddListener(() =>
        {
            data.Close?.Invoke();
        });
        GameObject Buttons = _selfGameObject.transform.Find("Buttons").gameObject;
        List<Button> buttons = new List<Button>();
        for (int i = 0; i < Buttons.transform.childCount; i++)
        {
            Button button = Buttons.transform.GetChild(i).GetComponent<Button>();
            buttons.Add(button);
        }
        GameObject Infos = _selfGameObject.transform.Find("Infos").gameObject;
        List<GameObject> infos = new List<GameObject>();
        for (int i = 0; i < Infos.transform.childCount; i++)
        {
            GameObject gameObject = Infos.transform.GetChild(i).gameObject;
            infos.Add(gameObject);
        }
        buttons[0].onClick.AddListener(() =>
        {
            ShowInfoPage(infos[0], infos[1]);
        });
        buttons[1].onClick.AddListener(() =>
        {
            ShowStatusePage(infos[0], infos[1]);
        });
    }
    
    public void ShowInfoPage(GameObject infoPage, GameObject statusPage)
    {
        infoPage.SetActive(true);
        statusPage.SetActive(false);
    }

    public void ShowStatusePage(GameObject infoPage, GameObject statusPage)
    {
        infoPage.SetActive(false);
        statusPage.SetActive(true);

        List<GameObject> Statuses = new List<GameObject>();
        for (int i = 0; i < statusPage.transform.childCount; i++)
        {
            Statuses.Add(statusPage.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < GameManager.Instance.Players_alive.Count; i++)
        {
            if (Statuses[i].name + "(Clone)" == GameManager.Instance.Players_alive[i].name)
            {
                Debug.Log(Statuses[i].name);
                PlayerData playerData = GameManager.Instance.Players_alive[i].GetComponent<PlayerData>();
                string status =
                $"Hp : {playerData.currentHP}\n" +
                $"ATK : {playerData.currentATK}\n" +
                $"Defense : {playerData.currentDefense}\n" +
                $"Crit Rate : {playerData.currentCritRate}\n" +
                $"Crit DGM : {playerData.currentCritDGM}\n"
                ;

                TextMeshProUGUI text = Statuses[i].transform.Find("Text (TMP) (2)").GetComponent<TextMeshProUGUI>();
                if (text == null)
                {
                    Debug.Log("Null");
                }
                else
                {
                    text.SetText(status);
                }
            }
        }
        for (int i = GameManager.Instance.Players_alive.Count; i < Statuses.Count; i++)
        {
            Statuses[i].SetActive(false);
        }
    }
}