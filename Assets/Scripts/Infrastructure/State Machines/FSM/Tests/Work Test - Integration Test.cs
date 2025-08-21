using System;
using System.Collections.Generic;
using NUnit.Framework;
using UniEngine.FSM;

public class WorkTest_IntegrationTest
{
    Work work_A;
    Work work_X, work_Y;

    List<Work> startedLog;
    List<Work> updatedLog;
    List<Work> stoppedLog;


    [SetUp]
    public void SetUp()
    {
        work_A = new Work("Work A")
            .SetStartAction(() => startedLog.Add(work_A))
            .SetUpdateAction(() => updatedLog.Add(work_A))
            .SetStopAction(() => stoppedLog.Add(work_A));

        work_X = new Work("Work X")
            .SetStartAction(() => startedLog.Add(work_X))
            .SetUpdateAction(() => updatedLog.Add(work_X))
            .SetStopAction(() => stoppedLog.Add(work_X));

        work_Y = new Work("work Y")
            .SetStartAction(() => startedLog.Add(work_Y))
            .SetUpdateAction(() => updatedLog.Add(work_Y))
            .SetStopAction(() => stoppedLog.Add(work_Y));

        startedLog = new();
        updatedLog = new();
        stoppedLog = new();
    }

    [TearDown]
    public void TearDown()
    {
        work_X?.Dispose();
        work_Y?.Dispose();
        work_A?.Dispose();

        work_X = null;
        work_Y = null;
        work_A = null;
    }


    // 400 : Calling Order
    [Test]
    public void CallingOrderTest()
    {
        // Arrange
        work_A.Append(work_X, true);

        // Act
        work_A.Open();

        // Assert
        CollectionAssert.AreEqual(new Work[] { work_A, work_X }, startedLog);
    }



    // 410 : Stopped By Set Next
    [Test]
    public void StoppedBySetNext()
    {
        // Arrange
        work_X.OnUpdated += () => work_A.SetNext(work_Y);

        work_A
            .Append(work_X, true)
            .Append(work_Y);

        // Act & Assert
        work_A.Open();
        work_A.Invoke(); 

        CollectionAssert.AreEqual(new Work[] { work_A, work_X, work_Y }, startedLog);
        CollectionAssert.AreEqual(new Work[] { work_X }, stoppedLog);
        CollectionAssert.AreEqual(new Work[] { work_A, work_X }, updatedLog);


        // Act & Assert 2
        work_A.Invoke();
        CollectionAssert.AreEqual(new Work[] { work_A, work_X, work_A, work_Y }, updatedLog);


        // Act & Assert 3
        work_A.Close();
        CollectionAssert.AreEqual(new Work[] { work_X, work_Y, work_A }, stoppedLog);
    }
}
