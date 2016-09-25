-	Inherit ObsoleteAttribute

-	Load the bftg polygons to test performance.

-	Lots of duplicated code between DoMerge/DoInverseMerge

-	The coordinate system is wrong. Fourth quadrant should be positive.

-	Writing to .haumea files

-	Units.cs should not handle selection

-	Maybe DialogManager should be split into view/model

-	Minimap

-	Input is a psuedo-singleton

-	Textfield keybinds should be IsActive(), but needs an initial cd so it's easy to just do it once.

-	Handle focus of Textfield independently from focus of dialog.

-	Shaderprogramming: Replace basic effect with a custom one. Needs noise masking for province,
	and color masking to indicate realm.

-	Triangulation: Triangulator got an annoying licens, it might be good to reimplement it.
	It's based on https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf,
	which seems to be easy to understand.

-	Triangulation should be done at compile time. Should have a binary format for triangulated data.

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
-	VertexBuffer and IndexBuffer should be used in the future

-	Vector2 has reference based methods to avoid allocation, might be useful in the future.

-	I shouldn't really use int for primitive indices, but then I have to change Triangulator as well.