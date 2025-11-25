using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelegateManager;
using UnityEngine.UI;

public class PocketHUD : IUIElement
{
    public class Data
    {
        public Delegate.CallBack ShowRelicsPoketPopup;
        public Delegate.CallBack ShowBrokenRelicPocket;
        public Data(Delegate.CallBack showRelicsPocketPopup , Delegate.CallBack showBrokenRelicPocket)
        {
            ShowRelicsPoketPopup = showRelicsPocketPopup;
            ShowBrokenRelicPocket = showBrokenRelicPocket;
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
                Resources.Load<GameObject>("Prefabs/UI/HUDs/PocketHUD")
            );
            _selfGameObject.transform.SetParent(CanvasManager.Canvas1.transform, false);
        }
    
        Button button = _selfGameObject.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        
        if (AppStateManager.appState == AppState.Hub)
        {
            button.onClick.AddListener(() =>
            {
                SFXManager.Instance.PlayOneShot(SFX.Press1);
                data.ShowBrokenRelicPocket?.Invoke();
            });
        }
        else
        {
            button.onClick.AddListener(() =>
            {
                SFXManager.Instance.PlayOneShot(SFX.Press1);
                data.ShowRelicsPoketPopup?.Invoke();
            });
        }
        
    }
}

