using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelegateManager;
using UnityEngine.UI;

public class RelicPocketPopup : IUIElement
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
              Resources.Load<GameObject>("Prefabs/UI/Popups/RelicPocketPopup")
        );
        _selfGameObject.transform.SetParent(CanvasManager.Canvas1.transform, false);

        Button closebtn = _selfGameObject.transform.Find("Close").GetComponent<Button>();
        closebtn.onClick.AddListener(() =>
        {
            data.Close?.Invoke();
        });
        List<RelicData> relics = RelicStroageManager.RelicsInUserPocket;
        List<GameObject> pocket = new List<GameObject>();
        GameObject Pocket = _selfGameObject.transform.Find("RelicList").gameObject;
        for (int i = 0; i < Pocket.transform.childCount; i++)
        {
            pocket.Add(Pocket.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < relics.Count; i++)
        {
            Sprite sprite = relics[i].RelicSprite;
            Image image = pocket[i].GetComponent<Image>();
            image.sprite = sprite;
        }
    }

    public void Update()
    {
        List<RelicData> relics = RelicStroageManager.RelicsInUserPocket;
        List<GameObject> pocket = new List<GameObject>();
        GameObject Pocket = _selfGameObject.transform.Find("RelicList").gameObject;
        for (int i = 0; i < Pocket.transform.childCount; i++)
        {
            pocket.Add(Pocket.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < relics.Count; i++)
        {
            Sprite sprite = relics[i].RelicSprite;
            Image image = pocket[i].GetComponent<Image>();
            image.sprite = sprite;
        }
    }
}

