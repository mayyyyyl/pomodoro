using Blazored.LocalStorage;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Timers;
using Xunit;

namespace Pomodoro.Tests;
    public class IndexTests : TestContext
    {
        private readonly Mock<ILocalStorageService> localStorageMock;

        public interface IFileService
        {
            void WriteAllText(string path, string contents);
        }

        public IndexTests()
        {
            localStorageMock = new Mock<ILocalStorageService>();
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
        public async Task StopPomodoro_ShouldClearTime()
        {
            // Arrange
            var component = RenderComponent<pomodoro.Pages.Index>();
            await component.InvokeAsync(() => component.Instance.StartPomodoro());

            // Act
            await component.InvokeAsync(() => component.Instance.StopPomodoro());

            // Assert
            Assert.False(component.Instance.IsSessionActive);
            Assert.Equal(0, component.Instance.RemainingTime);
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
            component.Instance.PomodoroSetting.WorkDuration = 25;

            // Act
            component.Instance.UpdateWorkTime();

            // Assert
            Assert.Equal(25, component.Instance.TotalWorkMinutes);
        }

        [Fact]
        public void SaveHistory_ShouldWriteToCsvFile()
        {
            // Arrange
            var fileServiceMock = new Mock<IFileService>();
            Services.AddSingleton(fileServiceMock.Object);
            var component = RenderComponent<pomodoro.Pages.Index>();
            component.Instance.SessionHistory.Add("Test session");

            // Act
            component.Instance.SaveHistory();

            // Assert
            fileServiceMock.Verify(fs => fs.WriteAllText(It.IsAny<string>(), "Test session\n"), Times.Once);
        }

        [Fact]
        public void PomodoroSetting_Setter_ShouldUpdateValue()
        {
            // Arrange
            var component = RenderComponent<pomodoro.Pages.Index>();
            var newSettings = new pomodoro.Pages.Index.PomodoroSettings
            {
                WorkDuration = 45,
                BreakDuration = 15
            };

            // Act
            component.Instance.PomodoroSetting = newSettings;

            // Assert
            Assert.Equal(newSettings, component.Instance.PomodoroSetting);
        }

        [Fact]
        public void CleanupTimer_DisposesTimer()
        {
            // Arrange
            var cut = RenderComponent<pomodoro.Pages.Index>();
            var timer = cut.Instance.Timer;

            // Act
            cut.Instance.CleanupTimer();

            // Assert
            Assert.Null(timer);
        }

        [Fact]
        public async Task OnTimerElapsed_DecreasesRemainingTime()
        {
            // Arrange
            var cut = RenderComponent<pomodoro.Pages.Index>();
            cut.Instance.PomodoroSetting = new pomodoro.Pages.Index.PomodoroSettings
            {
                WorkDuration = 1,
                BreakDuration = 1
            };

            cut.Instance.StartPomodoro();
            await Task.Delay(1100);

            var initialRemainingTime = cut.Instance.RemainingTime;
            var dummySender = new object();
            var dummyEventArgs = CreateElapsedEventArgs();

            // Act
            cut.Instance.OnTimerElapsed(dummySender, dummyEventArgs);

            // Assert
            Assert.True(cut.Instance.RemainingTime < initialRemainingTime);
        }

        private static ElapsedEventArgs CreateElapsedEventArgs()
        {
            return Activator.CreateInstance(typeof(ElapsedEventArgs), nonPublic: true) as ElapsedEventArgs
                   ?? throw new InvalidOperationException("Failed to create ElapsedEventArgs instance.");
        }

        [Fact]
        public void IndexComponentRendersWorkSession_WhenIsWorkSessionIsTrue()
        {
            // Arrange
            var component = RenderComponent<pomodoro.Pages.Index>();
            component.Instance.StartPomodoro();

            // Act
            var result = component.Find("h4").TextContent;

            // Assert
            Assert.Equal("Work Session", result);
        }

        [Fact]
        public void IndexComponentRendersBreakSession_WhenIsWorkSessionIsFalse()
        {
            // Arrange
            var component = RenderComponent<pomodoro.Pages.Index>();
            component.Instance.StartPomodoro();
            component.Instance.StopPomodoro();

            // Act
            var result = component.Find("h4").TextContent;

            // Assert
            Assert.Equal("Break Session", result);
        }
    }
}