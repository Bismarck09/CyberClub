using System;
using System.Collections.Generic;
using System.IO;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CyberClubMainSceneValidator
{
    private const string MainScenePath = "Assets/Scenes/MainScene.unity";
    private const string ExpectedPremiumProductId = "premium_zone_100";

    [MenuItem("Tools/CyberClub/Validate Main Scene")]
    public static void ValidateFromMenu()
    {
        bool isValid = ValidateMainScene();
        EditorUtility.DisplayDialog(
            "CyberClub MainScene validation",
            isValid
                ? "MainScene passed the structural validation."
                : "MainScene has validation errors. See Console for details.",
            "OK");
    }

    public static void ValidateMainSceneBatch()
    {
        bool isValid = ValidateMainScene();
        EditorApplication.Exit(isValid ? 0 : 1);
    }

    public static void BuildDevelopmentWebGLBatch()
    {
        string outputPath = GetCommandLineArgument("-cyberClubBuildPath");
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            Debug.LogError(
                "CyberClub WebGL build: -cyberClubBuildPath is required.");
            EditorApplication.Exit(1);
            return;
        }

        Directory.CreateDirectory(outputPath);

        BuildPlayerOptions options = new()
        {
            scenes = new[] { MainScenePath },
            locationPathName = outputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.Development | BuildOptions.CleanBuildCache
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        Debug.Log(
            $"CyberClub WebGL build summary: result={summary.result}; " +
            $"errors={summary.totalErrors}; warnings={summary.totalWarnings}; " +
            $"sizeBytes={summary.totalSize}; time={summary.totalTime}.");
        EditorApplication.Exit(
            summary.result == BuildResult.Succeeded ? 0 : 1);
    }

    private static bool ValidateMainScene()
    {
        Scene scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        List<GameObject> gameObjects = CollectSceneObjects(scene);
        bool isValid = true;
        int missingScripts = 0;
        int missingReferences = 0;

        foreach (GameObject gameObject in gameObjects)
        {
            int missingOnObject =
                GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
            missingScripts += missingOnObject;

            if (missingOnObject > 0)
            {
                Debug.LogError(
                    $"CyberClub validation: '{GetHierarchyPath(gameObject.transform)}' " +
                    $"has {missingOnObject} missing script(s).",
                    gameObject);
            }

            foreach (Component component in gameObject.GetComponents<Component>())
            {
                if (component == null)
                    continue;

                missingReferences += ValidateSerializedReferences(component);
            }
        }

        isValid &= Require(missingScripts == 0, $"Missing scripts: {missingScripts}.");
        isValid &= Require(
            missingReferences == 0,
            $"Broken serialized object references: {missingReferences}.");

        List<CyberClubYG2PaymentsService> paymentServices =
            CollectComponents<CyberClubYG2PaymentsService>(gameObjects);
        isValid &= Require(paymentServices.Count == 1,
            $"Expected one payment service, found {paymentServices.Count}.");

        if (paymentServices.Count == 1)
        {
            SerializedObject service = new(paymentServices[0]);
            isValid &= RequireString(
                service,
                "_premiumZoneProductId",
                ExpectedPremiumProductId);
            isValid &= RequireReference(service, "_premiumLocationUnlocker");
            isValid &= RequireReference(service, "_saveLoadManager");
            isValid &= RequireReference(service, "_feedbackPresenter");
        }

        List<LocationPurchaseDialog> dialogs =
            CollectComponents<LocationPurchaseDialog>(gameObjects);
        isValid &= Require(dialogs.Count == 1,
            $"Expected one purchase dialog component, found {dialogs.Count}.");

        int activeDialogs = 0;
        foreach (LocationPurchaseDialog dialog in dialogs)
        {
            if (dialog.isActiveAndEnabled)
                activeDialogs++;
        }

        isValid &= Require(activeDialogs == 1,
            $"Expected one active purchase dialog, found {activeDialogs}.");

        foreach (LocationPurchaseDialog dialog in dialogs)
        {
            if (!dialog.isActiveAndEnabled)
                continue;

            SerializedObject serializedDialog = new(dialog);
            isValid &= RequireReference(serializedDialog, "_view");
            isValid &= RequireReference(serializedDialog, "_zonePurchase");
            isValid &= RequireReference(serializedDialog, "_paymentsService");
            isValid &= RequireReference(serializedDialog, "_premiumUnlocker");
            isValid &= RequireReference(serializedDialog, "_premiumZoneConfig");
            isValid &= RequireReference(serializedDialog, "_premiumZoneInformation");
            isValid &= RequireReference(serializedDialog, "_interactionWithUI");
        }

        List<EventSystem> eventSystems = CollectComponents<EventSystem>(gameObjects);
        List<InputSystemUIInputModule> inputModules =
            CollectComponents<InputSystemUIInputModule>(gameObjects);
        isValid &= Require(eventSystems.Count == 1,
            $"Expected one EventSystem, found {eventSystems.Count}.");
        isValid &= Require(inputModules.Count == 1,
            $"Expected one InputSystemUIInputModule, found {inputModules.Count}.");

        List<Canvas> canvases = CollectComponents<Canvas>(gameObjects);
        int worldSpaceCanvases = 0;
        int worldSpaceCanvasesWithoutRaycaster = 0;
        int worldSpaceCanvasesWithoutCamera = 0;

        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode != RenderMode.WorldSpace)
                continue;

            worldSpaceCanvases++;
            if (canvas.GetComponent<GraphicRaycaster>() == null)
                worldSpaceCanvasesWithoutRaycaster++;
            if (canvas.worldCamera == null)
                worldSpaceCanvasesWithoutCamera++;
        }

        isValid &= Require(
            worldSpaceCanvasesWithoutRaycaster == 0,
            $"World-space canvases without GraphicRaycaster: " +
            $"{worldSpaceCanvasesWithoutRaycaster}.");

        List<MobileVirtualJoystick> joysticks =
            CollectComponents<MobileVirtualJoystick>(gameObjects);
        List<MobileLookArea> lookAreas =
            CollectComponents<MobileLookArea>(gameObjects);
        isValid &= Require(joysticks.Count == 1,
            $"Expected one mobile joystick, found {joysticks.Count}.");
        isValid &= Require(lookAreas.Count == 1,
            $"Expected one mobile look area, found {lookAreas.Count}.");

        if (joysticks.Count == 1)
        {
            Graphic graphic = joysticks[0].GetComponent<Graphic>();
            isValid &= Require(graphic == null || !graphic.raycastTarget,
                "Joystick technical Graphic must not block UI raycasts.");
        }

        if (lookAreas.Count == 1)
        {
            Graphic graphic = lookAreas[0].GetComponent<Graphic>();
            isValid &= Require(graphic == null || !graphic.raycastTarget,
                "LookArea technical Graphic must not block UI raycasts.");
        }

        List<PlayerRotation> rotations = CollectComponents<PlayerRotation>(gameObjects);
        isValid &= Require(rotations.Count == 1,
            $"Expected one PlayerRotation, found {rotations.Count}.");

        if (rotations.Count == 1)
        {
            SerializedObject rotation = new(rotations[0]);
            isValid &= RequireReference(rotation, "_playerHead");
            isValid &= RequireReference(rotation, "_interactionWithUI");
            isValid &= RequireReference(rotation, "_inputReader");
            isValid &= RequireReference(rotation, "_thirdPersonOrbit");

            SerializedProperty controllerProperty =
                rotation.FindProperty("_cinemachineInputAxisController");
            if (controllerProperty?.objectReferenceValue is
                CinemachineInputAxisController controller)
            {
                isValid &= Require(
                    !controller.enabled,
                    "Direct Cinemachine input controller must be disabled.");
            }
        }

        Debug.Log(
            $"CyberClub validation summary: valid={isValid}; " +
            $"GameObjects={gameObjects.Count}; Canvases={canvases.Count}; " +
            $"WorldSpaceCanvases={worldSpaceCanvases}; " +
            $"WorldSpaceCanvasesWithoutCamera={worldSpaceCanvasesWithoutCamera}; " +
            $"MissingScripts={missingScripts}; " +
            $"BrokenReferences={missingReferences}.");

        return isValid;
    }

    private static List<GameObject> CollectSceneObjects(Scene scene)
    {
        List<GameObject> result = new();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
                result.Add(transform.gameObject);
        }

        return result;
    }

    private static List<T> CollectComponents<T>(List<GameObject> gameObjects)
        where T : Component
    {
        List<T> result = new();

        foreach (GameObject gameObject in gameObjects)
        {
            if (gameObject.TryGetComponent(out T component))
                result.Add(component);
        }

        return result;
    }

    private static int ValidateSerializedReferences(Component component)
    {
        SerializedObject serializedObject = new(component);
        SerializedProperty property = serializedObject.GetIterator();
        int brokenReferences = 0;
        bool enterChildren = true;

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (property.propertyType != SerializedPropertyType.ObjectReference ||
                property.objectReferenceValue != null ||
                property.objectReferenceInstanceIDValue == 0)
            {
                continue;
            }

            brokenReferences++;
            Debug.LogError(
                $"CyberClub validation: broken reference " +
                $"'{property.propertyPath}' on " +
                $"'{GetHierarchyPath(component.transform)}' ({component.GetType().Name}).",
                component);
        }

        return brokenReferences;
    }

    private static bool RequireReference(SerializedObject target, string propertyName)
    {
        SerializedProperty property = target.FindProperty(propertyName);
        return Require(
            property != null && property.objectReferenceValue != null,
            $"'{target.targetObject.name}.{propertyName}' is not assigned.");
    }

    private static bool RequireString(
        SerializedObject target,
        string propertyName,
        string expectedValue)
    {
        SerializedProperty property = target.FindProperty(propertyName);
        return Require(
            property != null && property.stringValue == expectedValue,
            $"'{target.targetObject.name}.{propertyName}' must be " +
            $"'{expectedValue}', actual '{property?.stringValue ?? "<missing>"}'.");
    }

    private static bool Require(bool condition, string message)
    {
        if (condition)
            return true;

        Debug.LogError($"CyberClub validation: {message}");
        return false;
    }

    private static string GetHierarchyPath(Transform transform)
    {
        string path = transform.name;

        while (transform.parent != null)
        {
            transform = transform.parent;
            path = $"{transform.name}/{path}";
        }

        return path;
    }

    private static string GetCommandLineArgument(string name)
    {
        string[] arguments = Environment.GetCommandLineArgs();

        for (int i = 0; i < arguments.Length - 1; i++)
        {
            if (string.Equals(arguments[i], name, StringComparison.Ordinal))
                return arguments[i + 1];
        }

        return string.Empty;
    }
}
