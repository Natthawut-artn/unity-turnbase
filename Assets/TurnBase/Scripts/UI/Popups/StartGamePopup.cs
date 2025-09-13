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
        public Data(Delegate.CallBack StarGameCallBack)
        {
            this.StarGameCallBack = StarGameCallBack;
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
            Resources.Load<GameObject>("Prefabs/UI/Popups/StartGamePopup")
        );
        _selfGameObject.transform.SetParent(CanvasManager.Canvas2.transform, false);

        Button Swift = _selfGameObject.GetComponent<Button>();
        Swift.onClick.AddListener(() =>
        {
            data.StarGameCallBack?.Invoke();
        });
    }
}
