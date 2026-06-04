
#This file is used to generate some of the VM code

def generate_instructionmap(datatypes:list[str], instructions:list[str]):
    
    res = ""

    res += f"void *instructionLabels[{len(instructions)+len(datatypes)}] = \n "
    res += "{\n"

    for inst in instructions:
        for datatype in datatypes:
            res += f"&&_{inst}_{datatype},"
        res+="\n"
    
    res = res[:-2]
    res+="\n}\n"

    
    return res

    


def generate_labels(datatypes:list[str], instructions:list[str]):
    res ="start:\n\t current++;"

    for inst in instructions:
        for datatype in datatypes:
            res+=f"_{inst}_{datatype}:\n"
            res+="goto start;\n\n"
    return res


def generate_vm_head(max_memory):
    return f'''uint32_t current = -1;\n
    Value operationStack[256];\n
    uint32_t stackPointer = 0;\n
    Value memoryStack[{max_memory}];\n
    uint32_t memPointer = 0;\n
    '''




def generate_value_struct(datatypes, ctypes_dict):
    res = "typedef union Value\n{\n"
    for dt in datatypes:
        res+="\t" + ctypes_dict[dt] + f" {dt}Val;\n"
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



    part_res = ""
    for op in operators.keys():
        for dt in datatypes:
            part_res += f"inline void {op}_{dt}(Stack& opStack)"+"\n{\n"
            part_res += "\tValue newVal;\n"
            part_res += f"\tnewVal.{dt}Val = PopStack(opStack)->{dt}Val{operators[op]}PopStack(opStack)->{dt}Val; \n"
            part_res += "\tPushStack(newVal,opStack);\n}\n"
        res += part_res + "\n"
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

datatypes = ["int","float","bool"]
ctypes_dict = {"int":"uint_32", "float":"float", "bool":"bool"}

vm = ""

print(generate_value_struct(datatypes,ctypes_dict))

file = open("vm_body.hpp","w")
file.write(generate_arithmetic_methods(["int","float"]))
file.close()
