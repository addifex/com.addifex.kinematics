# Unity package for controlling kinematic GameObjects

Designed to be a universal kinematic solution beyond just character controllers.
While Unity physics is amazing, it isn't always the best use case when specific behaviours or collisions are required.
Using physics results in slippery or sluggish characters controls and indeterministic game mechanics. However, Unity doesn't
provide much for kinematic functionality. This package aims to provide a set of components that provide most of the functionality someone
would need anywhere from character controls to puzzle interactions. Beyond that, the code is meant to be written in a manner that decouples logic from
MonoBehaviors, as to allow high re-use of kinematic logic in custom components.

## Installation
To add package to Unity project:
1. Open Package Manager window
2. Click the "+" icon in top left corner
3. Select "Install package from git URL..."
4. Copy and Paste: https://github.com/addifex/com.addifex.kinematics.git
5. Click "Install"

## Known Bugs

### (#001) Jitter on low objects.
When player continuously moves into a low, blocking object there is a noticeable camera jitter.

Expected behavior: No camera jitter when moving into blocking object.

### (#002) Jitter on complex collisions.
Camera jitters when player continuously moves into two or more blocking objects at once (i.e. a corner).

Expected behavior: No camera jitter when moving into multiple blocking objects.

### (#003) Sticky player movement. 
In tight, complex areas the character can end up getting blocked by moving forward,
but they become stuck when trying to move backwards. 
However, if they move left or right, they become unstuck.

Expected behavior: Player should be able to move out of a collision using the inverse of the movement that got them into the collision.

## In Progress
- Bug-free character controller prototype.
- Player rotation controls.
- Character controllers for sphere and box colliders.

## Future
- Object pushing.
- Dynamic ground following.
