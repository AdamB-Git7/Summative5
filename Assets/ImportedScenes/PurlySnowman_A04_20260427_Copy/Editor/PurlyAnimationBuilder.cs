// Bring in the generic List type used to collect keyframes.
using System.Collections.Generic;
// Bring in Unity editor APIs for asset and menu items.
using UnityEditor;
// Bring in editor animator APIs used to build the controller and states.
using UnityEditor.Animations;
// Bring in core Unity types like AnimationClip and Vector3.
using UnityEngine;

// Place this builder inside its imported-scene editor namespace.
namespace ImportedScenes.PurlySnowman_A04_20260427_Copy.Editor
{
    // Editor-only utility that procedurally rebuilds Purly's animation clips and controller.
    public static class PurlyAnimationBuilder
    {
        // Store the folder where the generated animation assets live.
        private const string RootPath = "Assets/ImportedScenes/PurlySnowman_A04_20260427_Copy/Animations";
        // Store the file path for the generated idle clip.
        private const string IdleClipPath = RootPath + "/Idle.anim";
        // Store the file path for the generated walk-right clip.
        private const string WalkRightClipPath = RootPath + "/Walk.anim";
        // Store the file path for the generated walk-left clip.
        private const string WalkLeftClipPath = RootPath + "/WalkLeft.anim";
        // Store the file path for the generated jump clip.
        private const string JumpClipPath = RootPath + "/Jump.anim";
        // Store the file path for the generated animator controller.
        private const string ControllerPath = RootPath + "/PurlyAnimator.controller";

        // Run this static method automatically when the editor loads.
        [InitializeOnLoadMethod]
        private static void BuildOnLoad()
        {
            // Defer the rebuild until the next editor tick to avoid asset-load conflicts.
            EditorApplication.delayCall += EnsureAnimationsExist;
        }

        // Expose a manual rebuild entry in the Tools menu.
        [MenuItem("Tools/Purly/Rebuild Animations")]
        public static void EnsureAnimationsExist()
        {
            // Skip rebuilding while the editor is entering or already in play mode.
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            // Make sure the editor folder exists before writing into it.
            DirectoryUtility.EnsureFolderExists("Assets/ImportedScenes/PurlySnowman_A04_20260427_Copy/Editor");

            // Build the idle animation clip in memory.
            AnimationClip idle = CreateIdleClip();
            // Build the walk-right animation clip in memory.
            AnimationClip walkRight = CreateWalkRightClip();
            // Build the walk-left animation clip in memory.
            AnimationClip walkLeft = CreateWalkLeftClip();
            // Build the jump animation clip in memory.
            AnimationClip jump = CreateJumpClip();

            // Save the idle clip to disk.
            SaveClip(idle, IdleClipPath);
            // Save the walk-right clip to disk.
            SaveClip(walkRight, WalkRightClipPath);
            // Save the walk-left clip to disk.
            SaveClip(walkLeft, WalkLeftClipPath);
            // Save the jump clip to disk.
            SaveClip(jump, JumpClipPath);

            // Build or refresh the animator controller using the saved clips.
            CreateController(idle, walkRight, walkLeft, jump);
            // Flush pending asset changes to disk.
            AssetDatabase.SaveAssets();
            // Re-import asset entries so changes show up in the editor.
            AssetDatabase.Refresh();
        }

        // Build the looping idle clip.
        private static AnimationClip CreateIdleClip()
        {
            // Create a fresh looping clip 0.8s long named Idle.
            AnimationClip clip = NewClip("Idle", true, 0.8f);

            // Animate the body's vertical position with a subtle bob.
            SetLocalPositionY(clip, "Body_Middle", Keys((0f, -0.257f), (0.4f, -0.235f), (0.8f, -0.257f)));
            // Animate the head's vertical position with a subtle bob.
            SetLocalPositionY(clip, "Head", Keys((0f, 0.473f), (0.4f, 0.505f), (0.8f, 0.473f)));
            // Animate the scarf's vertical position with a subtle bob.
            SetLocalPositionY(clip, "Scarf", Keys((0f, 0.134f), (0.4f, 0.145f), (0.8f, 0.134f)));

            // Hand the finished clip back to the caller.
            return clip;
        }

