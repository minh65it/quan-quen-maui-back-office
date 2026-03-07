#if IOS
using AVFoundation;
using Foundation;
using AgoraRtcKit;

namespace QuanQuenBackOfficeMobile.Services;

public partial class AgoraService
{
    private AgoraRtcEngineKit? _engine;

    partial void PlatformInitialize(string appId)
    {
        ConfigureAudioSession();

        _engine = AgoraRtcEngineKit.SharedEngineWithAppId(
            appId,
            new AgoraDelegate(this));

        _engine.SetChannelProfile(ChannelProfile.Communication);
        _engine.SetClientRole(ClientRole.Broadcaster);
        _engine.EnableAudio();
    }

    partial void PlatformJoinChannel(string token, string channel, uint uid)
    {
        _engine?.JoinChannelByToken(
            token,
            channel,
            null,
            uid,
            (ch, id, elapsed) =>
            {
                IsJoined = true;
                OnStatusChanged?.Invoke("Connected");
            });
    }

    partial void PlatformLeaveChannel()
    {
        _engine?.LeaveChannel(null);
        OnStatusChanged?.Invoke("Left");
    }

    partial void PlatformMute(bool mute)
    {
        _engine?.MuteLocalAudioStream(mute);
    }

    partial void PlatformSetSpeaker(bool enabled)
    {
        _engine?.SetEnableSpeakerphone(enabled);
    }

    private void ConfigureAudioSession()
    {
        var session = AVAudioSession.SharedInstance();

        session.SetCategory(
            AVAudioSessionCategory.PlayAndRecord,
            AVAudioSessionCategoryOptions.AllowBluetooth |
            AVAudioSessionCategoryOptions.DefaultToSpeaker);

        session.SetMode(AVAudioSession.ModeVoiceChat);
        session.SetActive(true);
    }

    private class AgoraDelegate : AgoraRtcEngineDelegate
    {
        private readonly AgoraService _service;

        public AgoraDelegate(AgoraService service)
        {
            _service = service;
        }

        public override void DidOccurError(
            AgoraRtcEngineKit engine,
            ErrorCode errorCode)
        {
            _service.OnStatusChanged?.Invoke($"Error: {errorCode}");
        }

        public override void DidJoinChannel(
            AgoraRtcEngineKit engine,
            string channel,
            nuint uid,
            nint elapsed)
        {
            _service.OnStatusChanged?.Invoke("Joined Channel");
        }

        public override void DidOfflineOfUid(
            AgoraRtcEngineKit engine,
            nuint uid,
            UserOfflineReason reason)
        {
            _service.OnStatusChanged?.Invoke("Remote user left");
        }
    }
}
#endif