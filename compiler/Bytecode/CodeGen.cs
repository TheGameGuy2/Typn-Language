using System.Globalization;
using IR;
using Parsing;


namespace CodeGen;


public class CodeGenerator
{
    private List<Instruction> irInstructions;
    private Dictionary<InstrType, byte> opcodes = new();
    private Dictionary<IRDataType, byte> dataTypeId = new(); //Data type id needed for some instr.
    
    private Dictionary<string, int> varStackOffset = new(); //each variable has a stack offset.
    private int currentStackBase = 0; //sbp for data stack
    private int currentStackOffset = 0;
    public CodeGenerator(List<Instruction> instructions)
    {
        irInstructions = instructions;

        const byte dataTypeCount = 3;

        //We generate an instruction type for each data type, that's why we end up with datatypes*instructioncount
        byte opcode = 0;
        for(int i = 0; i<Enum.GetValues(typeof(InstrType)).Length; i++)
        {

            Console.WriteLine($"{(InstrType)i} : {opcode}");
            opcodes[(InstrType)i] = opcode;
            for(byte j = 0; j<dataTypeCount; j++)
            {
                opcode++;
                
            }

            Console.WriteLine($"{(InstrType)i} : {opcode}");
            opcode++;
            
        }

        

        dataTypeId[IRDataType.Int] = 0;
        dataTypeId[IRDataType.Float] = 1;
        dataTypeId[IRDataType.Bool] = 2;
        dataTypeId[IRDataType.None] = 0;

        
    }

    //Important note here: We treat memory as Value units. Value is a union.
    //so stack address 1 is not 1 byte, it's 1*sizeof(Value)

    public List<byte> Generate()
    {
        List<byte> code = new(64);

        foreach(Instruction instr in irInstructions)
        {
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

                case InstrType.Jmp | InstrType.JmpFalse | InstrType.JmpTrue:
                //Address
                    foreach(byte b in GetBytes(instr.instrDataType, instr.GetValue().value))
                    {
                        code.Add(b);
                    }
                break;

                
                    
            }
        }

        code.Add(0); //program end

        return code;
    }

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