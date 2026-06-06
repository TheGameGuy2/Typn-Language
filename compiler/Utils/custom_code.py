
#Some VM functions aren't pre generated because they have special functionality.

custom = '''


inline void Not_bool(Stack& opStack)
{
	Value invVal = *PopStack(opStack);
	invVal.boolVal = !invVal.boolVal;
	PushStack(invVal,opStack);
}

inline void And_bool(Stack& opStack)
{
	Value invVal = *PopStack(opStack);
	invVal.boolVal = invVal.boolVal && PopStack(opStack)->boolVal;
	PushStack(invVal,opStack);
}

inline void Or_bool(Stack& opStack)
{
	Value invVal = *PopStack(opStack);
	invVal.boolVal = invVal.boolVal || PopStack(opStack)->boolVal;
	PushStack(invVal,opStack);
}

inline void Comp_int(Stack& opStack, byte* cmpFlag)
{
	int cmpVal = PopStack(opStack)->intVal;
	if(cmpVal < 0)
	{
		*cmpFlag = -1;
	}
	else if(cmpVal > 0)
	{
		*cmpFlag = 1;
	}
	else
	{
		*cmpFlag = 0;
	}
}

inline void Jmp_int(uint32_t* current, std::vector<byte>& bytecode)
{
	*current += 1;
	*current = FetchInstruction_int(&bytecode[*current],current).uintVal; 
}

inline void JmpTrue_int(uint32_t* current, std::vector<byte>& bytecode,const byte& cmpFlag)
{
	if(cmpFlag>0)
	{
		*current += 1;
		*current = FetchInstruction_int(&bytecode[*current],current).uintVal; 
	}
	else
	{
		
		*current += sizeof(int32_t);
	}
}

inline void JmpFalse_int(uint32_t* current, std::vector<byte>& bytecode,const byte& cmpFlag)
{
	if(cmpFlag<=0)
	{
		*current += 1;
		*current = FetchInstruction_int(&bytecode[*current],current).uintVal; 
	}
	else
	{
		*current += sizeof(int32_t);
	}
}

inline void Call_int(Stack& opStack)
{
	Value val = *PopStack(opStack);
	print(val.intVal) NL
}


'''