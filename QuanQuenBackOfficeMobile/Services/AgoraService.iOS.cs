#if IOS
public class AgoraService : IAgoraService
{
    public void Initialize(string appId)
        => AgoraiOSBridge.initialize(appId);

    public void JoinChannel(string token, string channel, uint uid)
        => AgoraiOSBridge.joinChannel(token, channel, uid);

    public void LeaveChannel()
        => AgoraiOSBridge.leaveChannel();

    public void Mute(bool mute)
        => AgoraiOSBridge.mute(mute);

    public void SetSpeaker(bool enabled)
        => AgoraiOSBridge.speaker(enabled);
}
#endif