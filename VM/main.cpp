
#include <iostream>
#include <cstdint>
#include <vector>

#define print(x) std::cout<< x;
#define NL std::cout << "\n";

typedef unsigned char byte;

typedef union Value
{
    uint32_t intVal;
    float floatVal;
    bool boolVal;
} Value;


typedef struct Stack
{
    void* values;
    
    uint32_t pointer = 0;
} Stack;

inline Value FetchInstructionInt(void* startAdr,uint32_t* current)
{
    //Expecting current to be at the first byte

    *current += sizeof(int32_t);
    //ending at last related byte

    print("Deciphering int at")
    print(startAdr) NL

    Value val;
    val.intVal = *(int32_t*)startAdr;
    print(val.intVal) NL

    return val;
}


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

//Opcode layout
/*
    B0      B1-B4
    PUSH    VALUE
    STORE   S_ADDRESS //stores instruction stack top in data stack
    LOAD    S_ADDRESS //Loads from data stack into instruction stack

*/
#include "vm_body.hpp";

void RunVM()
{ 

    std::vector<byte> bytecode = {1,0,0,0,67,1,0,0,0,0,67,2,0};
    uint32_t current = -1;

    //Value operationStack[256];
    //uint32_t sPointer = 0;

    Value opStack[256];

    Stack operationStack = {};
    operationStack.values = (void*)opStack;


    Value memStack[1024];
    Stack memoryStack;
    memoryStack.values = (void*)memStack;
    
    

    void* instructionLabels[4] = {&&end,&&allocVar,&&pushInt,&&addInt};

    start:

    current++;

    goto *instructionLabels[bytecode[current]];


    allocVar:
        current++;
        ((Value*)memoryStack.values)[memoryStack.pointer] = FetchInstructionInt(&bytecode[current],&current);
        memoryStack.pointer++;
    goto start;

    addInt:
        

    goto start;

    pushInt:
        
    goto start;

    end:
        std::cout<<"Programm end."<<  "Top of Stack:" << ((Value*)operationStack.values)[operationStack.pointer-1].iVal <<"\n";
    


}



int main()
{
    RunVM();
}

