import Foundation
import AgoraRtcKit

@objc(AgoraWrapper)
public class AgoraWrapper: NSObject {

    static var engine: AgoraRtcEngineKit?

    @objc public static func initialize(_ appId: String) {
        engine = AgoraRtcEngineKit.sharedEngine(withAppId: appId, delegate: nil)
        engine?.setChannelProfile(.communication)
        engine?.setClientRole(.broadcaster)
        engine?.enableAudio()
    }

    @objc public static func joinChannel(
        _ token: String,
        channel: String,
        uid: UInt
    ) {
        engine?.joinChannel(
            byToken: token,
            channelId: channel,
            info: nil,
            uid: uid,
            joinSuccess: nil
        )
    }

    @objc public static func leaveChannel() {
        engine?.leaveChannel(nil)
        AgoraRtcEngineKit.destroy()
        engine = nil
    }
}