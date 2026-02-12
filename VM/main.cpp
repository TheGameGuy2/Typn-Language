
#include <iostream>
#include <cstdint>
#include <vector>

#define print(x) std::cout<< x;
#define NL std::cout << "\n";

typedef union Value
{
    int32_t iVal;
    float fVal;
} Value;

inline Value FetchInt(void* startAdr,uint32_t* current)
{
    //Expecting current to be at the first byte

    *current += sizeof(int32_t);
    //ending at last related byte

    print("Deciphering int at")
    print(startAdr) NL

    Value val;
    val.iVal = *(int32_t*) startAdr;
    print(val.iVal) NL

    return val;
}



void RunVM()
{ 

    std::vector<unsigned char> bytecode = {1,0,0,0,67,1,0,0,0,0,67,2,0};
    uint32_t current = -1;

    Value stack[256];
    uint32_t sPointer = 0;


    void* instructionLabels[3] = {&&end,&&pushInt,&&addInt};

    start:

    current++;

    goto *instructionLabels[bytecode[current]];


    addInt:
        uint32_t a = stack[sPointer].iVal;
        sPointer--;
        uint32_t b = stack[sPointer].iVal;
        sPointer--;
        Value val;
        val.iVal = a+b;
        stack[sPointer] = val;
        print("Added") NL
        sPointer++;

    goto start;

    pushInt:
        stack[sPointer] = FetchInt(&bytecode[current],&current);
        sPointer++;
        print("pushed") NL
        print("At") NL print(current) NL
    goto start;

    end:
        std::cout<<"Programm end."<<  "Top of Stack:" << stack[sPointer-1].iVal <<"\n";
    


}



int main()
{
    RunVM();
}