        // Build the looping walk-right clip.
        private static AnimationClip CreateWalkRightClip()
        {
            // Create a fresh looping clip 0.6s long named Walk.
            AnimationClip clip = NewClip("Walk", true, 0.6f);

            // Animate the left leg's X, Y, and Z positions through the walk cycle.
            SetPosition(clip, "LeftLeg",
                Keys((0f, -0.41045982f), (0.15f, -0.43f), (0.3f, -0.39f), (0.45f, -0.4f), (0.6f, -0.41045982f)),
                Keys((0f, -1.7782948f), (0.15f, -1.72f), (0.3f, -1.66f), (0.45f, -1.75f), (0.6f, -1.7782948f)),
                Keys((0f, -0.05195633f), (0.6f, -0.05195633f)));

            // Animate the right leg's X, Y, and Z positions through the walk cycle.
            SetPosition(clip, "RightLeg",
                Keys((0f, 0.60548025f), (0.15f, 0.62f), (0.3f, 0.56f), (0.45f, 0.58f), (0.6f, 0.60548025f)),
                Keys((0f, -1.8753389f), (0.15f, -1.82f), (0.3f, -1.94f), (0.45f, -1.88f), (0.6f, -1.8753389f)),
                Keys((0f, 0.101387955f), (0.6f, 0.101387955f)));

            // Animate the left hand's X, Y, and Z positions through the walk cycle.
            SetPosition(clip, "LeftHand",
                Keys((0f, -0.83060324f), (0.15f, -0.85f), (0.3f, -0.79f), (0.45f, -0.81f), (0.6f, -0.83060324f)),
                Keys((0f, -0.60908157f), (0.15f, -0.6f), (0.3f, -0.67f), (0.45f, -0.62f), (0.6f, -0.60908157f)),
                Keys((0f, -0.17130628f), (0.6f, -0.17130628f)));

            // Animate the right hand's X, Y, and Z positions through the walk cycle.
            SetPosition(clip, "RightHand",
                Keys((0f, 0.9383498f), (0.15f, 0.97f), (0.3f, 0.91f), (0.45f, 0.94f), (0.6f, 0.9383498f)),
                Keys((0f, -0.62977576f), (0.15f, -0.65f), (0.3f, -0.57f), (0.45f, -0.61f), (0.6f, -0.62977576f)),
                Keys((0f, 0.08835077f), (0.6f, 0.08835077f)));

            // Add an up-and-down body bob during walk.
            SetLocalPositionY(clip, "Body_Middle", Keys((0f, -0.257f), (0.15f, -0.23f), (0.3f, -0.257f), (0.45f, -0.23f), (0.6f, -0.257f)));
            // Add an up-and-down head bob during walk.
            SetLocalPositionY(clip, "Head", Keys((0f, 0.473f), (0.15f, 0.515f), (0.3f, 0.473f), (0.45f, 0.515f), (0.6f, 0.473f)));

            // Hand the finished clip back to the caller.
            return clip;
        }

        // Build the looping walk-left clip (mirror of walk-right timing).
        private static AnimationClip CreateWalkLeftClip()
        {
            // Create a fresh looping clip 0.6s long named WalkLeft.
            AnimationClip clip = NewClip("WalkLeft", true, 0.6f);

            // Animate the left leg using the inverted walk pose order.
            SetPosition(clip, "LeftLeg",
                Keys((0f, -0.39f), (0.15f, -0.4f), (0.3f, -0.41045982f), (0.45f, -0.43f), (0.6f, -0.39f)),
                Keys((0f, -1.66f), (0.15f, -1.75f), (0.3f, -1.7782948f), (0.45f, -1.72f), (0.6f, -1.66f)),
                Keys((0f, -0.05195633f), (0.6f, -0.05195633f)));

            // Animate the right leg using the inverted walk pose order.
            SetPosition(clip, "RightLeg",
                Keys((0f, 0.56f), (0.15f, 0.58f), (0.3f, 0.60548025f), (0.45f, 0.62f), (0.6f, 0.56f)),
                Keys((0f, -1.94f), (0.15f, -1.88f), (0.3f, -1.8753389f), (0.45f, -1.82f), (0.6f, -1.94f)),
                Keys((0f, 0.101387955f), (0.6f, 0.101387955f)));

            // Animate the left hand using the inverted walk pose order.
            SetPosition(clip, "LeftHand",
                Keys((0f, -0.79f), (0.15f, -0.81f), (0.3f, -0.83060324f), (0.45f, -0.85f), (0.6f, -0.79f)),
                Keys((0f, -0.67f), (0.15f, -0.62f), (0.3f, -0.60908157f), (0.45f, -0.6f), (0.6f, -0.67f)),
                Keys((0f, -0.17130628f), (0.6f, -0.17130628f)));

            // Animate the right hand using the inverted walk pose order.
            SetPosition(clip, "RightHand",
                Keys((0f, 0.91f), (0.15f, 0.94f), (0.3f, 0.9383498f), (0.45f, 0.97f), (0.6f, 0.91f)),
                Keys((0f, -0.57f), (0.15f, -0.61f), (0.3f, -0.62977576f), (0.45f, -0.65f), (0.6f, -0.57f)),
                Keys((0f, 0.08835077f), (0.6f, 0.08835077f)));

            // Add the body bob to the walk-left clip.
            SetLocalPositionY(clip, "Body_Middle", Keys((0f, -0.257f), (0.15f, -0.23f), (0.3f, -0.257f), (0.45f, -0.23f), (0.6f, -0.257f)));
            // Add the head bob to the walk-left clip.
            SetLocalPositionY(clip, "Head", Keys((0f, 0.473f), (0.15f, 0.515f), (0.3f, 0.473f), (0.45f, 0.515f), (0.6f, 0.473f)));

            // Hand the finished clip back to the caller.
            return clip;
        }

