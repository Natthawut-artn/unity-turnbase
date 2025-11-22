using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnnouncerPopup : IUIElement
{
    public class Data
    {
        public string text;
        public Sprite sprite;
        public Data(string text, Sprite sprite)
        {
            this.text = text;
            this.sprite = sprite;
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
                Resources.Load<GameObject>("Prefabs/UI/Popups/AnnouncerPopup")
            );
            _selfGameObject.transform.SetParent(CanvasManager.Canvas3.transform, false);
        }

        Button close = _selfGameObject.transform.Find("Close").GetComponent<Button>();
        close.onClick.AddListener(() =>
        {
            Hide();
        });

        string text = data?.text;
        Sprite sprite = data.sprite;

        GameObject box = _selfGameObject.transform.Find("Box").gameObject;
        TextMeshProUGUI AnnounceTxt = box.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
        Image AnnounceImg = box.transform.Find("Image").GetComponent<Image>();

        AnnounceTxt.text = text;
        AnnounceImg.sprite = sprite;

        if (text == null || text == "") AnnounceTxt.gameObject.SetActive(false);
        if (sprite == null) AnnounceImg.gameObject.SetActive(false);
    }
}
