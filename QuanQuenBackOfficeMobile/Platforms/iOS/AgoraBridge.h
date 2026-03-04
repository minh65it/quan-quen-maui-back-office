#import <Foundation/Foundation.h>

@interface AgoraWrapper : NSObject

+ (void)initialize:(NSString *)appId;
+ (void)joinChannel:(NSString *)token channel:(NSString *)channel uid:(unsigned int)uid;
+ (void)leaveChannel;

@end