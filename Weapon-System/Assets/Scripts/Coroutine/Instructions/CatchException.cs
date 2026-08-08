using System;

namespace Coroutine.Instructions
{
    public class CatchException : IYieldInstruction
    {
        private readonly IYieldInstruction _instruction;
        
        public Exception? Exception { get; private set; }

        public CatchException(IYieldInstruction instruction)
        {
            _instruction = instruction;
        }

        public bool IsDone()
        {
            try
            {
                return _instruction.IsDone();
            }
            catch (Exception e)
            {
                Exception = e;
                return true;
            }
        }
    }
    
    public static class CatchExceptionExtensions
    {
        public static CatchException CatchExceptionInstruction(this IYieldInstruction instruction) => new(instruction);
    }
}