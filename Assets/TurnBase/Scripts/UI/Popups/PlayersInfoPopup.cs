using UnityEngine;
using DelegateManager;
using UnityEngine.UI;
using System.Collections.Generic;

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
            infos[0].SetActive(true);
            infos[1].SetActive(false);
        });
        buttons[1].onClick.AddListener(() =>
        {
            infos[0].SetActive(false);
            infos[1].SetActive(true);
        });
    }
}