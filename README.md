# spline-controller-test
Tests with Spline creation and traversal in Unity

After trying out various free solutions for Spline creation and traversal in Unity, I decided to make my own - partly because I wasn't satisfied with them, and partly because after finding some examples, I thought it would be a fun challenge. "Spliner" is the result. It uses [tiagosr's Spline Section](https://gist.github.com/tiagosr/11361023) as a foundation for mathematical functions and very basic implementation concepts, and expands on it dramatically with many additional features. 

I don't intend for this to become a massively featured project - I really just need the ability to draw and traverse splines.

###Features

#####SplinerBase

* Single Component design - Unlike many free Spline solutions I've seen, with Spliner there is no need for Empty GameObjects as reference points. You can create a resizeable array of Spline Sections straight inside the Spliner component Inspector.
* Automatic Positional Continuity - Spline segments can be automatically joined in 3D space, or left as separate segments.
* Automatic Tangent Alignment - Adjacent spline segments can automatically align their tangents, or the user can manually edit tangents for each segment.
* Methods to flatten the Spline to the X, Y, or Z axis.
* Editor Scene View GUI - In addition to positioning control points in the Inspector, there are also Scene View GUI Handles for points and tangents, with full Undo/Redo support for changes.
* Logical Continuity - Even if Spline Segments are not positionally continuous, they can still be logically continuous for Interpolation.

#####SplinerInterpolator
* Multiple modes of operation:
  * Forward Only - Standard 0...1 interpolation in one direction only.
  * Ping Pong - Turns around when it reaches either endpoint, continuing indefinitely.
  * Count - Turns around when it reaches either endpoint, until a desired number of turns has been reached.
  * Cycle - Teleports back to the start point when it reaches the endpoint, continuing indefinitely.
  * Cycle Count - A combination of Cycle and Count modes; performs a desired number of Cycles and then stops.
  * Manual - Control from script using a simple Evaluate() method that expects a 0...1 float input.
* Forward and Reverse Operation
* Variable Speed - A target duration can be specified in seconds or in frames, for framerate-dependent or framerate-independent applications.
* Align to Spline - The Interpolator's Transform can automatically align its rotation with the Spline, if desired. The user has control over how far the sampled "look target" is from the current point along the spline.
* Bounds and Precision - The Interpolator doesn't necessarily need to walk the entire Spline; the user can define a start and end point manually as a 0...1 fraction. The precision for a "turn" event can also be manually controlled.
* Preview - The user can 'walk' the Interpolator along the Spline in the Scene View to test the current settings.
* Optimized - The Interpolator allocates 0B additional memory at runtime, and in modes where movement stops (Forward Only, Count, and Cycle Count) it performs no computation once the action is finished. 

*I discovered Catlike Coding's awesome tutorial about spline systems in Unity only after I was about 90% done with this project, unfortunately... would have saved me a great deal of effort! [Check it out here if you haven't already seen it](http://catlikecoding.com/unity/tutorials/curves-and-splines/)
