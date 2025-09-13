using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBarHUD : IUIElement
{
    public class Data
    {
        public float currentHP;
        public Data(float currentHP)
        {
            this.currentHP = currentHP;
        }
    }
    public Data data;
    public Slider HPBar;

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
              Resources.Load<GameObject>("Prefabs/UI/HUDs/HPBarHUD")
        );
        _selfGameObject.transform.SetParent(CanvasManager.Canvas1.transform, false);

        HPBar = _selfGameObject.GetComponent<Slider>();
        HPBar.value = data.currentHP;
    }

    public void UpProgression(float currentHP)
    {
        HPBar.value = currentHP;
    }
}
