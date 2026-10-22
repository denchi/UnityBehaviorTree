# Behaviour Tree

Unity package that runs a ScriptableObject behaviour tree of tasks and services from a `Behaviour` component, and edits that tree in a node window. Shared variables come from the sibling [Behaviour Vars](https://github.com/denchi/UnityBehaviorVars) package.

Package id: `com.deathbygravitystudio.behaviour.tree` (0.1.0). Display name: **Behaviour Tree**. Runtime assembly: `Behaviour.Tree`. Editor assembly: `Behaviour.Tree.Editor`.

## Features

- `Tree` assets (`Behaviours/Tree` create menu, or **Behaviour > Tree > Create Tree Asset...**). A new asset starts with a root `Composite` node. `Tree` extends `ILayer` from Behaviour Vars, so each asset holds the shared variable list.
- `Behaviours.BT.Behaviour` ticks one tree. With `playOnAwake`, `Awake` builds a `RuntimeTreeData` from the assigned asset. The first `Update` starts the tree; later frames call `runtimeTree.update` with `Time.deltaTime` while the component is not paused. `pause`, `resume`, and `restart` are public. `pause` ends the running node; `resume` and `restart` start it again.
- Tasks return `State.Running`, `State.Success`, or `State.Failed`. Built-in composites: `Sequence`, `Selector`, `Repeat`, `Invert`, `Convert`, and `Random`. Built-in leaves: `Wait` and `Fail`. Built-in conditions: `CheckBool` and `CheckProbability`.
- Per-node services (`IService`) tick while the node is running, at `ticksPerSecond` (default 1). A service tick that returns a state other than `Running` ends that node update with that state. The built-in service is `Timeout`.
- Custom tasks are `Task` subclasses. `[RuntimeData(typeof(...))]` (from Behaviour Vars) supplies a `RuntimeTaskData` subclass for per-run state. `[TaskColor("hex")]` draws a colour bar on the node. `[BTVar(ValueType)]` on a string or int field draws a popup of matching variable names from the tree open in the Behaviour Tree window.
- **Window > Behaviour Tree Window** lays out nodes, draws links, and shows a properties pane with the tree's variables and the selected task's inspector. Right-click a node to delete it, copy or paste its task, encapsulate it, move it back among siblings, set its task, add a service, add a child (on a composite), or remove the task. **Set task** lists `Task` subclasses from every loaded assembly. **Add service** lists `IService` subclasses from the Behaviour Tree assembly.
- In play mode, with a selected `GameObject` that has an `IBehaviourTreeProvider`, node frames tint from the last returned state. `Task.debugBreak` calls `Debug.Break` when the returned state matches `debugBreakCondition`.
- `IBehaviourTreeProvider` exposes one or more trees to that window. `Behaviour` implements it for its single tree. `setFloat`, `setInt`, `setBool`, `setString`, and `setTrigger` (and the matching getters, except trigger) forward to the runtime variable collection by name or by `string.GetHashCode()`.

## Unity version

`package.json` does not set `unity` or `unityRelease`. The editor sources use C# 9 (`is not` patterns, `or` patterns, and switch expressions). Unity compiles C# 9 from **2021.2** onward, so that is the oldest editor this package can be imported into as written.

## Installation

This package depends on `com.deathbygravitystudio.behaviour.vars` at version `0.1.0` (`Behaviour.Tree` references `Behaviour.Vars`; the editor assembly also references `Behaviour.Vars.Editor`). Unity does not clone that sibling repository from the version dependency alone. Add both git URLs.

In Package Manager, **Add package from git URL** for each line, or merge them into `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.deathbygravitystudio.behaviour.tree": "https://github.com/denchi/UnityBehaviorTree.git",
    "com.deathbygravitystudio.behaviour.vars": "https://github.com/denchi/UnityBehaviorVars.git"
  }
}
```

Vars package: <https://github.com/denchi/UnityBehaviorVars.git>

## Quick start

1. Create a tree with **Behaviour > Tree > Create Tree Asset...**.
2. Open **Window > Behaviour Tree Window** and select the asset. On the **Tree** tab, add a bool named `alert` (default false). Right-click the root, **Set task...** to `Behaviours/BT/Sequence`, then **Add node** twice and set those children to `Behaviours/BT/Conditions/CheckBool` (`varName` = `alert`, `varValue` = true) and to the `CountTo` task below.
3. Add `Behaviours.BT.Behaviour` to a GameObject, assign the tree asset, and leave `playOnAwake` enabled. `SetTree` only builds runtime data when the component's `tree` field is already set, which is how `Awake` uses the inspector reference.
4. Enter play mode. The sequence fails `CheckBool` until something sets `alert`. The component below does that on Space, and restarts the tree on R.

`CountTo` is a custom task. Each run keeps a counter on its runtime data and succeeds once the counter reaches `target`.

```csharp
using Behaviours;
using Behaviours.BT;
using Behaviours.BT.Runtime;

[RuntimeData(typeof(CountTo.CountData))]
public class CountTo : Task
{
    public int target = 3;

    public class CountData : RuntimeTaskData
    {
        public int count;
    }

    public override void start(RuntimeTaskData taskData)
    {
        ((CountData)taskData).count = 0;
    }

    public override State update(RuntimeTaskData taskData, float dt)
    {
        var data = (CountData)taskData;
        data.count++;
        return data.count >= target ? State.Success : State.Running;
    }
}
```

```csharp
using UnityEngine;

public class TreeRunner : MonoBehaviour
{
    Behaviours.BT.Behaviour treeBehaviour;

    void Awake()
    {
        treeBehaviour = GetComponent<Behaviours.BT.Behaviour>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            treeBehaviour.setBool("alert", true);

        if (Input.GetKeyDown(KeyCode.R))
            treeBehaviour.restart();
    }
}
```

`setBool` looks up a variable copied from the tree asset at init. A missing name logs a warning from Behaviour Vars and leaves the tree unchanged. The `int` overloads use the same hash as `RuntimeValueData`: `name.GetHashCode()`. `setTrigger` sets that bool for three frames, and only if the `Behaviour` GameObject is active.

## Built-in tasks and services

| Type | Behaviour |
| --- | --- |
| `Sequence` | Runs children in order. Returns `Running` while the current child runs, `Success` after every child succeeds, and `Failed` when a child fails. |
| `Selector` | Runs children in order. Returns `Success` when a child succeeds, and `Failed` when every child has failed. |
| `Repeat` | Loops the active child (the first child after start). `StopCondition.Never` keeps looping. `Success`, `Fail`, or `Any` returns when the child finishes with that result. |
| `Invert` | Swaps `Success` and `Failed` from the active child. `Running` stays `Running`. |
| `Convert` | Returns `toState` when the active child returns `fromState`. Any other state is returned as-is. |
| `Random` | On start, picks one child and then returns that child's state. With `shuffleResults`, a tree with more than one child avoids the index chosen on the previous start. |
| `Wait` | Stays `Running` until `time` seconds have elapsed, then returns `Success`. |
| `Fail` | Returns `Failed`. |
| `CheckBool` | Returns `Success` when the runtime bool named `varName` equals `varValue`. |
| `CheckProbability` | Returns `Success` when `probability` is greater than or equal to `UnityEngine.Random.value`. |
| `Timeout` | Service. On start, picks a duration between `timeoutTimeMin` and `timeoutTimeMax`. Each tick adds `1 / ticksPerSecond` to the elapsed time and returns `Failed` once the duration is reached. |

`ICondition.update` calls `check` once per tick. Subclass `ICondition` and override `check` for a condition that does not span frames. Subclass `Task` and override `start`, `update`, and `end` for anything else. Subclass `IService` and override `onServiceStarted`, `tick`, and `onServiceEnded` for work that runs beside a node.

## Sibling packages

These repositories are separate packages from Death By Gravity Studio. They share Behaviour Vars (`ILayer`, `RuntimeValueCollection`, `RuntimeDataAttribute`, and the editor types `GenericWindow` and `ValueGUI`). This tree package's assembly references only `Behaviour.Vars` / `Behaviour.Vars.Editor`.

| Repository | Package | What it is |
| --- | --- | --- |
| [UnityBehaviorVars](https://github.com/denchi/UnityBehaviorVars) | `com.deathbygravitystudio.behaviour.vars` | Shared variable system. Required by this package. |
| [UnityBehaviorHFSM](https://github.com/denchi/UnityBehaviorHFSM) | `com.deathbygravitystudio.behaviour.hfsm` | Hierarchical finite state machine. Depends on Vars. |
| [UnityBehaviorGraph](https://github.com/denchi/UnityBehaviorGraph) | `com.deathbygravitystudio.behaviour.graph` | Graph execution system. Depends on Vars. |
| [UnityBehaviorDialogs](https://github.com/denchi/UnityBehaviorDialogs) | `com.deathbygravitystudio.behaviour.dialog` | Dialog system. Depends on Vars, HFSM, and `com.unity.timeline`. |

HFSM, Graph, and Dialogs do not declare a dependency on this tree package. Install them the same way, by git URL, only when you use them.

## Licence

Released under the [MIT License](LICENSE). Copyright (c) 2026 Denis Chiriac / Death By Gravity Studio.
