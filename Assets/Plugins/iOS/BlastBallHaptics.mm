#import <UIKit/UIKit.h>

// HapticType: 1 = Light, 2 = Medium, 3 = Heavy, 4 = Success
extern "C" void BlastBall_Haptic(int type)
{
    if (@available(iOS 10.0, *))
    {
        if (type == 4)
        {
            UINotificationFeedbackGenerator *generator = [[UINotificationFeedbackGenerator alloc] init];
            [generator notificationOccurred:UINotificationFeedbackTypeSuccess];
            return;
        }

        UIImpactFeedbackStyle style = UIImpactFeedbackStyleLight;
        if (type == 2) style = UIImpactFeedbackStyleMedium;
        else if (type == 3) style = UIImpactFeedbackStyleHeavy;

        UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:style];
        [generator impactOccurred];
    }
}
