using System.Runtime.CompilerServices;
using IR;

//Code in this file is deprecated. 

namespace Runner;


public class VM
{
    private Instruction[] ops;
    private Stack<IRValue> values = new();
    private Dictionary<string, IRValue> vars = new();
    private sbyte compareFlag = -1;
    public VM(List<Instruction> instructions)
    {
        ops = instructions.ToArray();
    }
    public void Run()
    {
        
        int current = 0;
        
        while(current<ops.Length)
        {
            Instruction curOp = ops[current];
            IRValue curVal = curOp.GetValue();
            /*
            Console.WriteLine($"Executing: {curOp}");
            Console.WriteLine("--S--");
            foreach(IRValue value in values.ToArray())
            {
                Console.WriteLine(value);
            }
            Console.WriteLine("-----");
            Console.ReadKey();
              */
            switch(curOp.type)
            {
                case InstrType.Push:
                    values.Push(curVal);
                    break;
                
                case InstrType.Define:
                    vars[curVal.value] = curVal;
                    break;
                
                case InstrType.Set:
                    IRValue sTop = values.Pop();
                    vars[curVal.value] = sTop;
                    break;
                case InstrType.Load:
                    values.Push(vars[curVal.value]);
                    break;
                    
                case InstrType.Call:
                    
                    DoCall(curVal.value);
                    break;
                case InstrType.Add:
                    DoArith(curOp.type);
                    break;
                case InstrType.Sub:
                    DoArith(curOp.type);
                    break;
                case InstrType.Mul:
                    DoArith(curOp.type);
                    break;
                case InstrType.Div:
                    DoArith(curOp.type);
                    break;
                case InstrType.Not:
                    DoNot(curOp.type);
                    break;
                case InstrType.Comp: 
                    int iCval = int.Parse(values.Pop().value);
                    
                    if(iCval>0)
                    {
                        compareFlag = 1;
                    }
                    else if(iCval<0)
                    {
                        compareFlag = -1;
                    }
                    else
                    {
                        compareFlag = 0;
                    }
                    break;
                case InstrType.CmpEq:
                    DoValueCompare(curOp.type);
                    break;
                case InstrType.CmpG:
                    DoValueCompare(curOp.type);
                    break;
                case InstrType.CmpGEq:
                    DoValueCompare(curOp.type);
                    break;
                case InstrType.CmpLEq:
                    DoValueCompare(curOp.type);
                    break;
                case InstrType.CmpL:
                    DoValueCompare(curOp.type);
                    break;
                case InstrType.CmpNEq:
                    DoValueCompare(curOp.type);
                    break;
                case InstrType.Jmp:
                    current = int.Parse(curVal.value);
                    break;
                
                case InstrType.JmpTrue:
                    if(compareFlag > 0)
                    {
                        current = int.Parse(curVal.value);
                    }
                    break;
                case InstrType.JmpFalse:
                    if(compareFlag <= 0)
                    {
                        current = int.Parse(curVal.value);
                    }
                    break;
                    
            }
            current++;
            
        }
    }
    private void DoValueCompare(InstrType type)
    {
        IRValue val1 = values.Pop();
        IRValue val2 = values.Pop();
        //right push first
        /*
            val1 < val2
            push val2
            push val1
            cmpl (val2 > val1)
        */
        IRValue True = new IRValue("1", IRValueType.Const);
        IRValue False = new IRValue("0", IRValueType.Const);
        int x = int.Parse(val1.value);
        int y = int.Parse(val2.value);
        switch(type)
        {
            case InstrType.CmpNEq:
                if(x != y) 
                {
                    values.Push(True);
                }
                else
                {
                    values.Push(False);
                } 
                break;
            case InstrType.CmpEq:
                if(x == y) 
                {
                    values.Push(True);
                }
                else
                {
                    values.Push(False);
                } 
                break;
            case InstrType.CmpG:
                if(x > y) //x < y because right side of AST generates first
                {
                    values.Push(True);
                }
                else
                {
                    values.Push(False);
                } 
                break;
            case InstrType.CmpGEq:
                if(x >= y) 
                {
                    values.Push(True);
                }
                else
                {
                    values.Push(False);
                } 
                break;
            case InstrType.CmpL:
                if(x < y) 
                {
                    values.Push(True);
                }
                else
                {
                    values.Push(False);
                } 
                break;
            case InstrType.CmpLEq:
                if(x <= y)
                {
                    values.Push(True);
                }
                else
                {
                    values.Push(False);
                } 
                break;
        }
    }
    private void DoCall(string func)
    {
        if(func == "print")
        {
            Console.WriteLine(values.Pop().value);   
        }
        else if(func == "printc")
        {
            
            char val = (char)byte.Parse(values.Pop().value);
            Console.Write(val);  
        }
        else if(func == "helloWorld_Print")
        {
            Console.WriteLine("Sometimes I dream of saving the world.\nSaving everyone from the invisible hand.");
        }
        
    }
    
    private void DoNot(InstrType type) //the cat
    {
        
        
        int val = int.Parse(values.Pop().value);
        
        values.Push(new IRValue((val<0 ? 1 : 0).ToString(),IRValueType.Const));
    
    }
    //Todo: type checking
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DoArith(InstrType type)
    {
        IRValue val1 = values.Pop();
        IRValue val2 = values.Pop();
        
        
        int x = int.Parse(val1.value);
        int y = int.Parse(val2.value);
        switch(type)
        {
            case InstrType.Add:
                values.Push(new IRValue((x + y).ToString(), IRValueType.Const));
                break;
            case InstrType.Sub:
                values.Push(new IRValue((x - y).ToString(), IRValueType.Const));
                break;
            case InstrType.Mul:
                values.Push(new IRValue((x * y).ToString(), IRValueType.Const));
                break;
            case InstrType.Div:
                values.Push(new IRValue((x / y).ToString(), IRValueType.Const));
                break;
        }
    }

    
}


