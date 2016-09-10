-	Revamp the stupid Haumea_Core.FormsUI. It's shit, and I should feel bad.

-   Engine should know less about XNA specifics.

-	Extend the file format to allow complex polygons, and multiple outlines for one province.

-	Inherit ObsoleteAttribute

-	Convert more code to System.Linq

-	Make a more sensible system for printing debug info on screen. Probably add more methods to Debug.

-	Load the bftg polygons to test performance.

-	Lots of duplicated code between DoMerge/DoInverseMerge

-	The separtion between Model/View needs to improve. 
	Examples of currently odd usage:
		Units, not UnitsViews keeps track of selected armies.
		Provinces, not ProvincesView keeps track MouseOver etc.
	The model should not care about the interface at all. It __only__ cares about data.

-	The coordinate system is wrong. Fourth quadrant should be positive.

Guidelines
--------------------------------
-	Views should be named [name]View.cs. If the view is the primary representation of a single model,
	the format [modelname]View.cs should be used.

-	No exceptions except for `NotSupportedException` and `NotImplementedException`. In those cases,
	also use the `ObsoleteAttribute` (when possible) with the crash-flag set to `true`.

	In most cases, the `Try...` pattern can directly replace exceptions, and in the cases where the exception
	indicates wrong code usage, use Debug.Assert (but note that it is ignored in the release build).