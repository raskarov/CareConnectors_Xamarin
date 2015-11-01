// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace Fit
{
	[Register ("ProfileViewController")]
	partial class ProfileViewController
	{
		[Outlet]
		UIKit.UILabel ageHeightValueLabel { get; set; }

		[Outlet]
		UIKit.UILabel ageUnitLabel { get; set; }

		[Outlet]
		UIKit.UILabel heightUnitLabel { get; set; }

		[Outlet]
		UIKit.UILabel heightValueLabel { get; set; }

		[Outlet]
		UIKit.UILabel weightUnitLabel { get; set; }

		[Outlet]
		UIKit.UILabel weightValueLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton Send { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (Send != null) {
				Send.Dispose ();
				Send = null;
			}
		}
	}
}
