-	Inherit ObsoleteAttribute

-	Load the bftg polygons to test performance.

-	Lots of duplicated code between DoMerge/DoInverseMerge

-	The coordinate system is wrong. Fourth quadrant should be positive.

-	Writing to .haumea files

-	Units.cs should not handle selection

-	Maybe DialogManager should be split into view/model

-	Minimap

-	Input is a psuedo-singleton

Guidelines
--------------------------------
-	Views should be named [name]View.cs. If the view is the primary representation of a single model,
	the format [modelname]View.cs should be used.

-	No exceptions except for `NotSupportedException` and `NotImplementedException`. In those cases,
	also use the `ObsoleteAttribute` (when possible) with the crash-flag set to `true`.
	See SortedList for example of both.

	In most cases, the `Try...` pattern can directly replace exceptions, and in the cases where the exception
	indicates wrong code usage, use Debug.Assert (but note that it is ignored in the release build).

Notes
--------------------------------
-	When implenenting text input for dialogs, Game.Window.TextInput should be used.

-	Vector2 has reference based methods to avoid allocation, might be useful in the future.