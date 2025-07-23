using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
namespace KCY_Accounting.Logic;
public static class MessageBox
{
    public static async Task ShowInfo(string title, string message)
    {
        var msgBox = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
        {
            ButtonDefinitions = ButtonEnum.Ok,
            Icon = Icon.Info,
            ContentTitle = title,
            ContentMessage = message,
        });

        await msgBox.ShowAsync();
    }

    public static async Task<bool> ShowYesNo(string title, string message)
    {
        var msgBox = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
        {
            ButtonDefinitions = ButtonEnum.YesNo,
            Icon = Icon.Question,
            ContentTitle = title,
            ContentMessage = message,
        });

        var result = await msgBox.ShowAsync();
        return result == ButtonResult.Yes;
    }

    public static async Task ShowError(string title, string message)
    {
        var msgBox = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
        {
            ButtonDefinitions = ButtonEnum.Ok,
            Icon = Icon.Error,
            ContentTitle = title,
            ContentMessage = message,
        });

        await msgBox.ShowAsync();
    }
}