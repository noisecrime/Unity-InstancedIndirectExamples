# Unity-InstancedIndirectExamples
Exploring Unity 5.6 InstanceIndirect Method to render large numbers of meshes.

This is a Unity Project for Unity 5.6 ( at time of writing in beta )


InstancedIndirectExample
A slightly expanded version of the examples from https://docs.unity3d.com/560/Documentation/ScriptReference/Graphics.DrawMeshInstancedIndirect.html

Fixes a few shader and script issues ( see https://forum.unity3d.com/threads/drawmeshinstancedindirect-example-comments-and-questions.446080/) as well as providing the actual Unity project with some important set up such as;

Shadows disabled on directional light - you can enable them, but it will naturally affect performance.
Shadow Quality changed to 'No Cascades' since each cascade requires rendering the instances again, thus affecting performance.
Camera set to 'Deferred' Rendering mode so multiple lights do not result in additional passes.



InstancedIndirectSelectionExample
This is a quick and dirty proof of concept to an anwser I made in this thread
https://forum.unity3d.com/threads/most-efficient-way-to-display-large-scatter-plots.403702/#post-2885620

The OP needed to render potentially millions of objects and be able to pick/select them on screen. 

Trying to use Unity PhysX for this would naturally grind any machine to a halt, if only due to having to have each object represented by a gameObject in order to apply a collider to it. A custom raycast solution using a spatial partitioning scheme ( octree or KD-Tree ) would probably work and might even be faster than the solution presented here as long as the scene is completely static as it could be pre-calculated. However writing such a system from scratch would be time consuming.

The suggestion presented was an old-skool method for picking, where by you render the scene to off-screen buffer ( RenderTexture ) and give each object an ID via a unqiue colour. In simple terms imagine having 255 objects you could assign each a value as part of the red component in a RGBA texture, then you could read back the renderTexture, get the red value and convert it back into an ID.

It turned out a little more tricky than that, but as a proof-of-concept it appears to work. It can handle up to 16,777,214 unique objects, which is far more than could be rendered efficently currently. 

Minimal testing and checking has been performed, but it appears to return correct ID's with the few simple tests I made. Personally I'd want to test it further before relying on it, by which I mean the concpet works, just not 100% sure it always gives the correct ID's.


