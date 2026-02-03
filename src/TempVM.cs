using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using IR;

namespace Runner
{

    public class VM
    {
        private Instruction[] ops;
        private Stack<IRValue> values = new();

        private Dictionary<string, IRValue> vars = new();

        private sbyte compareFlag = 0;

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

                
                /*Console.WriteLine($"Executing: {curOp}");
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
                        if(curVal.value == "print")
                        {
                            Console.WriteLine(values.Pop().value);   
                        }
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
                    case InstrType.Jne:
                        if(compareFlag!=0)
                        {
                            current = int.Parse(curVal.value);
                        }
                        break;
                    case InstrType.Je:
                        if(compareFlag==0)
                        {
                            current = int.Parse(curVal.value);
                        }
                        break;
                    case InstrType.Jmp:
                        current = int.Parse(curVal.value);
                        break;
                }

                current++;
            }

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



}

