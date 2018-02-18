# Sanity Checker for Unity - Automatically Find Missing References

One of the most common mistakes in Unity is forgetting to assign references in the Editor. Finding those errors can be time-consuming since Unity's error messages sometimes aren't helpful and there is no way to check for those errors automatically.

Sanity Checker makes it easy to automate error-checking when building scenes and during automated testing and helps avoid most run-time errors from missing references. All you have to do is add an attribute to the field you want to have checked on your MonoBehaviour.

# Features

* Automated error checking in loaded scenes, build scenes and prefabs
* Error messages that make it easy to locate objects with missing references
* 7 types of automated checks
* EditMode test support using Unity's test tools

# Quick Start

Create a new C# script and attach it to any GameObject in your scene. Add a public GameObject field to expose it in the Editor. To enable automatic checks, add the attribute `JetBrains.Annotations.NotNullAttribute` which is included in Unity by default.

```csharp
using JetBrains.Annotations;
using UnityEngine;

public class SampleScript : MonoBehaviour
{
  [NotNull] public GameObject obj;
}
```

Now click `Tools - Sanity Checker - Run Checks in Current Scenes` or press `Control+Alt+C` (or ⌥⌘C on Mac) to run checks in the current scene. You will get an error message in the Unity console that tells you where exactly the reference is missing. You can also click on it to highlight the offending object in your scene hierarchy. Assign any GameObject to the field and run the checks again. Now you will see that the error message doesn't appear again.

## Nested objects

Sometimes you need to use nested objects inside your MonoBehaviour. By default, they are ignored by Sanity Checker. If you want to enable Sanity Checks for a nested object, add `Skaillz.SanityChecker.Attributes.CheckInsideAttribute` to the field containg the object. Sanity Checker will then treat it like a MonoBehaviour and apply all checks to it.

```csharp
using JetBrains.Annotations;
using Skaillz.SanityChecker.Attributes;
using UnityEngine;

public class SampleScript : MonoBehaviour
{
  [CheckInside] public CustomClass cls;
}

public class CustomClass
{
  [NotNull] public GameObject reference;
}
```

## Checking Build Scenes and Prefabs

You can also run Sanity Checks for all your prefabs and build scenes in the `Tools - Sanity Checker` menu.

# Other checks

Aside from checking for missing references, there are other useful checks you can apply by adding their attributes to your fields. To access them, add `using Skaillz.SanityChecker.Attributes` to your source file.

* `[GreaterThan(double)]` (when applied to numeric field)
* `[LessThan(double)]` (when applied to numeric field)
* `[GreaterThanOrEquals(double)]` (when applied to numeric field)
* `[LessThanOrEquals(double)]` (when applied to numeric field)
* `[NotNegative]` (when applied to numeric field)
* `[NotNullOrEmpty]` (when applied to string)

Take a look at the sample scene located in `Plugins/SkaillZ/SanityChecker/Example/SampleScene.unity` and `SampleScript.cs` for examples how to use them.

# Automated testing

Although running sanity checks through the editor can help find issues quickly, scenes with broken references could still slip into your repository. To find those errors automatically before building for production, you can configure Unity Cloud Build or another CI suite to run tests and report back whether any of your tests fail. Sanity Checker can be run in non-interactive mode and comes with a suite of tests.

EditMode tests for auto-checking of build scenes and prefabs can be enabled for Unity's test tools. Click `Tools - Sanity Checker - Enable Editor Tests...` to see instructions how to enable them for your project.