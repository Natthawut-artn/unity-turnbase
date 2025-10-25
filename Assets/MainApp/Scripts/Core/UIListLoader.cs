using System.Collections.Generic;

public class UIListLoader
{
    public List<object> HUDs { get; private set; } = new List<object>()
    {
        new SettingHUD(),
        new HpBarsHUD(),
        new PlayerOptionHUD(),
        new PlayersInfoHUD(),
        new PocketHUD(),
        new SkillPointsHUD()
    };
    public List<object> PopUps { get; private set; } = new List<object>()
    {
        new StartGamePopup(),
        new SettingPopup(),
        new PlayersInfoPopup(),
        new RelicPocketPopup(),
        new RelicInstPopup(),
        new RelicUnInstPopup()
    };
    public List<object> Pages { get; private set; } = new List<object>()
    {
        
    };

    private readonly List<IUIElement> _uiElements = new();

    public UIListLoader(UIManager uiManager)
    {
        HUDs.Add(uiManager);
    }

    public void RegisterAll()
    {
        RegisterList(HUDs);
        RegisterList(PopUps);
        RegisterList(Pages);

        foreach (var element in _uiElements)
        {
            element.Init();
        }
    }

    private void RegisterList(List<object> list)
    {
        foreach (var item in list)
        {
            ServiceLocator.Register(item);

            if (item is IUIElement element)
                _uiElements.Add(element);
        }
    }
}
