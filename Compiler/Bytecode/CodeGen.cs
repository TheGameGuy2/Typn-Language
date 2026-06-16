using System.Globalization;
using IR;


namespace CodeGen;


public class CodeGenerator
{
    private List<Instruction> irInstructions;
    private Dictionary<InstrType, byte> opcodes = new();
    private Dictionary<IRDataType, byte> dataTypeId = new(); //Data type id needed for some instr.
    private Dictionary<string, int> varStackOffset = new(); //each variable has a stack offset.
    private Dictionary<string, byte[]> labelDict = new(); //stores: label -> address in bytes.
    
    //In case a label is defined later, we store the index of the instructions in need of the label's value.
    //When the label is defined, we go through the list of subscribers and add the address from labeldict to the instruction.
    private Dictionary<string, List<int>> subscriberDict = new(); //LabelName -> List[subscribed addresses] (points to )

    
    private int currentStackBase = 0; //stack frame pointer for data stack
    private int currentStackOffset = 0; //stack pointer for data stack

    public CodeGenerator(List<Instruction> instructions)
    {
        irInstructions = instructions;

        const byte dataTypeCount = 3;

        //We generate an instruction type for each data type, that's why we end up with datatypes*instructioncount
        //-1 is done to ignore label instructions.
        byte opcode = 0;
        for(int i = 0; i<Enum.GetValues<InstrType>().Length-1; i++)
        {

            //Console.WriteLine($"{(InstrType)i} : {opcode}");
            opcodes[(InstrType)i] = opcode;

            for(byte j = 0; j<dataTypeCount; j++)
            {
                opcode++;
                
            }
            
            //Console.WriteLine($"{(InstrType)i} : {opcode}");
            //opcode++;
            
        }

        

        dataTypeId[IRDataType.Int] = 0;
        dataTypeId[IRDataType.Float] = 1;
        dataTypeId[IRDataType.Bool] = 2;
        dataTypeId[IRDataType.None] = 0;

        
    }

    public void DefLabel(string name,int address, List<byte> bytecode)
    {
        byte[] labelBytes = BitConverter.GetBytes(address);
        labelDict.Add(name,labelBytes);

        if(subscriberDict.TryGetValue(name, out List<int>? subs))
        {
            //Patch bytecode
            foreach(int sub in subs)
            {
                for(int i = 0; i<labelBytes.Length; i++)
                {
                    bytecode[sub+i] = labelBytes[i]; //+1
                }
            }
        }
    }

    public byte[] GetLabel(string name, int curAddress)
    {
        if(labelDict.TryGetValue(name, out byte[]? address))
        {
            return address;
        }
        else
        {
            if(subscriberDict.TryGetValue(name, out List<int>? subs))
            {
                subs.Add(curAddress);
            }
            else
            {
                subscriberDict.Add(name,[curAddress]);
            }
            return new byte[sizeof(int)]; //empty byte array, gets patched later.
        }
    }

    //Important note here: We treat memory as Value units. Value is a union inside the VM.
    //So stack address 1 is not 1 byte, it's 1*sizeof(Value)
    //Bytecode addresses are in bytes!
    public List<byte> Generate()
    {
        List<byte> code = new(64);

        foreach(Instruction instr in irInstructions)
        {
            if(instr.type == InstrType.Label)
            {
                DefLabel(instr.GetValue().value, code.Count-1, code);
                // count-1 : VM increases PC once after jump. Count -> next instr. Count-1 last instruction.
                continue;
            }

            code.Add((byte)(opcodes[instr.type]+dataTypeId[instr.instrDataType]));

            switch(instr.type)
            {
                case InstrType.Define: //No arguments passed, define always allocates one Value
                    varStackOffset[instr.GetValue().value] = currentStackBase+currentStackOffset;
                    currentStackOffset += 1;
                break;

                case InstrType.Set:
                //mem Stack address
                    foreach(byte b in BitConverter.GetBytes(varStackOffset[instr.GetValue().value]))
                    {
                        code.Add(b);
                    }
                break;

                case InstrType.Load:
                //Stack address
                    foreach(byte b in BitConverter.GetBytes(varStackOffset[instr.GetValue().value]))
                    {
                        code.Add(b);
                    }
                break;

                case InstrType.Push:
                //Value
                    foreach(byte b in GetBytes(instr.instrDataType, instr.GetValue().value))
                    {
                        code.Add(b);
                    }
                break;

                case InstrType.Jmp:
                //Address
                    foreach(byte b in GetLabel(instr.GetValue().value, code.Count))
                    {
                        code.Add(b);
                    }
                break;

                case InstrType.JmpFalse:
                    foreach(byte b in GetLabel(instr.GetValue().value, code.Count))
                    {
                        code.Add(b);
                    }
                break;

                case InstrType.JmpTrue:
                    foreach(byte b in GetLabel(instr.GetValue().value, code.Count))
                    {
                        code.Add(b);
                    }
                break;

                
                    
            }
        }

        //foreach(KeyValuePair<string,byte[]> label in labelDict)
        //{
        //    Console.WriteLine(label.Key);
        //    Console.WriteLine(BitConverter.ToInt32(label.Value).ToString());
        //}

        code.Add(0); //program end

        return code;
    }

    /// <summary>
    /// Gets bytes from a given IRValue and a type.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns>array of bytes.</returns>
    private byte[] GetBytes(IRDataType type, string value)
    {
        switch(type)
        {
            case IRDataType.Float:
                float val = float.Parse(value, CultureInfo.InvariantCulture);
                return BitConverter.GetBytes(val);
            

            case IRDataType.Int:
                int ival = int.Parse(value);
                return BitConverter.GetBytes(ival);

            case IRDataType.Bool:
                byte bval = (byte)(int.Parse(value) > 0 ? 1 : 0);
                return [bval];
            
            default:
                return new byte[1];
        }


    }

}