using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultPopup : IUIElement
{
    public GameObject SelfGameObject
    {
        get { return _selfGameObject; }
    }

    private GameObject _selfGameObject;
    private IUIManager _uiManager;
    private const string mother = "Graphic2D/Boss/Motherart";
    private const string eternal = "Graphic2D/Boss/EternalNight-ART";
    private const string zenos = "Graphic2D/Boss/Zenos";

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
        _selfGameObject = GameObject.Instantiate
          (
              Resources.Load<GameObject>("Prefabs/UI/Popups/ResultPopup")
        );
        _selfGameObject.transform.SetParent(CanvasManager.Canvas3.transform, false);

        Image boss = _selfGameObject.transform.Find("Panels/Boss/Image (1)").GetComponent<Image>();
        if (AppStateManager.theme == Theme.Scifi)
        {
            boss.sprite = Resources.Load<Sprite>(mother);
        }
        else if (AppStateManager.theme == Theme.Victoria)
        {
            boss.sprite = Resources.Load<Sprite>(eternal);
        }
        else if(AppStateManager.theme == Theme.Medieval)
        {
            boss.sprite = Resources.Load<Sprite>(zenos);
        }

        Image reward = _selfGameObject.transform.Find("Panels/Reward/Image").GetComponent<Image>();
        int index = BrokenRelicManager.BrokenRelicPocket.Count - 1;
        RelicData brokenrelic = BrokenRelicManager.BrokenRelicPocket[index];
        reward.sprite = brokenrelic.RelicSprite;

        Button hub = _selfGameObject.transform.Find("Button").GetComponent<Button>();
        hub.onClick.AddListener(() =>
        {
            GameManager.Instance.BackToTheHub();
            Hide();
        });
    }
}
