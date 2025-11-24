using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelegateManager;
using UnityEngine.UI;
using System.Linq;

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
        if (_selfGameObject == null)
        {
            _selfGameObject = GameObject.Instantiate
            (
                Resources.Load<GameObject>("Prefabs/UI/Popups/SettingPopup")
            );
            _selfGameObject.transform.SetParent(CanvasManager.Canvas3.transform, false);
        }
        
        Button close = _selfGameObject.transform.Find("Close").GetComponent<Button>();
        close.onClick.AddListener(() =>
        {
            SFXManager.Instance.PlayOneShot(SFX.Cancel);
            Hide();
        });

        Button confirm = _selfGameObject.transform.Find("Confirm").GetComponent<Button>();
        confirm.onClick.AddListener(() =>
        {
            SFXManager.Instance.PlayOneShot(SFX.Press1);
            data.ConfirmCallBack?.Invoke();
        });

        Button tutorial = _selfGameObject.transform.Find("Tutorial").GetComponent<Button>();
        tutorial.onClick.AddListener(() =>
        {
            SFXManager.Instance.PlayOneShot(SFX.Press1);
            ServiceLocator.Get<TutorialPage>().Show();
            SettingHUDController.Hide();
            PocketHUDController.Hide();
            PlayersInfoHUDController.Hide();
            HPBarsHUDController.Hide();
            Hide();
        });

        if (AppStateManager.appState == AppState.Hub)
        {
            confirm.interactable = false;
        }
        
        if (!GameManager.Instance.Players_alive.Any() && AppStateManager.appState == AppState.State)
        {
            close.interactable = false;
        }
    }
}
