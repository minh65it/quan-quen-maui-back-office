using System;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.Services;

public partial class AgoraService : IAgoraService
{
    public event Action<string>? OnStatusChanged;

    public void Initialize(string appId)
        => PlatformInitialize(appId);

    public void JoinChannel(string token, string channel, uint uid)
        => PlatformJoinChannel(token, channel, uid);

    public void LeaveChannel()
        => PlatformLeaveChannel();

    public void Mute(bool mute)
        => PlatformMute(mute);

    public void SetSpeaker(bool enabled)
        => PlatformSetSpeaker(enabled);

    partial void PlatformInitialize(string appId);
    partial void PlatformJoinChannel(string token, string channel, uint uid);
    partial void PlatformLeaveChannel();
    partial void PlatformMute(bool mute);
    partial void PlatformSetSpeaker(bool enabled);
}