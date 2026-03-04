#if ANDROID
using Android.Content;
using Com.Agora.Rtc;
using Microsoft.Maui.ApplicationModel;

public class AgoraService : Java.Lang.Object, IAgoraService, IRtcEngineEventHandler
{
    RtcEngine _engine;

    public void Initialize(string appId)
    {
        _engine = RtcEngine.Create(
            Android.App.Application.Context,
            appId,
            this
        );

        _engine.SetChannelProfile(Constants.ChannelProfileCommunication);
        _engine.SetClientRole(Constants.ClientRoleBroadcaster);
        _engine.EnableAudio();
    }

    public void JoinChannel(string token, string channel, uint uid)
    {
        _engine.JoinChannel(token, channel, "", uid);
    }

    public void LeaveChannel()
    {
        _engine?.LeaveChannel();
        RtcEngine.Destroy();
        _engine = null;
    }

    public void Mute(bool mute)
    {
        _engine?.MuteLocalAudioStream(mute);
    }

    public void SetSpeaker(bool enabled)
    {
        _engine?.SetEnableSpeakerphone(enabled);
    }

    public override void OnJoinChannelSuccess(string channel, int uid, int elapsed)
    {
        System.Diagnostics.Debug.WriteLine("Joined");
    }
}
#endif