using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelegateManager;
using UnityEngine.UI;
using TMPro;

public class RelicUnInstPopup : IUIElement
{
    public class Data
    {
        public RelicsStoage RelicsStoage;
        public RelicData Relic;
        public Data(RelicData relic, RelicsStoage relicsStoage)
        {
            RelicsStoage = relicsStoage;
            Relic = relic;
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
              Resources.Load<GameObject>("Prefabs/UI/Popups/RelicUnInstPopup")
        );
        _selfGameObject.transform.SetParent(CanvasManager.Canvas3.transform, false);

        RelicsStoage Pocket = data.RelicsStoage;
        RelicData relicData = data.Relic;
        GameObject relicdata = _selfGameObject.transform.Find("RelicData").gameObject;
        Image image = relicdata.transform.Find("Relic").GetComponent<Image>();
        image.sprite = relicData.RelicSprite;
        TextMeshProUGUI text = relicdata.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
        text.SetText(relicData.GetInfo());

        Button close = _selfGameObject.transform.Find("Close").GetComponent<Button>();
        close.onClick.AddListener(() =>
        {
            Hide();
            ServiceLocator.Get<PlayersInfoPopup>().Update();
        });

        Button confirm = _selfGameObject.transform.Find("Confirm").GetComponent<Button>();
        confirm.onClick.AddListener(() =>
        {
            Pocket.RemoveRelic(relicData);
            Hide();
            ServiceLocator.Get<PlayersInfoPopup>().Update();
        });
    }
}
