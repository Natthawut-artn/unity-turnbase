using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockCamHUD : IUIElement
{
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
        if (_selfGameObject == null)
        {
            _selfGameObject = GameObject.Instantiate
            (
                Resources.Load<GameObject>("Prefabs/UI/HUDs/LockCamHUD")
            );
            _selfGameObject.transform.SetParent(CanvasManager.Canvas1.transform, false);
        }   
    }
}
