//
//  macOSXAppDelegate.h
//  macOSX
//
//  Created by Julio Jerez on 11/23/12.
//  Copyright 2012 __MyCompanyName__. All rights reserved.
//

#import <Cocoa/Cocoa.h>

@interface macOSXAppDelegate : NSObject <NSApplicationDelegate> 
{
    NSWindow *window;
}

@property (assign) IBOutlet NSWindow *window;

@end
