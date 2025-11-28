using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenGame : IUIElement
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
        _selfGameObject = GameObject.Instantiate
          (
              Resources.Load<GameObject>("Prefabs/UI/Pages/OpenGame")
        );
        _selfGameObject.transform.SetParent(CanvasManager.Canvas4.transform, false);

        Button hub = _selfGameObject.transform.Find("startgame").GetComponent<Button>();
        hub.onClick.AddListener(() =>
        {
            SFXManager.Instance.PlayOneShot(SFX.Press1);
            SFXManager.Instance.PlayMusic(Music.Hub);
            PocketHUDController.Show();
            AppStateManager.ChangeAppState(AppState.Hub);
            Hide();
        });
        Button set = _selfGameObject.transform.Find("Setting").GetComponent<Button>();
        set.onClick.AddListener(() =>
        {
            SFXManager.Instance.PlayOneShot(SFX.Press1);
            SettingPopupController.Show();
        });
        Button exit = _selfGameObject.transform.Find("Exit").GetComponent<Button>();
        set.onClick.AddListener(() =>
        {
            SFXManager.Instance.PlayOneShot(SFX.Press1);
            Application.Quit();
        });
    }
}
