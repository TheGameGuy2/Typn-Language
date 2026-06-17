using Lexing;


namespace IR;

public enum InstrType : byte
{
    End,
    Define,
    Add,
    Mul,
    Sub,
    Div,
    Not,
    And,
    Or,

    Set,
    Call,
    CallNative,
    Comp, //Sets the compare flag.
    CmpG, //pushes bool value of comparisson.
    CmpGEq, //pushes bool value of comparisson.
    CmpL,//pushes bool value of comparisson.
    CmpLEq,//pushes bool value of comparisson.
    CmpEq,//pushes bool value of comparisson.
    CmpNEq,//pushes bool value of comparisson.
    Push,
    Load,
    Jmp,
    JmpTrue,
    JmpFalse,
    Label


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
    Bool,
    String
}

//switch to class if something breaks
public struct IRValue
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

    //We need to store stack push instructions before operations so we don't re define variables in e.g. a loop.
    private List<Instruction> functionDefines = new();
    private List<Instruction> functionInstructions = new();

    private int labelCounter = -1;

    public void ShowInstructions()
    {
        foreach(Instruction inst in instructions)
        {
            Console.WriteLine(inst);
        }
    }

    
    public void StartFunction()
    {
        functionDefines.Clear();
        functionInstructions.Clear();
    }


    //Writing the separated code into the main list. 
    public void EndFunction()
    {
        foreach(Instruction i in functionDefines)
        {
            instructions.Add(i);
        }
        
        foreach(Instruction i in functionInstructions)
        {
            instructions.Add(i);
        }
    }

    //TODO: Remove label logic from First IR pass, patch labels after optim. later.
    //TODO: Do that! patch labels in Code Gen.
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

        if(value == TokenDataType.Int)
        {
            return IRDataType.Int;
        }
        else if(value == TokenDataType.Float)
        {
            return IRDataType.Float;
        }
        else if (value == TokenDataType.Bool)
        {
            return IRDataType.Bool;
        }
        else if(value == TokenDataType.String)
        {
            return IRDataType.String;
        }
        else
        {
            throw new Exception($"[IR Gen] Can not get datatype from value {dataType.value}");
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
            TokenType.And => InstrType.And,
            TokenType.Or => InstrType.Or,
            TokenType.Not => InstrType.Not,
            TokenType.NotEqual => InstrType.CmpNEq,
            TokenType.CompEqual => InstrType.CmpEq,
            TokenType.Lesser => InstrType.CmpL,
            TokenType.LessEqual => InstrType.CmpLEq,
            TokenType.Greater => InstrType.CmpG,
            TokenType.GreaterEqual => InstrType.CmpGEq,
            _ => throw new Exception($"[IR Gen] Can not get Instruction from token: {op}")
        };
    }

    public IRDataType GetLastDT()
    {
        return functionInstructions[^1].instrDataType;
    }

    

    //creates a label after the current instruction
    /// <summary>
    ///  Places a label at the current position
    /// </summary>
    /// <param name="name"></param>
    public void MakeLabel(string name)
    {
        //labelDict[name] = instructions.Count-1;
        Instruction label = new(InstrType.Label, IRDataType.None);
        label.AddValue(new(name,IRValueType.Const));
        functionInstructions.Add(label);
    }


    public void ClearLabel(string name)
    {
        
    }


    public void MakeNot()
    {
        Instruction instr = new Instruction(InstrType.Not,IRDataType.Bool);
        functionInstructions.Add(instr);
    }

    public void MakeAnd()
    {
        Instruction instr = new Instruction(InstrType.And,IRDataType.Bool);
        functionInstructions.Add(instr);
    }
    
    public void MakeOr()
    {
        Instruction instr = new Instruction(InstrType.Or,IRDataType.Bool);
        functionInstructions.Add(instr);
    }

    public void MakeJump(string jmpLabel)
    {
        Instruction instr = new Instruction(InstrType.Jmp);
        instr.AddValue(new(jmpLabel,IRValueType.Const));
        functionInstructions.Add(instr);
    }

    //Compares the top value of the stack with 0, sets comp flag to result.
    public void MakeCmp()
    {
        Instruction instr = new(InstrType.Comp);
        functionInstructions.Add(instr);
    }

    public void MakeJmpTrue(string jmpLabel)
    {
        Instruction instr = new Instruction(InstrType.JmpTrue);
        instr.AddValue(new(jmpLabel,IRValueType.Const));
        functionInstructions.Add(instr);
    }

    public void MakeJmpFalse(string jmpLabel)
    {
        Instruction instr = new Instruction(InstrType.JmpFalse);
        instr.AddValue(new(jmpLabel,IRValueType.Const));
        functionInstructions.Add(instr);
    }

    
    public void MakeLoad(string name)
    {
        if(!variables.ContainsKey(name))
        {
            throw new Exception($"[IR Gen] Trying to access unexisting variable {name}");
        }

        Instruction instr = new(InstrType.Load, variables[name]);

        instr.AddValue(new IRValue(name, IRValueType.Var));

        functionInstructions.Add(instr);
    }

    public void MakeConstant(string value, IRDataType dataType)
    {
        Instruction instr = new(InstrType.Push, dataType);
        instr.AddValue(new IRValue(value, IRValueType.Const));

        functionInstructions.Add(instr);
    }

    public void MakeDefine(string symbolID, IRDataType dataType)
    {
        if(variables.ContainsKey(symbolID))
        {
            throw new Exception($"[IR Gen] Redefining variable {symbolID}. Redefining is not allowed. Context based resolution is WIP.");
        }

        variables[symbolID] = dataType;

        Instruction instr = new(InstrType.Define, dataType);
        instr.AddValue(new IRValue(symbolID, IRValueType.Var));

        functionDefines.Add(instr);
    }

    public void MakeOperator(InstrType op)
    {
        Instruction instr = new(op, functionInstructions[^1].instrDataType);

        functionInstructions.Add(instr);
    }

    public void MakeCall(string name)
    {
        //TODO functions
        
        Instruction instr = new(InstrType.Call);
        instr.AddValue(new IRValue(name,IRValueType.Var));

        functionInstructions.Add(instr);
    }

    public void MakeSet(string name)
    {
        if(!variables.ContainsKey(name))
        {
           
            throw new Exception($"[IR Gen] Trying to set unexisting variable {name}");
        }

        Instruction instr = new Instruction(InstrType.Set, variables[name]);
        instr.AddValue(new IRValue(name, IRValueType.Var));
        
        functionInstructions.Add(instr);
    }

}