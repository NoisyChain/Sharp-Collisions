# Sharp-Collisions
Deterministic collision system written in C#

**Features:**
- 2D/3D collisions and resolution
- Discrete collision detection
- Deterministic behavior using fixed point math
- Broad Phase/Narrow Phase
- Space partitioning for broad phase (quadtree for 2D/ octree for 3D)
- AABB collisions
- Sphere/Capsule collisions using distance checks
- Convex polygon collisions using GJK-EPA algorithms
- Collision layers / Ignore collisions
- Triggers
- Rotation support
- Support for multiple colliders
- Collider offset
- Collision flags
- Collision events
- Collision manifolds (return contact normal, depth and contact point)
- A basic character controller for both 2D and 3D

## Quick Start
### SharpNode
The main node for SharpCollisions. Every important node should inherit from SharpNode if you want it to run in the SharpCollisions' main loop.
```c#
private SharpNode node;
// Always call _Instance() manually when you create any kind of SharpNode to properly include it to the existing SharpManager.
node._Instance();
// Always call _Destroy() to properly dispose your SharpNode.
node._Destroy();
// SharpNode loops
// delta returns SharpTime.DeltaTime;
node._FixedPreProcess(Fix64 delta) {} // Runs before everything
node._FixedProcess(Fix64 delta) {} // Runs between _FixedPreProcess and SharpWorld.Simulate
node._FixedPostProcess(Fix64 delta) {} // Runs after SharpWorld.Simulate
```
### SharpManager
The central class to control all the SharpNodes. It's meant to be an example of how to implement SharpCollisions properly. It should be modified or replicated depending on your needs.
### SharpWorld
The actual physics world. It simulates physics interactions between SharpBodies. You need to include the right SharpWorld depending on your needs (ShawpWorld2D for 2D games, SharpWorld3D for 3D games).
```c#
// Ticks per second
int tps = 60;
// Discrete substeps per tick
int steps = 4;
// Space partition size (quadtree for 2D, octree for 3D)
int areaSize = 10;
// Maximum body count per partition
int maxBodiesPerPartition = 4;
// Set SharpTime values
SharpTime.Set(tps, steps);
// 2D Sharp World;
SharpWorld2D world2d = new SharpWorld2D(areaSize, maxBodiesPerPartition);
// 3D Sharp World
SharpWorld3D world3d = new SharpWorld3D(areaSize, maxBodiesPerPartition);
// Simulate physics worlds
world2d.Simulate();
world3d.Simulate();
```
### SharpBody
The physics body. Always use tih if you want a body with physics behavior. 
```c#
SharpBody body
// Called at the exact moment the body collides with something
public virtual void OnBeginOverlap(CollisionManifold3D collision) {}
// Called every tick the body is colliding with something
public virtual void OnOverlap(CollisionManifold3D collision) {}
// Called at the moment the body stops colliding with something
public virtual void OnEndOverlap(CollisionManifold3D collision) {}
```
### SharpCollider
The colliders are what allows the SharpBody to collide. Every SharpBody needs to have at least 1 collider.
### SharpTime
A static class to control/return every time related variables, like fixed delta time and simulation ticks.

## Third-party Libraries
This repositiry uses third-party libraries:

[FixedMath.NET](https://github.com/asik/FixedMath.Net)

[Debug Draw 3D](https://github.com/DmitriySalnikov/godot_debug_draw_3d)
##
Sharp Collisions is in preview state. There's some things to polish at the moment.

Using Godot 4.6.2 .NET. Unity/Monogame versions are being planned.
