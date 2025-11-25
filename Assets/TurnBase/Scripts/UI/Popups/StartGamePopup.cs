using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DelegateManager;

public class StartGamePopup : IUIElement
{
    public class Data
    {
        public Delegate.CallBack StarGameCallBack;
        public string Coverpath = "";
        public string Bosspath = "";
        public Data(Delegate.CallBack StarGameCallBack, string coverpath, string bosspath)
        {
            this.StarGameCallBack = StarGameCallBack;
            Coverpath = coverpath;
            Bosspath = bosspath;
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
        if (_selfGameObject == null)
        {
            _selfGameObject = GameObject.Instantiate
            (
                Resources.Load<GameObject>("Prefabs/UI/Popups/StartGamePopup")
            );
            _selfGameObject.transform.SetParent(CanvasManager.Canvas2.transform, false);
        }
        
        Button Swift = _selfGameObject.transform.Find("Startgame").GetComponent<Button>();
        Swift.onClick.AddListener(() =>
        {
            SFXManager.Instance.PlayOneShot(SFX.Hammer);
            data.StarGameCallBack?.Invoke();
        });

        Button hub = _selfGameObject.transform.Find("Hub").GetComponent<Button>();
        hub.onClick.AddListener(() =>
        {
            SFXManager.Instance.PlayOneShot(SFX.Cancel);
            Hide();
        });

        Image image = _selfGameObject.GetComponent<Image>();
        image.sprite = Resources.Load<Sprite>(data.Coverpath);
        Image boss = _selfGameObject.transform.Find("Image").GetComponent<Image>();
        boss.sprite = Resources.Load<Sprite>(data.Bosspath);
    }
}
