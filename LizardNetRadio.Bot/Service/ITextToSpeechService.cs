namespace LizardNetRadio.Bot.Service;

public interface ITextToSpeechService
{
    Task<Uri> SpeakAsync(string text);
}