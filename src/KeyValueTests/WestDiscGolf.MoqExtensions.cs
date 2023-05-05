using Microsoft.Extensions.Logging;
using Moq;


namespace WestDiscGolf.MoqExtensions;

// Hat Tip Adam Storr
// Awesome Extension Method for Verifying via https://adamstorr.azurewebsites.net/blog/mocking-ilogger-with-moq
// https://github.com/WestDiscGolf/Random
// Eventuall replace with nuget package:
// https://github.com/calebjenkins/WestDiscGolf.MoqExtensions

public static class MoqLoggingExtensions
{
    public static Mock<ILogger> VerifyLogging(this Mock<ILogger> logger, string expectedMessage,
        LogLevel expectedLogLevel = LogLevel.Debug, Times? times = null)
    {
        times ??= Times.Once();

        Func<object, Type, bool> state = (v, t) => v.ToString().CompareTo(expectedMessage) == 0;

        logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == expectedLogLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => state(v, t)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), (Times)times);

        return logger;
    }

    public static Mock<ILogger<T>> VerifyLogging<T>(this Mock<ILogger<T>> logger, string expectedMessage,
        LogLevel expectedLogLevel = LogLevel.Debug, Times? times = null)
    {
        times ??= Times.Once();

        Func<object, Type, bool> state = (v, t) => v.ToString().CompareTo(expectedMessage) == 0;

        logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == expectedLogLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => state(v, t)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), (Times)times);

        return logger;
    }
}

