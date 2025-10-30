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
    private const string Sicfiboss = "Graphic2D/Boss/Mother";
    private const string Victotyboss = "Graphic2D/Boss/Eternal Night";
    private const string Medievalboss = "Graphic2D/Boss/Zenos";

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
        if (GameManager.Instance.CurrentStage.name.Contains("Sci-fi"))
        {
            Debug.Log("L");
            boss.sprite = Resources.Load<Sprite>(Sicfiboss);
        }
        else if (GameManager.Instance.CurrentStage.name.Contains("Victoty"))
        {
            Debug.Log("L");
            boss.sprite = Resources.Load<Sprite>(Victotyboss);
        }
        else if (GameManager.Instance.CurrentStage.name.Contains("Medieval"))
        {
            Debug.Log("L");
            boss.sprite = Resources.Load<Sprite>(Medievalboss);
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
