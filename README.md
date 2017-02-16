# spline-controller-test
Tests with Spline creation and traversal in Unity

After trying out various free solutions for Spline creation and traversal in Unity, I decided to make my own - partly because I wasn't satisfied with them, and partly because after finding some examples, I thought it would be a fun challenge. "Spliner" is the result. It uses [tiagosr's Spline Section](https://gist.github.com/tiagosr/11361023) as a foundation for mathematical functions and very basic implementation concepts, and expands on it dramatically with many additional features. 

I don't intend for this to become a massively featured project - I really just need the ability to draw and traverse splines.

###Features

* Single Component design - Unlike many free Spline solutions I've seen, with Spliner there is no need for Empty GameObjects as reference points. You can create a resizeable array of Spline Sections straight inside the Spliner component Inspector.
* Automatic Positional Continuity - Spline segments can be automatically joined in 3D space, or left as separate segments.
* Automatic Tangent Alignment - Adjacent spline segments can automatically align their tangents, or the user can manually edit tangents for each segment.
* Methods to flatten the Spline to the X, Y, or Z axis.
* Editor Scene View GUI - In addition to positioning control points in the Inspector, there are also Scene View GUI Handles for points and tangents, with full Undo/Redo support for changes.

Interpolation is not yet fully implemented; stay tuned!
