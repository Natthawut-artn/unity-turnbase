using UnityEngine;
using DelegateManager;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class BrokenRelicInstPopup : IUIElement
{
    public class Data
    {
        public int BrokenRelicIndex;
        public List<Delegate.CallBack> Callbacks = new List<Delegate.CallBack>();
        public Delegate.CallBack Destroy;
        public Data(int brokenrelicIndex, List<Delegate.CallBack> callBacks , Delegate.CallBack destroy)
        {
            BrokenRelicIndex = brokenrelicIndex;
            Callbacks = callBacks;
            Destroy = destroy;
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
                Resources.Load<GameObject>("Prefabs/UI/Popups/BRelicInstPopup")
            );
            _selfGameObject.transform.SetParent(CanvasManager.Canvas3.transform, false);
        }

        RelicData relicData = BrokenRelicManager.BrokenRelicPocket[data.BrokenRelicIndex];
        GameObject relicdata = _selfGameObject.transform.Find("RelicData").gameObject;
        Image image = relicdata.transform.Find("Relic").GetComponent<Image>();
        image.sprite = relicData.RelicSprite;
        TextMeshProUGUI text = relicdata.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
        text.SetText(relicData.GetInfo());

        Button close = _selfGameObject.transform.Find("Close").GetComponent<Button>();
        close.onClick.AddListener(() =>
        {
            SFXManager.Instance.PlayOneShot(SFX.Cancel);
            Hide();
        });

        List<Button> InstallEachPlayer = new List<Button>();
        GameObject Players = _selfGameObject.transform.Find("Players").gameObject;
        List<GameObject> players = new List<GameObject>();
        for (int i = 0; i < Players.transform.childCount; i++)
        {
            GameObject player = Players.transform.GetChild(i).gameObject;
            Button button = player.transform.Find("Button").GetComponent<Button>();
            players.Add(player);
            InstallEachPlayer.Add(button);
        }
        for (int i = 0; i < players.Count; i++)
        {
            int index = i;
            InstallEachPlayer[index].onClick.AddListener(() =>
            {
                SFXManager.Instance.PlayOneShot(SFX.Hammer);
                data.Callbacks[index]?.Invoke();
                ServiceLocator.Get<BrokenRelicPocket>().Update();
                Hide();
            });
        }
        
        Button destroy = _selfGameObject.transform.Find("Destroy").GetComponent<Button>();
        destroy.onClick.AddListener(() =>
        {
            SFXManager.Instance.PlayOneShot(SFX.Cancel);
            data.Destroy?.Invoke();
            ServiceLocator.Get<BrokenRelicPocket>().Update();
            Hide();
        });
    }
}