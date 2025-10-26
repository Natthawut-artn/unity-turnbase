using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnHUD : IUIElement
{
    public class Data
    {
        
    }
    public Data data;
    public const string E = "Graphic2D/Turn_E";
    public const string P = "Graphic2D/Turn_P";

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
                Resources.Load<GameObject>("Prefabs/UI/HUDs/TurnHUD")
            );
            _selfGameObject.transform.SetParent(CanvasManager.Canvas1.transform, false);
        }

        Image image = _selfGameObject.GetComponent<Image>();
        GameObject player = MainCamera.Instance.Player.gameObject;
        CharactorInheritance charactordata = player.GetComponent<CharactorInheritance>();
        if (charactordata.heroclass == HeroClass.Enemy || charactordata.heroclass == HeroClass.Boss)
        {
            image.sprite = Resources.Load<Sprite>(E);
        }
        else
        {
            image.sprite = Resources.Load<Sprite>(P);
        }
    }
}
