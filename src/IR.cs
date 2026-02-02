
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
    Call,
    CallNative,
    CompEQ,
    CompLE,
    CompGE,
    CompG,
    CompL,
    Push,
    Load,
    Je,
    Jne,
    Jl,
    Jg

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
    public readonly IRValueType type;
    public string value { get; private set; }

    public IRValue(string value, IRValueType type)
    {
        this.value = value;
        this.type = type;
    }

    public override string ToString()
    {
        return $"[{value}]";
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

    public IRValue GetValue()
    {
        if(values.Count == 0)
        {
            return new IRValue("ERROR", IRValueType.Var);
        }
        return values[0];
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

    //variables stores all defined variables and their types for typechecking
    private Dictionary<string, IRDataType> variables = new();

    private Instruction currentLabelJump;

    private Dictionary<string, int> labelDict = new();
    private Dictionary<string, List<Instruction>> labelSubscribers = new();

    private int labelCounter = -1;


    public void ShowInstructions()
    {
        foreach(Instruction inst in instructions)
        {
            Console.WriteLine(inst);
        }
    }



    public string NewLabelName()
    {
        labelCounter++;
        return $"_L{labelCounter}";
    }

    public List<Instruction> GetInstructions()
    {
        return instructions;
    }

    public static IRDataType GetDTFromToken(Token dataType)
    {
        string value = dataType.value;

        if(value == DataTypes.Int)
        {
            return IRDataType.Int;
        }
        else if(value == DataTypes.Float)
        {
            return IRDataType.Float;
        }
        else if (value == DataTypes.Bool)
        {
            return IRDataType.Bool;
        }
        else
        {
            throw new Exception($"[IR Gen] Can not get datatype from vlaue {dataType.value}");
        }
        
    }

    public static InstrType GetInstrFromOp(Token op)
    {
        return op.type switch
        {
            TokenType.Plus => InstrType.Add,
            TokenType.Sub => InstrType.Sub,
            TokenType.Mul => InstrType.Mul,
            TokenType.Div => InstrType.Div,
            _ => throw new Exception($"[IR Gen] Can not get Instruction from token: {op}")
        };
    }


    //Start label beginns label mode, means the current instruction will get an address as a value when EndLabel is called
    public void StartLabel()
    {
        currentLabelJump = instructions[^1];
    }

    //creates a label after the current instruction
    /// <summary>
    ///  Places a label at the current position
    /// </summary>
    /// <param name="name"></param>
    public void MakeLabel(string name)
    {
        labelDict[name] = instructions.Count;
    }

    private void SubscribeToLabel(Instruction subscriber, string labelName)
    {
        if(labelDict.ContainsKey(labelName))
        {
            subscriber.AddValue(new IRValue(labelDict[labelName].ToString(), IRValueType.Const));
        }
        else
        {
            labelSubscribers.TryGetValue(labelName, out List<Instruction>? waitingSubs);
            if(waitingSubs != null)
            {
                labelSubscribers[labelName].Add(subscriber);
            }
            else
            {
                labelSubscribers[labelName] = new List<Instruction>();

                labelSubscribers[labelName].Add(subscriber);
            }
        }
    }

    public void ClearLabel(string name)
    {
        List<Instruction> subs = labelSubscribers[name];
        foreach(Instruction inst in subs)
        {
            inst.AddValue(new IRValue(labelDict[name].ToString(),IRValueType.Const));
        }
        labelSubscribers[name].Clear();
        labelDict.Remove(name);
    }

    //Assigns the later created labels to all subscribers
    //public void FinishLabels()
    //{
    //    //Don't ask me what this does I have no Idea. Just call it after you're done with labels.
    //    foreach(KeyValuePair<string,List<Instruction>> subs in labelSubscribers)
    //    {
    //        if (!labelDict.ContainsKey(subs.Key)) { throw new Exception($"[IR Gen] Label {subs.Key} not found at label clean."); }
    //        
    //        foreach(Instruction inst in subs.Value)
    //        {
    //            inst.AddValue(new IRValue(labelDict[subs.Key].ToString(),IRValueType.Const));
    //        }
    //    }
    //    //labelDict.Clear();
    //    //labelSubscribers.Clear();
    //    
    //}

    public void MakeCmpEq()
    {
        instructions.Add(new Instruction(InstrType.CompEQ));
    }

    public void MakeJmpEQ(string jmpLabel)
    {
        Instruction instr = new Instruction(InstrType.Je);
        instructions.Add(instr);
        SubscribeToLabel(instr, jmpLabel);
    }

    public void MakeJmpNEQ(string jmpLabel)
    {
        Instruction instr = new Instruction(InstrType.Jne);
        instructions.Add(instr);
        SubscribeToLabel(instr, jmpLabel);
    }

    public void MakeLoad(string name)
    {
        if(!variables.ContainsKey(name))
        {
            throw new Exception($"[IR Gen] Trying to access unexisting variable {name}");
        }

        Instruction instr = new(InstrType.Load, variables[name]);

        instr.AddValue(new IRValue(name, IRValueType.Var));

        instructions.Add(instr);
    }

    public void MakeConstant(string value, IRDataType dataType)
    {
        Instruction instr = new(InstrType.Push, dataType);
        instr.AddValue(new IRValue(value, IRValueType.Const));

        instructions.Add(instr);
    }

    public void MakeDefine(string name, IRDataType dataType)
    {
        if(variables.ContainsKey(name))
        {
            throw new Exception($"[IR Gen] Redefining variable {name}. Redefining is not allowed. Context based resolution is WIP.");
        }

        variables[name] = dataType;

        Instruction instr = new(InstrType.Define, dataType);
        instr.AddValue(new IRValue(name, IRValueType.Var));

        instructions.Add(instr);
    }

    public void MakeOperator(InstrType op)
    {
        Instruction instr = new(op, instructions[^1].instrDataType);

        instructions.Add(instr);
    }

    public void MakeCall(string name)
    {
        //TODO functions
        
        Instruction instr = new(InstrType.Call);
        instr.AddValue(new IRValue(name,IRValueType.Var));

        instructions.Add(instr);
    }

    public void MakeSet(string name)
    {
        if(!variables.ContainsKey(name))
        {
           
            throw new Exception($"[IR Gen] Trying to set unexisting variable {name}");
        }

        Instruction instr = new Instruction(InstrType.Set, variables[name]);
        instr.AddValue(new IRValue(name, IRValueType.Var));
        
        instructions.Add(instr);
    }

}