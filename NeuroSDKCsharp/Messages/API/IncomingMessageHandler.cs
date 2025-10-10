using NeuroSDKCsharp.Websocket;

namespace NeuroSDKCsharp.Messages.API
{
    public interface IIncomingMessageHandler
    {
        ExecutionResult Validate(string command,IncomingData incomingData, out object? resultData);
        void ReportResult(object? resultData,ExecutionResult executionResult);
        void Execute(object? incomingData);
        bool CanHandle(string command);
    }
    
    public abstract class IncomingMessageHandler : IIncomingMessageHandler
    {
        public abstract bool CanHandle(string command);
        protected abstract ExecutionResult Validate(string command,IncomingData incomingData);
        protected abstract void ReportResult(ExecutionResult executionResult);
        protected abstract void Execute();
    
        ExecutionResult IIncomingMessageHandler.Validate(string command,IncomingData incomingData, out object? resultData)
        {
            ExecutionResult result = Validate(command, incomingData);
            resultData = null;
            return result;
        }

        void IIncomingMessageHandler.ReportResult(object? resultData, ExecutionResult executionResult) =>
            ReportResult(executionResult);

        void IIncomingMessageHandler.Execute(object? incomingData) => Execute();
    }
    
    public abstract class IncomingMessageHandler<T> : IIncomingMessageHandler
    {
        public abstract bool CanHandle(string command);
        protected abstract ExecutionResult Validate(string command,IncomingData incomingData, out T? resultData);
        protected abstract void ReportResult(T? resultData,ExecutionResult executionResult);
        protected abstract void Execute(T? incomingData);
    
        ExecutionResult IIncomingMessageHandler.Validate(string command,IncomingData incomingData, out object? resultData)
        {
            ExecutionResult result = Validate(command, incomingData, out var tResultData);
            resultData = tResultData;
            return result;
        }

        void IIncomingMessageHandler.ReportResult(object? resultData, ExecutionResult executionResult) =>
            ReportResult((T?) resultData,executionResult);

        void IIncomingMessageHandler.Execute(object? incomingData) => Execute((T?) incomingData);
    }
}