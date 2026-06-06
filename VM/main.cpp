
#include <iostream>
#include <fstream>
#include <cstdint>
#include <vector>

//#define DEBUG

#include "vm_body.hpp"
//typedef unsigned char byte;
//
//typedef union Value
//{
//    uint32_t intVal;
//    float floatVal;
//    bool boolVal;
//} Value;
//
//
//typedef struct Stack
//{
//    void* values;
//    
//    uint32_t pointer = 0;
//} Stack;

//inline Value FetchInstructionInt(void* startAdr,uint32_t* current)
//{
//    //Expecting current to be at the first byte
//
//    *current += sizeof(int32_t);
//    //ending at last related byte
//    print("Deciphering int at")
//    print(startAdr) NL
//
//    Value val;
//    val.intVal = *(int32_t*)startAdr;
//    print(val.intVal) NL
//
//    return val;
//}
//
//
//inline Value* PopStack(Stack& opStack)
//{
//    Value* val = &(((Value*)opStack.values)[opStack.pointer]);
//    opStack.pointer--;
//
//    return val;
//} 
//
//inline void PushStack(Value val, Stack& opStack)
//{
//    opStack.pointer++;
//    ((Value*)opStack.values)[opStack.pointer] = val;
//}

//Opcode layout
/*
    B0      B1-B4
    PUSH    VALUE
    STORE   S_ADDRESS //stores instruction stack top in data stack
    LOAD    S_ADDRESS //Loads from data stack into instruction stack

*/





int main(int argc, char* argv[])
{

    if(argc>1)
    {
        
        std::string path = argv[1];

        std::ifstream file(path, std::ios::binary);
        
        if(!file)
        {
            print("File not found: ") print(path) NL
            return -1;
        }


        std::vector<byte> bytecode = std::vector<byte>(std::istreambuf_iterator<char>(file),
        std::istreambuf_iterator<char>());

        RunVM(bytecode);

    }   
    else
    {
        print("No input program.") NL
    }
}

