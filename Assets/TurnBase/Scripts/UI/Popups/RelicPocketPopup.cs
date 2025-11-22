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
        public Delegate.CallBackWithInt InstallRelic;
        public Data(Delegate.CallBack close, Delegate.CallBackWithInt installRelic)
        {
            InstallRelic = installRelic;
            Close = close;
        }
    }
    public Data data;
    public List<GameObject> pocket = new List<GameObject>();
    private const string Emthy_Path = "Graphic2D/Relic_BoxIn";

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
        pocket.Clear();
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
                Resources.Load<GameObject>("Prefabs/UI/Popups/RelicPocketPopup")
            );
            _selfGameObject.transform.SetParent(CanvasManager.Canvas3.transform, false);
        }
        
        Button closebtn = _selfGameObject.transform.Find("Close").GetComponent<Button>();
        closebtn.onClick.AddListener(() =>
        {
            data.Close?.Invoke();
        });
        List<RelicData> relics = RelicStroageManager.RelicsInUserPocket;
        GameObject Pocket = _selfGameObject.transform.Find("RelicList/Contents").gameObject;
        for (int i = 0; i < Pocket.transform.childCount; i++)
        {
            pocket.Add(Pocket.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < relics.Count; i++)
        {
            Sprite sprite = relics[i].RelicSprite;
            Image image = pocket[i].transform.Find("Image").GetComponent<Image>();
            image.sprite = sprite;
        }

        for (int i = 0; i < pocket.Count; i++)
        {
            int index = i;
            Button button = pocket[i].GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                data.InstallRelic.Invoke(index);
            });
        }
    }

    public void Update()
    {
        if (_selfGameObject == null) return;
        List<RelicData> relics = RelicStroageManager.RelicsInUserPocket;
        Debug.Log(relics.Count);
        for (int i = 0; i < pocket.Count; i++)
        {
            Image image = pocket[i].transform.Find("Image").GetComponent<Image>();
            if (i < relics.Count)
            {
                image.sprite = relics[i].RelicSprite;
                image.enabled = true;
            }
            else
            {
                image.sprite = Resources.Load<Sprite>(Emthy_Path);
            }
        }
    }
}

