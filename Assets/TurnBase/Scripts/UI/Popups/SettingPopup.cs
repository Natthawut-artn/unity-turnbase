using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelegateManager;
using UnityEngine.UI;

public class SettingPopup : IUIElement
{
    public class Data
    {
        public Delegate.CallBack ConfirmCallBack;
        public Data(Delegate.CallBack ConfirmCallBack)
        {
            this.ConfirmCallBack = ConfirmCallBack;
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
              Resources.Load<GameObject>("Prefabs/UI/Popups/SettingPopup")
        );
        _selfGameObject.transform.SetParent(CanvasManager.Canvas3.transform, false);

        Button close = _selfGameObject.transform.Find("Close").GetComponent<Button>();
        close.onClick.AddListener(() =>
        {
            Hide();
        });
        
        Button confirm = _selfGameObject.transform.Find("Confirm").GetComponent<Button>();
        confirm.onClick.AddListener(() =>
        {
            data.ConfirmCallBack?.Invoke();
        });

        if (AppStateManager.appState == AppState.Hub)
        {
            confirm.interactable = false;    
        }
    }
}
