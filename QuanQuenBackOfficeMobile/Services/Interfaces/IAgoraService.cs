public interface IAgoraService
{
    void Initialize(string appId);
    void JoinChannel(string token, string channelName, uint uid);
    void LeaveChannel();

    void Mute(bool mute);
    void SetSpeaker(bool enable);

    event Action<string> OnStatusChanged;
}