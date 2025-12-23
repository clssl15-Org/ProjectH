using System;
using System.Reflection;
using NUnit.Framework;
using Moq;
using UniEngine;
using UniEngine.FSM;

public class WorkTest_StreamManager
{
    object streamManager;
    Mock<Work> owner;

    EventStream stream_1, stream_2;


    [SetUp]
    public void SetUp()
    {
        stream_1 = new();
        stream_2 = new();

        owner = new Mock<Work>("Owner");

        var ctor = typeof(Work)
            .GetNestedType("StreamManager", BindingFlags.NonPublic)
            .GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(Work) }, null);

        streamManager = ctor.Invoke(new object[] { owner.Object });
    }

    [TearDown]
    public void TearDown()
    {
        streamManager = null;
        owner = null;

        stream_1?.Dispose();
        stream_2?.Dispose();

        stream_1 = null;
        stream_2 = null;
    }

    #region Reflections
    void SetStream(EventStream stream)
    {
        streamManager.GetType()
            .GetMethod("SetStream", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(EventStream) }, null)
            .Invoke(streamManager, new object[] { stream });
    }
    void SetStream(Func<EventStream> stream)
    {
        streamManager.GetType()
            .GetMethod("SetStream", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(Func<EventStream>) }, null)
            .Invoke(streamManager, new object[] { stream });
    }
    void RemoveStream()
    {
        streamManager.GetType()
            .GetMethod("RemoveStream", BindingFlags.Instance | BindingFlags.Public)
            .Invoke(streamManager, null);
    }

    void Start()
    {
        streamManager.GetType()
            .GetMethod("Start", BindingFlags.Instance | BindingFlags.Public)
            .Invoke(streamManager, null);
    }
    void Stop()
    {
        streamManager.GetType()
            .GetMethod("Stop", BindingFlags.Instance | BindingFlags.Public)
            .Invoke(streamManager, null);
    }
    #endregion



    #region 300 : State Transition
    [Test]
    public void StateTransition_Started()
    {
        // Arrange
        SetStream(stream_1);
        Start();

        // Act
        stream_1.Update();

        // Assert
        owner.Verify(w => w.Invoke(), Times.Once());
    }

    [Test]
    public void StateTransition_Stopped()
    {
        // Arrange
        SetStream(stream_1);
        Stop();

        // Act
        stream_1.Update();

        // Assert
        owner.Verify(w => w.Invoke(), Times.Never());
    }

    [Test]
    public void StateTransition_RemoveStream()
    {
        // Arrange
        SetStream(stream_1);
        RemoveStream();
        Start();

        // Act
        stream_1.Update();

        // Assert
        owner.Verify(w => w.Invoke(), Times.Never());
    }

    [Test]
    public void StateTransition_ReRegisterStream()
    {
        // Arrange
        SetStream(stream_1);
        RemoveStream();
        SetStream(stream_2);
        Start();

        // Act & Assert
        stream_1.Update();
        owner.Verify(w => w.Invoke(), Times.Never());

        stream_2.Update();
        owner.Verify(w => w.Invoke(), Times.Once());
    }
    #endregion



    #region 310 : General
    [Test]
    public void SetStream_OnInactive()
    {
        // Arrange
        SetStream(stream_1);
        Stop();
        SetStream(stream_2);
        Start();

        // Act & Assert
        stream_1.Update();
        owner.Verify(w => w.Invoke(), Times.Never());

        stream_2.Update();
        owner.Verify(w => w.Invoke(), Times.Once());
    }

    [Test]
    public void SetStream_OnActive()
    {
        // Arrange
        SetStream(stream_1);
        SetStream(stream_2);
        Start();

        // Act & Assert
        stream_1.Update();
        owner.Verify(w => w.Invoke(), Times.Never());

        stream_2.Update();
        owner.Verify(w => w.Invoke(), Times.Once());
    }
    #endregion
}
