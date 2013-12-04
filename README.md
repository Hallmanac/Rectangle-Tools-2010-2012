Rectangle-Tools-2010-2012
=========================

Add-in to Autodesk Inventor which provides a suite of symmetrical rectangle sketching tools.

********************************
INVENTOR PRODUCTIVITY TOOLS    
____________________________

Author:
        QUBE-IT DESIGN, INC
        Brian Hall
        8/22/2010
********************************

DESCRIPTION
_______________

V3(Current)

This tool set has four distinct rectangle drawing tools that are used inside the sketch
environment for part files, assembly files, and drawing files.  They are located on a 
separate ribbon panel called "Rectangles" on the "Sketch" ribbon tab. They are as follows:

Center Point Rectangle (default displayed button):
        This rectangle tool draws a rectangle about the origin of where the user picks.  It
        constrains the rectangle by adding a vertical and horizontal alignment constraint to
        one of the vertical and horizontal lines respectively as well as a coincident constraint
        to the geometry that the user selects.  If no geometry is selected, then only the vertical
        and horizontal alignment constraints will be used.
Diagonal Center Point Rectangle:
        This rectangle tool draws a rectangle about the origin of where the user picks and adds
        a diagonal line that goes from one corner to the opposite corner with a sketch point constrained
        to the mid-point of the diagonal line.  By virtue of the diagonal line, the rectangle relationship 
        to the origin is maintained, but if the user selects existing geometry then a coincident
        constraint is added to the origin sketch point and selected geometry.
Horizontal Mid-Point Rectangle:
        This rectangle tool draws a rectangle that eminates from the mid-point of one of the 
        horizontal lines of the rectangle.  A sketch point is then added and constrained to 
        that line's mid-point.  If the user selects existing geometry, then the sketch point
        is constrained to the existing geometry accordingly.
Vertical Mid-Point Rectangle:
        This rectangle tool draws a rectangle that eminates from the mid-point of one of the 
        vertical lines of the rectangle.  A sketch point is then added and constrained to 
        that line's mid-point.  If the user selects existing geometry, then the sketch point
        is constrained to the existing geometry accordingly.

There is also the ability to configure the user interface for this add-in.  You may use the original
drop-down control to access the commands or you may switch to a panel style layout.  To configure the
user interface simply go to the application menu (the big "I" in the upper left corner of Inventor) and
choose the "Rectangle Tools Settings" command and it will bring up a dialog box with two radio buttons
which allow you to set the desired user interface for these commands. This will require a restart of 
Inventor for the change to take effect.


SOURCE CODE
_________________

This is an open source project.

Source code was written in C# and is provided at https://github.com/Qube-it/Rectangle-Tools-2010-2012

SUPPORTED VERSIONS
______________________

This add-in supports Inventor 2010 forward and does not support the classic
interface. 


INSTALLATION
________________ 

An installer wizard ("Qube-It Tools.msi") has been provided. Since this add-in was
built using .NET 3.5 framework, you will need that framework to run the add-in.  The
installer will automatically detect this and install it if you don't have it. Follow 
the prompts for quick installation.  You can change the location of the install folder 
if you like.

You may uninstall the add-in through your system's control panel. You may also simply
uncheck the "Load on Startup" box if you're only looking to unload it without uninstalling it.


KNOWN ISSUES
_________________

This add-in does not support the classic interface by design. 

The default rectangle button shown on the Rectangles tab is the Center point Rectangle.
When the user selects one of the other rectangle tools, they will remain displayed
and "pressed" during the life of the tool, but when the user exits the tool or starts another
tool, the displayed button will go back to being the Center Point Rectangle tool again.  
This is intentional because even if the other tools are displayed (i.e. horizontal or 
vertical mid-point rectangle tools) and the user clicks that displayed button, the center point rectangle tool 
will be what gets activated (unless the user selects the drop down first). 

There is no support for the heads up display to be utilized with these tools. It is not 
supported through the API yet.


BUG FIXES
__________________
R2 fixes:
                        1) Added support for the construction line toggle.
                        2) Added the Diagonal Center Point Rectangle tool to provide for a more robust
                           rectangle so that there are no unexpected results when the corners are filleted 
                           or chamfered.
                        3) Changed the behavior of the tool so that it acts in a manner that is consistent with 
                           the behavior of the current Inventor rectangle tools where the command stays 
                           active until the user escapes out of the command or starts a new command.

R3 fixes:
                        1) Added support for a keyboard shortcut to TERMINATE a running rectangle command
                        2) Added the ability for a user to configure the user interface.


RELEASE HISTORY
__________________

R1                                                 7/4/2010
R2                                                 7/11/2010
R3 (current release)                                8/22/2011


FEEDBACK
________________

Brian@Qube-it.com
or post online to  http://www.mcadforums.com/forums/posting.php?mode=edit&f=34&p=78891

***************************************************************************************

Permission to use, copy, modify, and distribute this software in
object code form for any purpose and without fee is hereby granted, 
provided that the above copyright notice appears in all copies and 
that both that copyright notice and the limited warranty and
restricted rights notice below appear in all supporting 
documentation.  This add-in may be freely distributed and installed
on all properly licensed installations of Autodesk Inventor.

QUBE-IT DESIGN PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
QUBE-IT DESIGN SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  QUBE-IT DESIGN, INC. 
DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
UNINTERRUPTED OR ERROR FREE.
