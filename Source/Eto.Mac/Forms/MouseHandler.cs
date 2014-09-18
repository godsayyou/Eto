using System;
using Eto.Forms;
#if XAMMAC2
using AppKit;
using Foundation;
using CoreGraphics;
using ObjCRuntime;
using CoreAnimation;
#else
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.CoreGraphics;
using MonoMac.ObjCRuntime;
using MonoMac.CoreAnimation;
#endif

namespace Eto.Mac.Forms
{
	public class MouseHandler : Mouse.IHandler
	{
		public void Initialize ()
		{
		}
		public Widget Widget { get; set; }

		public Eto.Platform Platform { get; set; }

		public Eto.Drawing.PointF Position
		{
			get
			{
				var mouseLocation = NSEvent.CurrentMouseLocation;
				var origin = NSScreen.Screens[0].Frame.Bottom;
				mouseLocation.Y = origin - mouseLocation.Y;
				return mouseLocation.ToEto();
			}
		}

		public MouseButtons Buttons
		{
			get
			{
				var current = NSEvent.CurrentPressedMouseButtons;
				var buttons = MouseButtons.None;
				if ((current & 1) != 0)
					buttons |= MouseButtons.Primary;
				if ((current & 2) != 0)
					buttons |= MouseButtons.Alternate;
				if ((current & 4) != 0)
					buttons |= MouseButtons.Middle;
				return buttons;
			}
		}
	}
}

