
#include <iostream>
#include <fstream>
#include <cstdint>
#include <vector>

//#define DEBUG

#include "vm_body.hpp"



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

