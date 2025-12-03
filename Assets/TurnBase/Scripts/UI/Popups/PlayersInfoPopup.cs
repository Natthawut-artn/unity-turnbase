using UnityEngine;
using DelegateManager;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class PlayersInfoPopup : IUIElement
{
    public class Data
    {
        public Delegate.CallBack Close;
        public Delegate.CallBackWith2Int CallBackWith2Ints;
        public Data(Delegate.CallBack close,Delegate.CallBackWith2Int callBackWith2Ints)
        {
            CallBackWith2Ints = callBackWith2Ints;
            Close = close;
        }
    }
    public Data data;
    private const string Emthy_Path = "Graphic2D/Profile_Relic_Slot";
    private List<GameObject> infos;

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
        // infos.Clear();
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
            _selfGameObject = GameObject.Instantiate(
                Resources.Load<GameObject>("Prefabs/UI/Popups/PlayersInfoPopup")
            );
            _selfGameObject.transform.SetParent(CanvasManager.Canvas3.transform, false);
        }

        Button closeBtn = _selfGameObject.transform.Find("Close").GetComponent<Button>();
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() =>
        {
            SFXManager.Instance.PlayOneShot(SFX.Cancel);
            data.Close?.Invoke();
        });

        GameObject infos = _selfGameObject.transform.Find("Infos").gameObject;
        List<GameObject> pages = new List<GameObject>();
        for (int i = 0; i < infos.transform.childCount; i++)
            pages.Add(infos.transform.GetChild(i).gameObject);

        GameObject buttonsObj = _selfGameObject.transform.Find("Buttons").gameObject;
        List<Button> buttons = new List<Button>();
        for (int i = 0; i < buttonsObj.transform.childCount; i++)
            buttons.Add(buttonsObj.transform.GetChild(i).GetComponent<Button>());

        buttons[0].onClick.RemoveAllListeners();
        buttons[0].onClick.AddListener(() =>
        {
            SFXManager.Instance.PlayOneShot(SFX.Press1);
            ShowInfoPage(pages[0], pages[1]);
        });

        buttons[1].onClick.RemoveAllListeners();
        buttons[1].onClick.AddListener(() =>
        {
            SFXManager.Instance.PlayOneShot(SFX.Press1);
            ShowStatusePage(pages[0], pages[1]);
        });

        ShowInfoPage(pages[0], pages[1]);
    }
        
    public void ShowInfoPage(GameObject infoPage, GameObject statusPage)
    {
        infoPage.SetActive(true);
        statusPage.SetActive(false);

        // หา PlayerSlot ทั้งหมด
        List<GameObject> slots = new List<GameObject>();
        for (int i = 0; i < infoPage.transform.childCount; i++)
            slots.Add(infoPage.transform.GetChild(i).gameObject);

        for (int i = 0; i < slots.Count; i++)
        {
            GameObject uiSlot = slots[i];
            string slotName = uiSlot.name;

            GameObject player = GameManager.Instance.Players_alive
                .FirstOrDefault(p => p.name.StartsWith(slotName));

            if (player == null)
            {
                uiSlot.SetActive(false);
                continue;
            }

            uiSlot.SetActive(true);

            // Update Relics
            Transform pocket = uiSlot.transform.Find("Relics");
            var relics = player.GetComponent<RelicsStoage>().RelicsInPocket;

            for (int j = 0; j < pocket.childCount; j++)
            {
                Transform child = pocket.GetChild(j);
                Image img = child.Find("Image").GetComponent<Image>();

                if (j < relics.Count)
                    img.sprite = relics[j].RelicSprite;
                else
                    img.sprite = Resources.Load<Sprite>(Emthy_Path);

                int slotIndex = i;
                int relicIndex = j;

                Button btn = child.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    data.CallBackWith2Ints?.Invoke(slotIndex, relicIndex);
                });
            }
        }
    }

    public void ShowStatusePage(GameObject infoPage, GameObject statusPage)
    {
        infoPage.SetActive(false);
        statusPage.SetActive(true);

        List<GameObject> slots = new List<GameObject>();
        for (int i = 0; i < statusPage.transform.childCount; i++)
            slots.Add(statusPage.transform.GetChild(i).gameObject);

        for (int i = 0; i < slots.Count; i++)
        {
            GameObject uiSlot = slots[i];
            string slotName = uiSlot.name;

            GameObject player = GameManager.Instance.Players_alive
                .FirstOrDefault(p => p.name.StartsWith(slotName));

            if (player == null)
            {
                uiSlot.SetActive(false);
                continue;
            }

            uiSlot.SetActive(true);

            PlayerData pd = player.GetComponent<PlayerData>();

            string status =
                $"Hp : {pd.currentHP}\n" +
                $"ATK : {pd.currentATK}\n" +
                $"Defense : {pd.currentDefense}\n" +
                $"Crit Rate : {pd.currentCritRate}\n" +
                $"Crit DMG : {pd.currentCritDGM}\n" +  
                $"Level : {pd.LV}\n";  

            TextMeshProUGUI txt = uiSlot.transform
                .Find("Text (TMP) (2)")
                .GetComponent<TextMeshProUGUI>();

            txt.SetText(status);
        }
    }

    public void Update()
    {
        if (_selfGameObject == null) return;
        ShowInfoPage(infos[0],infos[1]);
    }
}