using System.Reflection.Emit;
using HarmonyLib;

namespace CommonAPI
{
    public static class CodeMatcherExtension
    {
        public static CodeMatcher GetInstructionAndAdvance(this CodeMatcher matcher, out OpCode opCode, out object operand)
        {
            opCode = matcher.Opcode;
            operand = matcher.Operand;
            matcher.Advance(1);
            return matcher;
        }
        
        public static CodeMatcher GetLabel(this CodeMatcher matcher, out Label label)
        {
            label = (Label) matcher.Instruction.operand;
            return matcher;
        }
    }
}