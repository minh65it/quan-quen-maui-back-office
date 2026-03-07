#if ANDROID
using Android.App;
using IO.Agora.Rtc2;
using IO.Agora.Rtc2.Constants;

namespace QuanQuenBackOfficeMobile.Services;

public partial class AgoraService 
{
    private RtcEngine? _engine;
    private AgoraEventHandler? _handler;

    public partial void Initialize(string appId)
    {
        _handler = new AgoraEventHandler(this);

        var config = new RtcEngineConfig
        {
            Context = Application.Context,
            AppId = appId,
            EventHandler = _handler
        };

        _engine = RtcEngine.Create(config);

        // 🔥 Voice optimized settings
        _engine.SetChannelProfile(Constants.ChannelProfileCommunication);
        _engine.SetClientRole(Constants.ClientRoleBroadcaster);
        _engine.EnableAudio();
        _engine.DisableVideo();
        _engine.SetAudioProfile(
            Constants.AudioProfileMusicHighQuality,
            Constants.AudioScenarioGameStreaming
        );

        RaiseStatus("Android Voice Engine Ready");
    }

    public partial void JoinChannel(string token, string channel, uint uid)
    {
        _engine?.JoinChannel(token, channel, "", uid);
    }

    public partial void LeaveChannel()
    {
        _engine?.LeaveChannel();
        RaiseStatus("Call Ended");
    }

    public partial void Mute(bool mute)
    {
        _engine?.MuteLocalAudioStream(mute);
        RaiseStatus(mute ? "Mic Muted" : "Mic Unmuted");
    }

    public partial void SetSpeaker(bool enabled)
    {
        _engine?.SetEnableSpeakerphone(enabled);
        RaiseStatus(enabled ? "Speaker On" : "Speaker Off");
    }
}

internal class AgoraEventHandler : IRtcEngineEventHandler
{
    private readonly AgoraService _service;

    public AgoraEventHandler(AgoraService service)
    {
        _service = service;
    }

    public override void OnJoinChannelSuccess(string channel, int uid, int elapsed)
    {
        _service.OnStatusChanged?.Invoke($"Joined: {channel}");
    }

    public override void OnUserJoined(int uid, int elapsed)
    {
        _service.OnStatusChanged?.Invoke($"User Joined: {uid}");
    }

    public override void OnUserOffline(int uid, int reason)
    {
        _service.OnStatusChanged?.Invoke($"User Left: {uid}");
    }

    public override void OnError(int err)
    {
        _service.OnStatusChanged?.Invoke($"Error: {err}");
    }
}
#endif