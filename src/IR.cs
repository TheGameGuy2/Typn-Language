
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Lexing;
using Parsing;

namespace IR;

public enum InstrType : byte
{
    Define,
    Add,
    Mul,
    Sub,
    Div,
    Set,
    Push,
    Pop,
    Call,
    CallNative,
    CompEQ,
    CompLE,
    CompGE,
    CompG,
    CompL,


}

public enum IRValueType
{
    Var,
    Const
}

public enum IRDataType
{
    None,
    Float,
    Int,
    Bool
}

public class IRValue
{
    private readonly IRValueType type;
    public IRDataType dataType { get; private set; }
    public string value { get; private set; }

    public IRValue(string value, IRValueType type, IRDataType dataType)
    {
        this.value = value;
        this.type = type;
        this.dataType = dataType;
    }

    public override string ToString()
    {
        return $"[{dataType} {value}]";
    }
}

public class Instruction
{
    public readonly InstrType type;
    public readonly IRDataType instrDataType; //this is used to differentiate instructions such as add int and add float

    private List<IRValue> values = new();

    public Instruction(InstrType type, IRDataType dataType = IRDataType.None)
    {
        this.type = type;
        this.instrDataType = dataType;
    }

    public void AddValue(IRValue value)
    {
        values.Add(value);
    }

    public override string ToString()
    {
        string instStr = $"{type} {instrDataType} ";
        foreach(IRValue value in values)
        {
            instStr += value.ToString() + " ";
        }
        return instStr;
    }

}



public class IRBuilder
{

    private List<Instruction> instructions = new();

    private Dictionary<string, int> nativeFuncMap =  new();

    public IRBuilder()
    {
        nativeFuncMap["print"] = 0;
    }


    public void ShowInstructions()
    {
        foreach(Instruction inst in instructions)
        {
            Console.WriteLine(inst);
        }
    }

    private static IRDataType GetDTFromToken(Token dataType)
    {
        return dataType.value switch
        {
            "int" => IRDataType.Int,
            "float" => IRDataType.Float,
            "bool" => IRDataType.Bool,
            _ => throw new Exception($"[IR Gen] Can not get datatype from vlaue {dataType.value}"),
        };
    }

    public void MakeDefine(Token var_name, Token dataType)
    {
        Instruction newInst = new(InstrType.Define);
        newInst.AddValue(new IRValue(var_name.value, IRValueType.Var,GetDTFromToken(dataType)));
        
        instructions.Add(newInst);
    }

    public void MakeArithmetic(Token op)
    {

        InstrType type = op.type switch
        {
            TokenType.Plus => InstrType.Add,
            TokenType.Sub => InstrType.Sub,
            TokenType.Mul => InstrType.Mul,
            TokenType.Div => InstrType.Div,
            _ => throw new Exception($"[IR Gen] Invalid operator token {op}")
        };

        Instruction newInstr = new(type);

        instructions.Add(newInstr);


    }

    public void MakeSet(Token varName)
    {
        Instruction newInstr = new(InstrType.Set);
        //Set pops value at the top and writes it to var
        newInstr.AddValue(new IRValue(varName.value, IRValueType.Var, IRDataType.None));
        
        instructions.Add(newInstr);
    }

    public void MakeCall(Token name, List<IRValue> args)
    {
        Instruction newInst;
        if(nativeFuncMap.ContainsKey(name.value))
        {
            newInst = new(InstrType.CallNative);

            newInst.AddValue(new IRValue(nativeFuncMap[name.value].ToString(),IRValueType.Var,IRDataType.None));

            foreach(IRValue value in args)
            {
                newInst.AddValue(value);
            }
        }
    }

    public void MakePush(Token value, Token dataType)
    {
        Instruction newInstr = new Instruction(InstrType.Push, GetDTFromToken(dataType));

        //I hope god can forgive me this sin.
        newInstr.AddValue(new IRValue(value.value, value.type == TokenType.Name ? IRValueType.Var : IRValueType.Const, GetDTFromToken(dataType)));

        instructions.Add(newInstr);
    }



}