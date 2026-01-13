namespace Application.Common.Abstractions;

public interface IPrivacySafeLogger
{
    void Info(string messageTemplate, params object[] args);
    void Warn(string messageTemplate, params object[] args);
    void Error(Exception ex, string messageTemplate, params object[] args);
}
