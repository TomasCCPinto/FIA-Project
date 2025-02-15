using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Filters;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.TestRunner.NUnitExtensions;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools;
using UnityEngine.TestTools.NUnitExtensions;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner
{
    internal interface IUnityTestAssemblyRunnerFactory
    {
        IUnityTestAssemblyRunner Create(TestPlatform testPlatform, string[] orderedTestNames, WorkItemFactory factory);
    }

    internal class UnityTestAssemblyRunnerFactory : IUnityTestAssemblyRunnerFactory
    {
        public IUnityTestAssemblyRunner Create(TestPlatform testPlatform, string[] orderedTestNames, WorkItemFactory factory)
        {
            return new UnityTestAssemblyRunner(new UnityTestAssemblyBuilder(orderedTestNames), factory);
        }
    }

    [Serializable]
    internal class EditModeRunner : ScriptableObject, IDisposable
    {
        [SerializeField]
        private Filter[] m_Filters;

        //The counter from the IEnumerator object
        [SerializeField]
        private int m_CurrentPC;

        [SerializeField]
        private bool m_ExecuteOnEnable;

        [SerializeField]
        private List<string> m_AlreadyStartedTests;

        [SerializeField]
        private List<TestResultSerializer> m_ExecutedTests;

        [SerializeField]
        private List<ScriptableObject> m_CallbackObjects = new List<ScriptableObject>();

        internal TestStartedEvent m_TestStartedEvent;
        internal TestFinishedEvent m_TestFinishedEvent;
        private RunStartedEvent m_RunStartedEvent;
        private RunFinishedEvent m_RunFinishedEvent;

        [SerializeField]
        private TestRunnerStateSerializer m_TestRunnerStateSerializer = new TestRunnerStateSerializer();

        [SerializeField]
        private bool m_RunningTests;

        [SerializeField]
        private TestPlatform m_TestPlatform;

        [SerializeField]
        private object m_CurrentYieldObject;

        [SerializeField]
        private BeforeAfterTestCommandState m_SetUpTearDownState;
        [SerializeField]
        private BeforeAfterTestCommandState m_OuterUnityTestActionState;

        [SerializeField] 
        private EnumerableTestState m_EnumerableTestState;

        [SerializeField]
        private string[] m_OrderedTestNames;

        [SerializeField] 
        public bool RunFinished;

        public bool RunningSynchronously { get; private set; }

        internal IUnityTestAssemblyRunner m_Runner;

        private ConstructDelegator m_ConstructDelegator;

        private IEnumerator m_RunStep;

        public IUnityTestAssemblyRunnerFactory UnityTestAssemblyRunnerFactory { get; set; }

        public void Init(Filter[] filters, TestPlatform platform, bool runningSynchronously,
            RunStartedEvent runStartedEvent, TestStartedEvent testStartedEvent, TestFinishedEvent testFinishedEvent, RunFinishedEvent runFinishedEvent, 
            string[] orderedTestNames)
        {
            m_Filters = filters;
            m_TestPlatform = platform;
            m_AlreadyStartedTests = new List<string>();
            m_ExecutedTests = new List<TestResultSerializer>();
            m_OrderedTestNames = orderedTestNames;
            RunningSynchronously = runningSynchronously;
            m_RunStartedEvent = runStartedEvent;
            m_TestStartedEvent = testStartedEvent;
            m_TestFinishedEvent = testFinishedEvent;
            m_RunFinishedEvent = runFinishedEvent;
            InitRunner();
        }

        private void InitRunner()
        {
            //We give the EditMode platform here so we dont suddenly create Playmode work items in the test Runner.
            m_Runner = (UnityTestAssemblyRunnerFactory ?? new UnityTestAssemblyRunnerFactory()).Create(TestPlatform.EditMode, m_OrderedTestNames, new EditmodeWorkItemFactory());
            var testAssemblyProvider = new EditorLoadedTestAssemblyProvider(new EditorCompilationInterfaceProxy(), new EditorAssembliesProxy());
            var assemblies = testAssemblyProvider.GetAssembliesGroupedByType(m_TestPlatform).Select(x => x.Assembly).ToArray();
            var loadedTests = m_Runner.Load(assemblies, TestPlatform.EditMode,
                UnityTestAssemblyBuilder.GetNUnitTestBuilderSettings(m_TestPlatform));
            CallbacksDelegator.instance.TestTreeRebuild(loadedTests);
            hideFlags |= HideFlags.DontSave;
            EnumerableSetUpTearDownCommand.ActivePcHelper = new EditModePcHelper();
            OuterUnityTestActionCommand.ActivePcHelper = new EditModePcHelper();
        }

        //TODO: Understand if we need to keep OnEnable or we can just have the Resumue method
        public void Resume(RunStartedEvent runStartedEvent, TestStartedEvent testStartedEvent, TestFinishedEvent testFinishedEvent, RunFinishedEvent runFinishedEvent)
        {
            if (m_ExecuteOnEnable)
            {
                m_RunStartedEvent = runStartedEvent;
                m_TestStartedEvent = testStartedEvent;
                m_TestFinishedEvent = testFinishedEvent;
                m_RunFinishedEvent = runFinishedEvent;
                InitRunner();
                foreach (var callback in m_CallbackObjects)
                {
                    AddListeners(callback as ITestRunnerListener);
                }
                m_ConstructDelegator = new ConstructDelegator(m_TestRunnerStateSerializer);

                EnumeratorStepHelper.SetEnumeratorPC(m_CurrentPC);

                UnityWorkItemDataHolder.alreadyExecutedTests = m_ExecutedTests.Select(x => x.uniqueName).ToList();
                UnityWorkItemDataHolder.alreadyStartedTests = m_AlreadyStartedTests;
                Run();
            }
        }
        
        public void OnEnable()
        {
            if (m_ExecuteOnEnable)
            {
                InitRunner();
                m_ExecuteOnEnable = false;
                foreach (var callback in m_CallbackObjects)
                {
                    AddListeners(callback as ITestRunnerListener);
                }
                m_ConstructDelegator = new ConstructDelegator(m_TestRunnerStateSerializer);

                EnumeratorStepHelper.SetEnumeratorPC(m_CurrentPC);

                UnityWorkItemDataHolder.alreadyExecutedTests = m_ExecutedTests.Select(x => x.uniqueName).ToList();
                UnityWorkItemDataHolder.alreadyStartedTests = m_AlreadyStartedTests;
                Run();
            }
        }

        public void TestStartedEvent(ITest test)
        {
            m_AlreadyStartedTests.Add(test.GetUniqueName());
        }

        public void TestFinishedEvent(ITestResult testResult)
        {
            m_AlreadyStartedTests.Remove(testResult.Test.GetUniqueName());
            m_ExecutedTests.Add(TestResultSerializer.MakeFromTestResult(testResult));
        }

        public void Run()
        {
            EditModeTestCallbacks.RestoringTestContext += OnRestoringTest;
            var context = m_Runner.GetCurrentContext();
            if (m_SetUpTearDownState == null)
            {
                m_SetUpTearDownState = CreateInstance<BeforeAfterTestCommandState>();
            }
            context.SetUpTearDownState = m_SetUpTearDownState;

            if (m_OuterUnityTestActionState == null)
            {
                m_OuterUnityTestActionState = CreateInstance<BeforeAfterTestCommandState>();
            }
            context.OuterUnityTestActionState = m_OuterUnityTestActionState;

            if (m_EnumerableTestState == null)
            {
                m_EnumerableTestState = new EnumerableTestState();
            }
            context.EnumerableTestState = m_EnumerableTestState;

            if (!m_RunningTests)
            {
                m_RunStartedEvent.Invoke(m_Runner.LoadedTest);
            }

            if (m_ConstructDelegator == null)
                m_ConstructDelegator = new ConstructDelegator(m_TestRunnerStateSerializer);

            Reflect.ConstructorCallWrapper = m_ConstructDelegator.Delegate;
            m_TestStartedEvent.AddListener(TestStartedEvent);
            m_TestFinishedEvent.AddListener(TestFinishedEvent);

            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;

            RunningTests = true;

            EditorApplication.LockReloadAssemblies();

            var testListenerWrapper = new TestListenerWrapper(m_TestStartedEvent, m_TestFinishedEvent);
            m_RunStep = m_Runner.Run(testListenerWrapper, GetFilter()).GetEnumerator();
            m_RunningTests = true;

            if (!RunningSynchronously) 
                EditorApplication.update += TestConsumer;
        }

        public void CompleteSynchronously()
        {
            while (!m_Runner.IsTestComplete)
                TestConsumer();
        }

        private void OnBeforeAssemblyReload()
        {
            EditorApplication.update -= TestConsumer;

            if (m_ExecuteOnEnable)
            {
                AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
                return;
            }

            if (m_Runner != null && m_Runner.TopLevelWorkItem != null)
                m_Runner.TopLevelWorkItem.ResultedInDomainReload = true;

            if (RunningTests)
            {
                Debug.LogError("TestRunner: Unexpected assembly reload happened while running tests");

                EditorUtility.ClearProgressBar();

                if (m_Runner.GetCurrentContext() != null && m_Runner.GetCurrentContext().CurrentResult != null)
                {
                    m_Runner.GetCurrentContext().CurrentResult.SetResult(ResultState.Cancelled, "Unexpected assembly reload happened");
                }
                OnRunCancel();
            }
        }

        private bool RunningTests;

        private Stack<IEnumerator> StepStack = new Stack<IEnumerator>();

        private bool MoveNextAndUpdateYieldObject()
        {
            var result = m_RunStep.MoveNext();

            if (result)
            {
                m_CurrentYieldObject = m_RunStep.Current;
                while (m_CurrentYieldObject is IEnumerator)    // going deeper
                {
                    var currentEnumerator = (IEnumerator)m_CurrentYieldObject;

                    // go deeper and add parent to stack
                    StepStack.Push(m_RunStep);

                    m_RunStep = currentEnumerator;
                    m_CurrentYieldObject = m_RunStep.Current;
                }

                if (StepStack.Count > 0 && m_CurrentYieldObject != null)    // not null and not IEnumerator, nested
                {
                    Debug.LogError("EditMode test can only yield null, but not <" + m_CurrentYieldObject.GetType().Name + ">");
                }

                return true;
            }

            if (StepStack.Count == 0)       // done
                return false;

            m_RunStep = StepStack.Pop();    // going up
            return MoveNextAndUpdateYieldObject();
        }

        private void TestConsumer()
        {
            var moveNext = MoveNextAndUpdateYieldObject();

            if (m_CurrentYieldObject != null)
            {
                InvokeDelegator();
            }

            if (!moveNext && !m_Runner.IsTestComplete)
            {
                CompleteTestRun();
                throw new IndexOutOfRangeException("There are no more elements to process and IsTestComplete is false");
            }

            if (m_Runner.IsTestComplete)
            {
                CompleteTestRun();
            }
        }

        private void CompleteTestRun()
        {
            if (!RunningSynchronously)
                EditorApplication.update -= TestConsumer;
   
            TestLauncherBase.ExecutePostBuildCleanupMethods(GetLoadedTests(), GetFilter(), Application.platform);
            
            m_RunFinishedEvent.Invoke(m_Runner.Result);
            RunFinished = true;

            if (m_ConstructDelegator != null)
                m_ConstructDelegator.DestroyCurrentTestObjectIfExists();
            Dispose();
            UnityWorkItemDataHolder.alreadyExecutedTests = null;
        }

        private void OnRestoringTest()
        {
            var item = m_ExecutedTests.Find(t => t.fullName == UnityTestExecutionContext.CurrentContext.CurrentTest.FullName);
            if (item != null)
            {
                item.RestoreTestResult(UnityTestExecutionContext.CurrentContext.CurrentResult);
            }
        }

        private static bool IsCancelled()
        {
            return UnityTestExecutionContext.CurrentContext.ExecutionStatus == TestExecutionStatus.AbortRequested || UnityTestExecutionContext.CurrentContext.ExecutionStatus == TestExecutionStatus.StopRequested;
        }

        private void InvokeDelegator()
        {
            if (m_CurrentYieldObject == null)
            {
                return;
            }

            if (IsCancelled())
            {
                return;
            }

            if (m_CurrentYieldObject is RestoreTestContextAfterDomainReload)
            {
                if (m_TestRunnerStateSerializer.ShouldRestore())
                {
                    m_TestRunnerStateSerializer.RestoreContext();
                }

                return;
            }

            try
            {
                if (m_CurrentYieldObject is IEditModeTestYieldInstruction)
                {
                    var editModeTestYieldInstruction = (IEditModeTestYieldInstruction)m_CurrentYieldObject;
                    if (editModeTestYieldInstruction.ExpectDomainReload)
                    {
                        PrepareForDomainReload();
                    }
                    return;
                }
            }
            catch (Exception e)
            {
                UnityTestExecutionContext.CurrentContext.CurrentResult.RecordException(e);
                return;
            }

            UnityTestExecutionContext.CurrentContext.CurrentResult.RecordException(new InvalidOperationException("EditMode test can only yield null"));
        }

        private void CompilationFailureWatch()
        {
            if (EditorApplication.isCompiling)
                return;

            EditorApplication.update -= CompilationFailureWatch;

            if (EditorUtility.scriptCompilationFailed)
            {
                EditorUtility.ClearProgressBar();
                OnRunCancel();
            }
        }

        private void PrepareForDomainReload()
        {
            m_TestRunnerStateSerializer.SaveContext();
            m_CurrentPC = EnumeratorStepHelper.GetEnumeratorPC(TestEnumerator.Enumerator);
            m_ExecuteOnEnable = true;

            RunningTests = false;
        }

        public T AddEventHandler<T>() where T : ScriptableObject, ITestRunnerListener
        {
            var eventHandler = CreateInstance<T>();
            eventHandler.hideFlags |= HideFlags.DontSave;
            m_CallbackObjects.Add(eventHandler);

            AddListeners(eventHandler);

            return eventHandler;
        }

        private void AddListeners(ITestRunnerListener eventHandler)
        {
            m_TestStartedEvent.AddListener(eventHandler.TestStarted);
            m_TestFinishedEvent.AddListener(eventHandler.TestFinished);
            m_RunStartedEvent.AddListener(eventHandler.RunStarted);
            m_RunFinishedEvent.AddListener(eventHandler.RunFinished);
        }

        public void Dispose()
        {
            Reflect.MethodCallWrapper = null;
            EditorApplication.update -= TestConsumer;

            DestroyImmediate(this);

            if (m_CallbackObjects != null)
            {
                foreach (var obj in m_CallbackObjects)
                {
                    DestroyImmediate(obj);
                }
                m_CallbackObjects.Clear();
            }
            RunningTests = false;
            EditorApplication.UnlockReloadAssemblies();
        }

        public void OnRunCancel()
        {
            UnityWorkItemDataHolder.alreadyExecutedTests = null;
            m_ExecuteOnEnable = false;
            m_Runner.StopRun();
            RunFinished = true;
        }

        public ITest GetLoadedTests()
        {
            return m_Runner.LoadedTest;
        }

        public ITestFilter GetFilter()
        {
            return new OrFilter(m_Filters.Select(filter => filter.ToRuntimeTestRunnerFilter(RunningSynchronously).BuildNUnitFilter()).ToArray());
        }
    }
}
