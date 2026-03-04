#if IOS
using System.Runtime.InteropServices;

namespace QuanQuenBackOfficeMobile.Platforms.iOS
{
    internal static class AgoraiOSBridge
    {
        [DllImport("__Internal")]
        internal static extern void initialize(string appId);

        [DllImport("__Internal")]
        internal static extern void joinChannel(string token, string channel, uint uid);

        [DllImport("__Internal")]
        internal static extern void leaveChannel();
    }
}
#endif