        // Build the one-shot jump clip.
        private static AnimationClip CreateJumpClip()
        {
            // Create a non-looping clip 0.3s long named Jump.
            AnimationClip clip = NewClip("Jump", false, 0.3f);

            // Animate the right leg's pose through the jump arc.
            SetPosition(clip, "RightLeg",
                Keys((0f, 0.60548025f), (0.08f, 0.69f), (0.18f, 0.56f), (0.3f, 0.60548025f)),
                Keys((0f, -1.8753389f), (0.08f, -2.03f), (0.18f, -1.76f), (0.3f, -1.8753389f)),
                Keys((0f, 0.101387955f), (0.3f, 0.101387955f)));

            // Animate the left leg's pose through the jump arc.
            SetPosition(clip, "LeftLeg",
                Keys((0f, -0.41045982f), (0.08f, -0.44f), (0.18f, -0.39f), (0.3f, -0.41045982f)),
                Keys((0f, -1.7782948f), (0.08f, -1.85f), (0.18f, -1.72f), (0.3f, -1.7782948f)),
                Keys((0f, -0.05195633f), (0.3f, -0.05195633f)));

            // Animate the left hand's pose through the jump arc.
            SetPosition(clip, "LeftHand",
                Keys((0f, -0.83060324f), (0.18f, -0.87f), (0.3f, -0.83060324f)),
                Keys((0f, -0.60908157f), (0.18f, -0.55f), (0.3f, -0.60908157f)),
                Keys((0f, -0.17130628f), (0.3f, -0.17130628f)));

            // Animate the right hand's pose through the jump arc.
            SetPosition(clip, "RightHand",
                Keys((0f, 0.9383498f), (0.18f, 0.98f), (0.3f, 0.9383498f)),
                Keys((0f, -0.62977576f), (0.18f, -0.56f), (0.3f, -0.62977576f)),
                Keys((0f, 0.08835077f), (0.3f, 0.08835077f)));

            // Add a slight upward head bump during the jump.
            SetLocalPositionY(clip, "Head", Keys((0f, 0.473f), (0.18f, 0.54f), (0.3f, 0.473f)));
            // Add a slight upward body bump during the jump.
            SetLocalPositionY(clip, "Body_Middle", Keys((0f, -0.257f), (0.18f, -0.22f), (0.3f, -0.257f)));

            // Hand the finished clip back to the caller.
            return clip;
        }

        // Build or refresh the animator controller and wire each motion state.
        private static void CreateController(AnimationClip idle, AnimationClip walkRight, AnimationClip walkLeft, AnimationClip jump)
        {
            // Try to load the existing controller from disk.
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            // Create a brand-new controller when no existing one was found.
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            }

            // Remove leftover sub-assets that previous runs may have left behind.
            CleanupControllerSubAssets(controller);

            // Clear all controller parameters before reconfiguring.
            controller.parameters = new AnimatorControllerParameter[0];
            // Clear all controller layers before reconfiguring.
            controller.layers = new AnimatorControllerLayer[0];

            // Construct a fresh base layer with its own state machine.
            AnimatorControllerLayer layer = new()
            {
                name = "Base Layer",
                defaultWeight = 1f,
                stateMachine = new AnimatorStateMachine()
            };

            // Attach the new state machine to the controller asset on disk.
            AssetDatabase.AddObjectToAsset(layer.stateMachine, controller);

            // Add the Idle state and bind it to the idle clip.
            AnimatorState idleState = layer.stateMachine.AddState("Idle", new Vector3(60f, 90f, 0f));
            idleState.motion = idle;

            // Add the WalkRight state and bind it to the walk-right clip.
            AnimatorState walkRightState = layer.stateMachine.AddState("WalkRight", new Vector3(220f, 40f, 0f));
            walkRightState.motion = walkRight;

            // Add the WalkLeft state and bind it to the walk-left clip.
            AnimatorState walkLeftState = layer.stateMachine.AddState("WalkLeft", new Vector3(220f, 150f, 0f));
            walkLeftState.motion = walkLeft;

