using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Moq;
using UniEngine.FSM;

public class WorkTest_HierarchyManager
{
    object hierarchyManager;
    object[] args_1;
    object[] args_2;
    Mock<Work> owner, work_A, work_B;

    [SetUp] 
    public void SetUp()
    {
        owner = new Mock<Work>("Owner");
        work_A = new Mock<Work>("Work A");
        work_B = new Mock<Work>("Work B");

        var ctor = typeof(Work)
            .GetNestedType("HierarchyManager", BindingFlags.NonPublic)
            .GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(Work) }, null);

        hierarchyManager = ctor.Invoke(new object[] { owner.Object });
        args_1 = new object[] { new(), new() };
        args_2 = new object[] { new(), new(), new() };
    }

    [TearDown]
    public void TearDown()
    {
        hierarchyManager = null;
        args_1 = null;
        args_2 = null;

        owner = null;
        work_A = null;
        work_B = null;
    }

    #region Reflections
    bool IsActive()
    {
        var value = hierarchyManager.GetType()
            .GetProperty("Active", BindingFlags.Instance | BindingFlags.Public)
            .GetValue(hierarchyManager);

        return (bool)value;
    }

    void Open(params object[] args)
    {
        hierarchyManager.GetType()
            .GetMethod("Open", BindingFlags.Instance | BindingFlags.Public)
            .Invoke(hierarchyManager, new object[] { args });
    }
    void Close()
    {
        hierarchyManager.GetType()
            .GetMethod("Close", BindingFlags.Instance | BindingFlags.Public)
            .Invoke(hierarchyManager, null);
    }
    void AddChild(Work work, bool primary = false)
    {
        try
        {
            hierarchyManager.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(m => m.Name == "AddChild" && m.IsGenericMethod)
                .MakeGenericMethod(typeof(Work))
                .Invoke(hierarchyManager, new object[] { work, primary });
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException;
        }
    }
    void RemoveChild(string name)
    {
        try
        {
            hierarchyManager.GetType()
            .GetMethod("RemoveChild", BindingFlags.Instance | BindingFlags.Public)
            .Invoke(hierarchyManager, new object[] { name });
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException;
        }
    }
    void SetNext(string next, bool restartIfPossible = false, params object[] args)
    {
        hierarchyManager.GetType()
            .GetMethod("SetNext", BindingFlags.Instance | BindingFlags.Public)
            .Invoke(hierarchyManager, new object[] { next, restartIfPossible, args });
    }
    void ClearNext()
    {
        hierarchyManager.GetType()
            .GetMethod("ClearNext", BindingFlags.Instance | BindingFlags.Public)
            .Invoke(hierarchyManager, null);
    }
    #endregion



    #region 200 : State Transition
    [Test]
    public void StateTransition_Open()
    {
        // Act
        Open();

        // Assert
        Assert.IsTrue(IsActive());
    }

    [Test]
    public void StateTransition_Close()
    {
        // Arrange
        Open();

        // Act
        Close();

        // Assert
        Assert.IsFalse(IsActive());
    }
    #endregion



    #region 210 : Child Management
    [Test]
    public void AddChild()
    {
        // Act
        AddChild(work_A.Object);

        // Assert
        Assert.Throws<ArgumentException>(() => AddChild(work_A.Object));
    }

    [Test]
    public void RemoveChild()
    {
        // Arrange
        AddChild(work_A.Object);

        // Act
        RemoveChild(work_A.Object.Name);

        // Assert
        AddChild(work_A.Object);
        Assert.Throws<ArgumentException>(() => AddChild(work_A.Object));
    }

    [Test]
    public void RemoveChild_FromClear()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => RemoveChild(work_A.Object.Name));
    }
    #endregion



    #region 220 : Set Primary
    [Test]
    public void SetPrimary_Normal()
    {
        // Arrange
        AddChild(work_A.Object, true);

        // Act
        Open(args_1);

        // Assert
        work_A.Verify(w => w.Open(It.Is<object[]>(a => a == args_1)), Times.Once());
    }

    [Test]
    public void SetPrimary_MultipleChild()
    {
        // Arrange
        AddChild(work_A.Object, true);
        AddChild(work_B.Object);

        // Act
        Open(args_1);

        // Assert
        work_A.Verify(w => w.Open(It.Is<object[]>(a => a == args_1)), Times.Once());
        work_B.Verify(w => w.Open(), Times.Never());
    }

    [Test]
    public void SetPrimary_MultipleSet()
    {
        // Arrange
        AddChild(work_A.Object, true);
        AddChild(work_B.Object, true);

        // Act
        Open(args_1);

        // Assert
        work_A.Verify(w => w.Open(), Times.Never());
        work_B.Verify(w => w.Open(It.Is<object[]>(a => a == args_1)), Times.Once());
    }
    #endregion



    #region 230 : Set Next
    [Test]
    public void SetNext_BrforeStart()
    {
        // Arrange
        AddChild(work_A.Object);
        SetNext(work_A.Object.Name);

        // Act
        Open(args_1);

        // Assert
        work_A.Verify(w => w.Open(It.Is<object[]>(a => a == args_1)), Times.Once());
    }

    [Test]
    public void SetNext_BeforeStart_MultipleChildren()
    {
        // Arrange
        AddChild(work_A.Object);
        AddChild(work_B.Object);
        SetNext(work_A.Object.Name);

        // Act
        Open(args_1);

        // Assert
        work_A.Verify(w => w.Open(It.Is<object[]>(a => a == args_1)), Times.Once());
        work_B.Verify(w => w.Open(), Times.Never());
    }

    [Test]
    public void SetNext_AfterStart()
    {
        // Arrange
        AddChild(work_A.Object);
        Open(args_1);

        work_A.Verify(w => w.Open(), Times.Never());

        // Act
        SetNext(work_A.Object.Name, args: args_2);

        // Assert
        work_A.Verify(w => w.Open(It.Is<object[]>(a => a == args_2)), Times.Once());
    }

    [Test]
    public void SetNext_AfterStart_MultipleChildren()
    {
        // Arrange
        AddChild(work_A.Object, true);
        AddChild(work_B.Object);
        Open(args_1);

        work_A.Verify(w => w.Open(It.Is<object[]>(a => a == args_1)), Times.Once());
        work_B.Verify(w => w.Open(), Times.Never());

        // Act
        SetNext(work_B.Object.Name, args: args_2);

        // Assert
        work_A.Verify(w => w.Open(It.Is<object[]>(a => a == args_1)), Times.Once());
        work_A.Verify(w => w.Close(), Times.Once());
        work_B.Verify(w => w.Open(It.Is<object[]>(a => a == args_2)), Times.Once());
    }

    [Test]
    public void SetNext_AfterStart_DontRestart()
    {
        // Arrange
        AddChild(work_A.Object, true);
        AddChild(work_B.Object);
        Open(args_1);

        work_A.Verify(w => w.Open(It.Is<object[]>(a => a == args_1)), Times.Once());
        work_B.Verify(w => w.Open(), Times.Never());

        // Act
        SetNext(work_A.Object.Name, args: args_2);

        // Assert
        work_A.Verify(w => w.Open(It.Is<object[]>(a => a == args_1)), Times.Once());
        work_A.Verify(w => w.Close(), Times.Never());
        work_B.Verify(w => w.Open(), Times.Never());
    }

    [Test]
    public void SetNext_AfterStart_Restart()
    {
        // Arrange
        AddChild(work_A.Object, true);
        AddChild(work_B.Object);
        Open(args_1);

        work_A.Verify(w => w.Open(It.Is<object[]>(a => a == args_1)), Times.Once());
        work_B.Verify(w => w.Open(), Times.Never());

        // Act
        SetNext(work_A.Object.Name, true, args_2);

        // Assert
        work_A.Verify(w => w.Open(It.Is<object[]>(a => a == args_1)), Times.Once());
        work_A.Verify(w => w.Open(It.Is<object[]>(a => a == args_2)), Times.Once());
        work_A.Verify(w => w.Close(), Times.Once());
        work_B.Verify(w => w.Open(), Times.Never());
    }
    #endregion



    #region 240 : Clear Next
    [Test]
    public void ClearNext_BeforeOpen()
    {
        // Arrange
        AddChild(work_A.Object, true);

        // Act
        ClearNext();
        Open(args_1);

        // Assert
        work_A.Verify(w => w.Open(), Times.Never());
    }

    [Test]
    public void ClearNext_AfterOpen()
    {
        // Arrange
        AddChild(work_A.Object, true);
        Open(args_1);

        // Act
        SetNext(string.Empty);

        // Assert
        work_A.Verify(w => w.Close(), Times.Once());
    }
    #endregion
}
