
#This file is used to generate some of the VM code

#This is used as an enum for operation methods args.
class ArgTypes:
    opstack = "opstack"
    memstack = "memstack"
    pc = "current"
    bytecode_ptr = "bytecode"

def generate_instructionmap(datatypes:list[str], instructions:list[str]):
    
    res = ""

    res += f"void* instructionLabels[{len(instructions)+len(datatypes)}] = \n "
    res += "{\n"

    for inst in instructions:
        for datatype in datatypes:
            res += f"&&_{inst}_{datatype},"
        res+="\n"
    
    res = res[:-2]
    res+="\n};\n"

    
    return res

    

def generate_instructionmap_decode(datatypes:list[str], instructions:list[str]):
    
    res = ""

    res += f"std::string instructionLabelsDc[{len(instructions)+len(datatypes)}] = \n "
    res += "{\n"

    for inst in instructions:
        for datatype in datatypes:
            res += f'"_{inst}_{datatype}",'
        res+="\n"
    
    res = res[:-2]
    res+="\n};\n"

    
    return res



#TODO handle generic template functions
def generate_labels(datatypes:list[str], instructions:list[str], method_dict:dict):
    res ="start:\n\t current++;\n"

    res+="\tgoto *instructionLabels[bytecode[current]];\n\n"

    for inst in instructions:
        
        for datatype in datatypes:
            res+=f"\t_{inst}_{datatype}:\n"

            #function call
            if inst in method_dict.keys() :
                res += f"{inst}_{datatype}("

                args = []
                if ArgTypes.pc in method_dict[inst]:
                    args.append("&current")

                if ArgTypes.opstack in method_dict[inst]:
                    args.append("operationStack")

                if ArgTypes.memstack in method_dict[inst]:
                    args.append("memoryStack")

                if ArgTypes.bytecode_ptr in method_dict[inst]:
                    args.append("bytecode")
                
                for arg in args:
                    res+=arg+","
                res = res[:-1]
                res+=");\n"
            
            res += "\ngoto _end;" if inst == "End" else "goto start;\n\n"
    return res


def generate_vm_head(max_memory):

    res = '''void RunVM(std::vector<byte>& bytecode) \n{\n'''


    res +=  f'''\tValue opStack[256];\n
\tStack operationStack;\n
\toperationStack.values = (void*)opStack;\n
\tValue memStack[{max_memory}];\n
\tStack memoryStack;\n
\tmemoryStack.values = (void*)memStack;\n
uint32_t current = -1;\n
byte cmpFlag = 0;
    '''

    return res




def generate_value_struct(datatypes, ctypes_dict):
    res = "typedef union Value\n{\n" + "\tValue() = default;\n"
    for dt in datatypes:
        res+="\t" + ctypes_dict[dt] + f" {dt}Val;\n"
        res+=f'''\tValue({ctypes_dict[dt]} val){{ {dt}Val = val; }}\n\n'''
    res+="} Value;"
    return res

'''
inline void Add_int(Stack& opStack)
{
    Value newVal;
    newVal.iVal = PopStack(opStack)->iVal+PopStack(opStack)->iVal;
    PushStack(newVal,opStack);
}
'''

def generate_arithmetic_methods(datatypes):
    res = "\n"
    operators = {"Add":"+","Sub":"-","Mul":"*","Div":"/"}

    for op in operators.keys():
        part_res = ""
        for dt in datatypes:
            part_res += f"\ninline void {op}_{dt}(Stack& opStack)"+"\n{\n"
            part_res += "\tValue newVal;\n"
            part_res += f"\tnewVal.{dt}Val = PopStack(opStack).{dt}Val{operators[op]}PopStack(opStack).{dt}Val; \n"
            part_res += "\tPushStack(newVal,opStack);\n}\n"
        res += part_res + "\n"
    return res

#this is stupid, we should look at what instructions don't require types.
def generate_define_methods(datatypes):
    res = ""
    for dt in datatypes:
        res += f'''inline void Define_{dt}(Stack& memStack)'''+'''
{
	memStack.pointer++;
	((Value*)memStack.values)[memStack.pointer] = Value();

}
'''
    return res


def generate_set_methods():
    res = '''

inline void Set(uint32_t* current, Stack& opStack, Stack& memStack, std::vector<byte>& bytecode)
{
	*current+=1;
	Value setAdr = FetchBytecodeValue<uint32_t>(&(bytecode[*current]),current);
	((Value*)memStack.values)[setAdr.intVal] = *PopStack(opStack);
}
'''
    return res

def generate_instr_fetch():
    res = '''
template<typename T>
inline Value FetchBytecodeValue(void* startAdr, uint32_t* current)
{
	*current += sizeof(T);
	Value val(*(T*)startAdr);
	return val;
}
''' 
    return res

