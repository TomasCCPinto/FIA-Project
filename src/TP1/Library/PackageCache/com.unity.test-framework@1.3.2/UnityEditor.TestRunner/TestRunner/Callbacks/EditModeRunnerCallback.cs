using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner
{
    internal class EditModeRunnerCallback : ScriptableObject, ITestRunnerListener
    {
        private EditModeLauncherContextSettings m_Settings;
        public EditModeRunner runner;

        private bool m_Canceled;
        private ITest m_CurrentTest;
        private int m_TotalTests;

        [SerializeField]
        private List<string> m_PendingTests;
        [SerializeField]
        private string m_LastCountedTestName;
        [SerializeField]
        private bool m_RunRestarted;

        public void OnDestroy()
        {
            CleanUp();
        }

        public void RunStarted(ITest testsToRun)
        {
            Setup();
            if (m_PendingTests == null)
            {
                m_PendingTests = GetTestsExpectedToRun(testsToRun, runner.GetFilter());
                m_TotalTests = m_PendingTests.Count;
            }
        }

        public void OnEnable()
        {
            if (m_RunRestarted)
            {
                Setup();
            }
        }

        private void Setup()
        {
            m_Settings = new EditModeLauncherContextSettings();
            Application.logMessageReceivedThreaded += LogReceived;
            EditorApplication.playModeStateChanged += WaitForExitPlaymode;
            EditorApplication.update += DisplayProgressBar;
            AssemblyReloadEvents.beforeAssemblyReload += BeforeAssemblyReload;
        }

        private void BeforeAssemblyReload()
        {
            if (m_CurrentTest != null)
            {
                m_LastCountedTestName = m_CurrentTest.FullName;
                m_RunRestarted = true;
            }
        }

        private void DisplayProgressBar()
        {
            if (m_CurrentTest == null)
                return;
            if (!m_Canceled && EditorUtility.DisplayCancelableProgressBar("Test Runner", "Running test " + m_CurrentTest.Name, Math.Min(1.0f, (float)(m_TotalTests - m_PendingTests.Count) / m_TotalTests)))
            {
                EditorApplication.update -= DisplayProgressBar;
                m_Canceled = true;
                EditorUtility.ClearProgressBar();
                runner.OnRunCancel();
            }
        }

        private static void LogReceived(string message, string stacktrace, LogType type)
        {
            if (TestContext.Out != null)
                TestContext.Out.WriteLine(message);
        }

        private static void WaitForExitPlaymode(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.playModeStateChanged -= WaitForExitPlaymode;
                //because logMessage is reset on Enter EditMode
                //we remove and add the callback
                //because Unity
                Application.logMessageReceivedThreaded -= LogReceived;
                Application.logMessageReceivedThreaded += LogReceived;
            }
        }

        public void RunFinished(ITestResult result)
        {
            CleanUp();
        }

        private void CleanUp()
        {
            m_CurrentTest = null;
            EditorUtility.ClearProgressBar();
            if (m_Settings != null)
            {
                m_Settings.Dispose();
            }
            Application.logMessageReceivedThreaded -= LogReceived;
            EditorApplication.update -= DisplayProgressBar;
        }

        public void TestStarted(ITest test)
        {
            if (test.IsSuite || !(test is TestMethod))
            {
                return;
            }

            m_CurrentTest = test;

            if (m_RunRestarted)
            {
                if (test.FullName == m_LastCountedTestName)
                    m_RunRestarted = false;
            }
        }

        public void TestFinished(ITestResult result)
        {
            if (result.Test is TestMethod)
            {
                m_PendingTests.Remove(result.Test.FullName);
            }
        }

        private static List<string> GetTestsExpectedToRun(ITest test, ITestFilter filter)
        {
            var expectedTests = new List<string>();

            if (filter.Pass(test))
            {
                if (test.IsSuite)
                {
                    expectedTests.AddRange(test.Tests.SelectMany(subTest => GetTestsExpectedToRun(subTest, filter)));
                }
                else
                {
                    expectedTests.Add(test.FullName);
                }
            }

            return expectedTests;
        }
    }
}
