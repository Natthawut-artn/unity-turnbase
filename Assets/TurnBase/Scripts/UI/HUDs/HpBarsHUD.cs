using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelegateManager;
using UnityEngine.UI;

public class HpBarsHUD : IUIElement
{
    public class Data
    {
        public List<float> PlayersCurrentHp;
        public Data(List<float> data)
        {
            PlayersCurrentHp = data;
        }
    }

    public Data data;
    public List<GameObject> HpBars = new List<GameObject>();

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
        HpBars.Clear();
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
        if (_selfGameObject == null)
        {
            _selfGameObject = GameObject.Instantiate
            (
                Resources.Load<GameObject>("Prefabs/UI/HUDs/HpBarsHUD")
            );
            _selfGameObject.transform.SetParent(CanvasManager.Canvas1.transform, false);
        }
        

        for (int i = 0; i < _selfGameObject.transform.childCount; i++)
        {
            HpBars.Add(_selfGameObject.transform.GetChild(i).gameObject);
            _selfGameObject.transform.GetChild(i).gameObject.SetActive(false);
        }
        List<GameObject> Players = GameManager.Instance.Players_alive;
        foreach (GameObject HpBarViwe in HpBars)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].name == HpBarViwe.name + "(Clone)")
                {
                    HpBarViwe.SetActive(true);
                    Slider HpBar = HpBarViwe.GetComponent<Slider>();
                    HpBar.value = data.PlayersCurrentHp[i];
                }
            }
        }
    }

    public void Update(List<float> newdata)
    {
        List<GameObject> Players = GameManager.Instance.Players_alive;
        foreach (GameObject HpBarViwe in HpBars)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].name == HpBarViwe.name + "(Clone)")
                {
                    Slider HpBar = HpBarViwe.GetComponent<Slider>();
                    HpBar.value = newdata[i];
                }
            }
        }
    }
}