def generate_push():
    res ='''
template<typname T>
inline void Push(uint32_t* current, Stack& opStack, std::vector<byte>& bytecode)
{
	*current += 1;
	PushStack(FetchBytecodeValue<T>(&(bytecode[*current]),current),opStack);
}
'''

  
    return res

def generate_compares(datatypes):
    res = ""

    compareTypes = {"CmpEq":"==", "CmpNEq":"!=", "CmpL":"<", "CmpG":">", "CmpLEq":"<=", "CmpGEq":">="}

    for dt in datatypes:
        for cmp in compareTypes.keys():
            res += f'''
inline void {cmp}_{dt}(Stack& opStack)
{{
	Value val;
	if(PopStack(opStack).{dt}Val {compareTypes[cmp]} PopStack(opStack).{dt}Val)
	{{
		val.intVal = 1;
		PushStack(val,opStack);
	}}
	else
	{{
		val.intVal = 0;
		PushStack(val,opStack);
	}}

}}
'''
    return res

def generate_load():
    res = '''
inline void Load(uint32_t* current, Stack& opStack, Stack& memStack, std::vector<byte>& bytecode)
{
	*current+=1;
	Value getAdr = FetchBytecodeValue<uint32_t>(&(bytecode[*current]),current);
	PushStack(((Value*)memStack.values)[getAdr.intVal], opStack);
}
'''
    return res

instructions = ["End",
            "Define",
            "Add",
            "Mul",
            "Sub",
            "Div",
            "Not",
            "And",
            "Or","Set",
            "Call","CallNative",
            "Comp","CmpG",
            "CmpGEq","CmpL",
            "CmpLEq","CmpEq","CmpNEq",
            "Push","Load",
            "Jmp","JmpTrue",
            "JmpFalse"]

#possible default dt
datatypes = ["int","float","bool"]

#maps default types to actual C types.
ctypes_dict = {"int":"int32_t", "float":"float", "bool":"bool"}

#stores operation and it's method args (current (pc), opStack, memStack are possible args.)
method_dict = {"Define":(ArgTypes.memstack),"Add":(ArgTypes.opstack), "Mul":(ArgTypes.opstack),
               "Sub":(ArgTypes.memstack), "Div":(ArgTypes.opstack),"Not":(ArgTypes.opstack), "And":(ArgTypes.opstack),
               "Or":(ArgTypes.opstack), "Set":(ArgTypes.opstack, ArgTypes.memstack, ArgTypes.pc, ArgTypes.bytecode_ptr), "Call":(ArgTypes.opstack),
               "Comp":(ArgTypes.opstack),"CmpG":(ArgTypes.opstack),"CmpGEq":(ArgTypes.opstack),
               "CmpL":(ArgTypes.opstack), "CmpLEq":(ArgTypes.opstack), "CmpEq":(ArgTypes.opstack),
               "CmpNEq":(ArgTypes.opstack), "Push":(ArgTypes.opstack, ArgTypes.pc, ArgTypes.bytecode_ptr), 
               "Load":(ArgTypes.opstack, ArgTypes.memstack, ArgTypes.pc, ArgTypes.bytecode_ptr),
               "Jmp":(ArgTypes.pc), "JmpTrue":(ArgTypes.opstack,ArgTypes.pc),"JmpFalse":(ArgTypes.opstack,ArgTypes.pc)}


#Warning: Value struct has to be defined before its used!.

file_header = '''//Generated by helper.py
#pragma once
#include <cstdint>
#include <iostream>
#include <vector>
#define print(x) std::cout<< x;
#define NL std::cout << "\\n";

typedef unsigned char byte;

typedef struct Stack
{
    void* values;
    
    uint32_t pointer = 0;
} Stack;

inline Value* PopStack(Stack& opStack)
{
    Value* val = &(((Value*)opStack.values)[opStack.pointer]);
    opStack.pointer--;

    return val;
} 

inline void PushStack(Value val, Stack& opStack)
{
    opStack.pointer++;
    ((Value*)opStack.values)[opStack.pointer] = val;
}

'''



def gen_all():
    from custom_code import custom

    vm = ''.join([file_header,
    generate_value_struct(datatypes,ctypes_dict),
    generate_instr_fetch(),
    generate_arithmetic_methods(["int","float"]),
    generate_compares(datatypes),
    generate_define_methods(datatypes),
    generate_set_methods(),
    generate_load(datatypes),
    generate_push(datatypes),
    custom,
    generate_vm_head(4096),
    generate_instructionmap(datatypes,instructions),
    generate_labels(datatypes, instructions, method_dict),
    "\n}\n"])


    file = open("test.hpp","w")
    file.write(vm)
    file.close()


if __name__ == "__main__":
    vm = generate_value_struct(datatypes,ctypes_dict)
    file = open("test.hpp","w")
    file.write(vm)
    file.close()