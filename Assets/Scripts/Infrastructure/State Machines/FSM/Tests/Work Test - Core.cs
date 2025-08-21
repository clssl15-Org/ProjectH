using System;
using NUnit.Framework;
using UniEngine.FSM;

public class WorkTest_Core
{
    Work work;

    int startedCount;
    int updatedCount;
    int stoppingCount;
    int stoppedCount;

    void Started() => startedCount++;
    void Updated() => updatedCount++;
    void Stopping() => stoppingCount++;
    void Stopped() => stoppedCount++;


    [SetUp]
    public void SetUp()
    {
        work = new Work("Work")
            .SetStartAction(Started)
            .SetUpdateAction(Updated)
            .SetStopAction(Stopping);
        
        work.OnStopped += Stopped;

        startedCount = 0;
        updatedCount = 0;
        stoppingCount = 0;
        stoppedCount = 0;
    }

    [TearDown]
    public void TearDown()
    {
        work?.Dispose();
        work = null;
    }


    #region 100 : State Transition
    [Test]
    public void StateTransition_Open()
    {
        // Act
        work.Open();

        // Assert
        Assert.IsTrue(work.Active);
        Assert.AreEqual(1, startedCount);
    }

    [Test]
    public void StateTransition_Close()
    { 
        work.Open();

        // Act
        work.Close();

        // Assert
        Assert.IsFalse(work.Active);
        Assert.AreEqual(1, stoppedCount);
    }

    [Test]
    public void StateTransition_Dispose()
    {
        // Act
        work.Dispose();

        // Assert
        Assert.IsTrue(work.IsDisposed);
    }
    #endregion



    #region 110 : Open
    [Test]
    public void Open_Active()
    {
        // Arrange
        work.Open();

        // Act
        work.Open();

        // Assert
        Assert.AreEqual(1, startedCount);
    }

