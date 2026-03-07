namespace QuanQuenBackOfficeMobile.Services;

public interface IAgoraService
{
    void Initialize(string appId);
    void JoinChannel(string token, string channel, uint uid);
    void LeaveChannel();

    void Mute(bool mute);
    void SetSpeaker(bool enabled);

    event Action<string>? OnStatusChanged;
}