            // Add the Jump state and bind it to the jump clip.
            AnimatorState jumpState = layer.stateMachine.AddState("Jump", new Vector3(360f, 90f, 0f));
            jumpState.motion = jump;

            // Make Idle the default state played on entry.
            layer.stateMachine.defaultState = idleState;
            // Install the configured layer back into the controller.
            controller.layers = new[] { layer };

            // Mark the state machine dirty so changes are saved.
            EditorUtility.SetDirty(layer.stateMachine);
            // Mark the controller dirty so changes are saved.
            EditorUtility.SetDirty(controller);
        }

        // Remove orphan sub-assets that share the controller asset path.
        private static void CleanupControllerSubAssets(AnimatorController controller)
        {
            // Load every asset stored inside the controller file.
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(ControllerPath);

            // Visit each sub-asset one by one.
            foreach (Object asset in assets)
            {
                // Skip null entries and the controller itself.
                if (asset == null || asset == controller)
                {
                    continue;
                }

                // Destroy leftover sub-assets immediately.
                Object.DestroyImmediate(asset, true);
            }
        }

        // Create a fresh clip with shared frame rate and loop settings.
        private static AnimationClip NewClip(string name, bool loop, float stopTime)
        {
            // Allocate a new clip and set its name and frame rate.
            AnimationClip clip = new() { name = name, frameRate = 60f };
            // Read the modifiable settings for the clip.
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            // Apply the looping flag.
            settings.loopTime = loop;
            // Apply the clip's stop time.
            settings.stopTime = stopTime;
            // Write the updated settings back into the clip.
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            // Hand the configured clip back.
            return clip;
        }

        // Save the clip at the given asset path, overwriting any existing copy in place.
        private static void SaveClip(AnimationClip clip, string path)
        {
            // Try to load any existing clip at that path.
            AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            // Create a new asset entry when none exists yet.
            if (existing == null)
            {
                AssetDatabase.CreateAsset(clip, path);
                return;
            }

            // Copy the new clip data into the existing asset so references stay intact.
            EditorUtility.CopySerialized(clip, existing);
            // Mark the existing asset dirty so the change is persisted.
            EditorUtility.SetDirty(existing);
        }

        // Bind a single Y-position curve onto the named transform path.
        private static void SetLocalPositionY(AnimationClip clip, string path, Keyframe[] yKeys)
        {
            // Route to the generic curve helper using the Y property name.
            SetCurve(clip, path, "m_LocalPosition.y", yKeys);
        }

        // Bind X, Y, and Z position curves onto the named transform path.
        private static void SetPosition(AnimationClip clip, string path, Keyframe[] xKeys, Keyframe[] yKeys, Keyframe[] zKeys)
        {
            // Bind the X-axis curve.
            SetCurve(clip, path, "m_LocalPosition.x", xKeys);
            // Bind the Y-axis curve.
            SetCurve(clip, path, "m_LocalPosition.y", yKeys);
            // Bind the Z-axis curve.
            SetCurve(clip, path, "m_LocalPosition.z", zKeys);
        }

        // Apply one animation curve to the given property of the given transform path.
        private static void SetCurve(AnimationClip clip, string path, string propertyName, Keyframe[] keys)
        {
            // Build the curve from the keyframes.
            AnimationCurve curve = new(keys);
            // Build the editor binding that identifies the target property.
            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(path, typeof(Transform), propertyName);
            // Attach the curve to the clip under that binding.
            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }

        // Convert a list of (time, value) tuples into a Keyframe array.
        private static Keyframe[] Keys(params (float time, float value)[] points)
        {
            // Allocate a list sized to the number of points.
            List<Keyframe> keys = new(points.Length);

            // Visit each (time, value) tuple.
            foreach ((float time, float value) in points)
            {
                // Add a new Keyframe for the tuple.
                keys.Add(new Keyframe(time, value));
            }

            // Convert the list to the final Keyframe array.
            return keys.ToArray();
        }

        // Small helper that creates nested asset folders if they do not already exist.
        private static class DirectoryUtility
        {
            // Make sure the given Assets-relative folder path exists.
            public static void EnsureFolderExists(string assetFolderPath)
            {
                // Split the path into individual folder segments.
                string[] parts = assetFolderPath.Split('/');
                // Begin from the root folder segment ("Assets").
                string current = parts[0];

                // Walk each remaining segment and create missing folders.
                for (int index = 1; index < parts.Length; index++)
                {
                    // Build the next nested path one segment deeper.
                    string next = $"{current}/{parts[index]}";
                    // Create the folder when it does not exist yet.
                    if (!AssetDatabase.IsValidFolder(next))
                    {
                        AssetDatabase.CreateFolder(current, parts[index]);
                    }

                    // Advance the cursor to the next segment.
                    current = next;
                }
            }
        }
    }
}