    [Test]
    public void Open_Disposed()
    {
        // Arrange
        work.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => work.Open());
    }
    #endregion



    #region 120 : Close
    [Test]
    public void Close_Active()
    {
        // Act
        work.Close();

        // Assert
        Assert.IsFalse(work.Active);
        Assert.AreEqual(0, stoppingCount);
    }

    [Test]
    public void Close_Disposed()
    {
        // Arrange
        work.Dispose();

        // Act & Assert
        Assert.DoesNotThrow(() => work.Close());
    }
    #endregion



    #region 130 : Invoke
    [Test]
    public void Invoke_Active()
    {
        // Arrange
        work.SetActive(true);

        // Act
        work.Invoke();

        // Assert
        Assert.AreEqual(1, updatedCount);
    }

    [Test]
    public void Invoke_Inactive()
    {
        // Act
        work.Invoke();

        // Assert
        Assert.AreEqual(0, updatedCount);
    }

    [Test]
    public void InvokeDisposed()
    {
        // Arrange
        work.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => work.Invoke());
    }
    #endregion



    #region 140 : Dispose
    [Test]
    public void Dispose_Active()
    {
        // Arrange
        work.SetActive(true);

        // Act
        work.Dispose();

        // Assert
        Assert.IsTrue(work.IsDisposed);
        Assert.AreEqual(1, stoppingCount);
    }

    [Test]
    public void Dispose_Disposed()
    {
        // Arrange
        work.Dispose();

        // Act
        work.Dispose();

        // Assert
        Assert.IsTrue(work.IsDisposed);
        Assert.AreEqual(0, stoppingCount);
    }

    [Test]
    public void Dispose_MultipleActivated()
    {
        // Act
        work.SetActive(true);
        work.SetActive(false);
        work.SetActive(true);
        work.Dispose();

        // Assert
        Assert.IsTrue(work.IsDisposed);
        Assert.AreEqual(2, stoppingCount);
    }
    #endregion



    #region 150 : Nested State Transition
    [Test]
    public void Open_WhenStarting()
    {
        // Arrange
        work.SetStartAction(() =>
        {
            Started();
            work.Open();
        });

        // Act
        work.Open();

        // Assert
        Assert.IsTrue(work.Active);

        Assert.AreEqual(1, startedCount);
        Assert.AreEqual(0, updatedCount);
        Assert.AreEqual(0, stoppedCount);
    }

    [Test]
    public void Invoke_WhenStarting()
    {
        // Arrange
        work.SetStartAction(() =>
        {
            Started();
            work.Invoke();
        });

        bool started = false;
        work.OnStarted += () => started = true;

        // Act
        work.Open();

        // Assert
        Assert.IsTrue(work.Active);

        Assert.AreEqual(1, startedCount);
        Assert.IsFalse(started);
        Assert.AreEqual(1, updatedCount); 
        Assert.AreEqual(0, stoppedCount);
    }

    [Test]
    public void Close_WhenStarting()
    {
        // Arrange
        work.SetStartAction(() =>
        {
            Started();
            work.Close();
        });

        bool started = false;
        work.OnStarted += () => started = true;

        // Act
        work.Open();

        // Assert
        Assert.IsFalse(work.Active);

        Assert.AreEqual(1, startedCount);
        Assert.IsFalse(started);
        Assert.AreEqual(0, updatedCount);
        Assert.AreEqual(1, stoppedCount);
    }



    [Test]
    public void Open_WhenUpdating()
    {
        // Arrange
        work.SetUpdateAction(() =>
        {
            Updated();
            work.Open();
        });

        bool updated = false;
        work.OnUpdated += () => updated = true;

        // Act
        work.Open();
        work.Invoke();

        // Assert
        Assert.IsTrue(work.Active);

        Assert.AreEqual(1, startedCount);
        Assert.AreEqual(1, updatedCount);
        Assert.IsTrue(updated);
        Assert.AreEqual(0, stoppedCount);
    }

    [Test]
    public void Invoke_WhenUpdating()
    {
        // Arrange
        bool isUpdating = false;

        work.SetUpdateAction(() =>
        {
            Updated();

            if (isUpdating) return;
            isUpdating = true;

            work.Invoke();
        });

        int updatedCount = 0;
        work.OnUpdated += () => updatedCount++;

        // Act
        work.Open();
        work.Invoke();

        // Assert
        Assert.IsTrue(work.Active);

        Assert.AreEqual(1, startedCount);
        Assert.AreEqual(2, this.updatedCount);
        Assert.AreEqual(1, updatedCount);
        Assert.AreEqual(0, stoppedCount);
    }

    [Test]
    public void Close_WhenUpdating()
    {
        // Arrange
        work.SetUpdateAction(() =>
        {
            Updated();
            work.Close();
        });

        bool updated = false;
        work.OnUpdated += () => updated = true;

        // Act
        work.Open();
        work.Invoke();

        // Assert
        Assert.IsFalse(work.Active);

        Assert.AreEqual(1, startedCount);
        Assert.AreEqual(1, updatedCount);
        Assert.IsFalse(updated);
        Assert.AreEqual(1, stoppedCount);
    }



    [Test]
    public void Open_WhenStopping()
    {
        // Arrange
        work.SetStopAction(() =>
        {
            Stopping();
            work.Open();
        });

        // Act
        work.Open();
        work.Close();

        // Assert
        Assert.IsTrue(work.Active);

        Assert.AreEqual(2, startedCount);
        Assert.AreEqual(0, updatedCount);
        Assert.AreEqual(1, stoppingCount);
        Assert.AreEqual(0, stoppedCount);
    }

    [Test]
    public void Invoke_WhenStopping()
    {
        // Arrange
        work.SetStopAction(() =>
        {
            Stopping();
            work.Invoke();
        });


        // Act
        work.Open();
        work.Close();

        // Assert
        Assert.IsFalse(work.Active);

        Assert.AreEqual(1, startedCount);
        Assert.AreEqual(0, updatedCount);
        Assert.AreEqual(1, stoppingCount);
        Assert.AreEqual(1, stoppedCount);
    }

    [Test]
    public void Close_WhenStopping()
    {
        // Arrange
        work.SetStopAction(() =>
        {
            Stopping();
            work.Close();
        });

        // Act
        work.Open();
        work.Close();

        // Assert
        Assert.IsFalse(work.Active);

        Assert.AreEqual(1, startedCount);
        Assert.AreEqual(0, updatedCount);
        Assert.AreEqual(1, stoppingCount);
        Assert.AreEqual(1, stoppedCount);
    }
    #endregion
}
