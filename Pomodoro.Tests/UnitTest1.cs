using Blazored.LocalStorage;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Pomodoro.Tests;

public class IndexTests : TestContext
{
    public IndexTests()
    {
        var localStorageMock = new Mock<ILocalStorageService>();

        Services.AddSingleton(localStorageMock.Object);
    }

    [Fact]
    public void IndexComponentRendersRemainingTimeCorrectly()
    {
        // Arrange
        var cut = RenderComponent<pomodoro.Pages.Index>();

        // Act
        cut.Find("button[type='submit']").Click();

        // Assert
        cut.Find("p").MarkupMatches("<p>Temps total de travail: 0 minutes</p>");
    }

    [Fact]
    public void StopPomodoro_ShouldStopTimer()
    {
        // Arrange
        var component = RenderComponent<pomodoro.Pages.Index>();
        var startButton = component.Find("button[type='submit']");
        startButton.Click();
        var stopButton = component.FindAll("button")[1];

        // Act
        stopButton.Click();

        // Assert
        Assert.False(component.Instance.isSessionActive);
        Assert.Equal(0, component.Instance.remainingTime);
    }

    [Fact]
    public void IndexComponentRendersCorrectly()
    {
        // Arrange
        var cut = RenderComponent<pomodoro.Pages.Index>();

        // Act
        cut.Instance.StartPomodoro();

        // Assert
        cut.Find("p").MarkupMatches("<p>Temps total de travail: 0 minutes</p>");
    }

    [Fact]
    public void UpdateWorkTime_ShouldIncreaseTotalWorkMinutes()
    {
        // Arrange
        var component = RenderComponent<pomodoro.Pages.Index>();
        component.Instance.pomodoroSettings.WorkDuration = 25;

        // Act
        component.Instance.UpdateWorkTime();

        // Assert
        Assert.Equal(25, component.Instance.totalWorkMinutes);
    }
}