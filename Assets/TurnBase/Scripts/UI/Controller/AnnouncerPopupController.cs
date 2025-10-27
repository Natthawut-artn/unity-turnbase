using UnityEngine;

public static class AnnouncerPopupController
{
    public static void Show(string text = null , Sprite sprite = null)
    {
        AnnouncerPopup.Data data = new AnnouncerPopup.Data(text, sprite);
        ServiceLocator.Get<AnnouncerPopup>().Show(data);
    }
    
    public static void Hide()
    {
        ServiceLocator.Get<AnnouncerPopup>().Hide();
    }
}