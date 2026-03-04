using Android.Content;
using Com.Agora.Rtc;
using QuanQuenBackOfficeMobile.Interfaces;

namespace QuanQuenBackOfficeMobile.Droid.Services
{
    public class AgoraService : Java.Lang.Object, IAgoraService, IRtcEngineEventHandler
    {
        private RtcEngine _engine;

        public void Initialize(string appId)
        {
            _engine = RtcEngine.Create(Android.App.Application.Context, appId, this);
            _engine.EnableAudio();
        }

        public void JoinChannel(string token, string channelName, uint uid)
        {
            _engine.JoinChannel(token, channelName, "", uid);
        }

        public void LeaveChannel()
        {
            _engine.LeaveChannel();
            RtcEngine.Destroy();
        }

        public override void OnJoinChannelSuccess(string channel, int uid, int elapsed)
        {
            System.Diagnostics.Debug.WriteLine($"Joined channel {channel} with UID {uid}");
        }

        public override void OnUserJoined(int uid, int elapsed)
        {
            System.Diagnostics.Debug.WriteLine($"User {uid} joined");
        }

        public override void OnUserOffline(int uid, int reason)
        {
            System.Diagnostics.Debug.WriteLine($"User {uid} left");
        }
    }
}