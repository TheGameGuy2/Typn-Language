<h1>Typn Programming Language</h1>

<p>Typn comes from "typed Python" as this language is inspired by Python, and includes some C elements like static typing.</br>
</p>

<h2>General Info</h2>
<p>Typn compiles to bytecode which is then executed by the (for now) stack based Virtual Machine.<br>
Because Typn forces types, it allows us to generate specific instructions for the exact type, avoiding Python's type matching overhead.<br>
</p>

<p>Typn's syntax is a mix of C and Python<br> 
 <br> Here's an example:</p>

<code>
###
This programm calculates fibonacci numbers
###

int:count = 1
int:one = 0
int:two = 1

int:temp;

while(count < 13) 
{
    print(one)

    count += 1

    temp = one + two

    one = two
    two = temp
    
}
</code>

<p>You can look at "Examples" for more code snippets.</p>
<h2>Philosophy</h2>
<p>This language is supposed to be used as a scripting language with low overhead.<br>
This means no JIT compiler. We want to use the execution time purely for executing instructions<br>
Instead, try optimizing the VM or create specialized instructions to reduce overhead.<br>
This will never get as fast as C, but that is not the goal. By generating bytecode we can compile once and run everywhere.</p>
<p>The syntax isn't supposed to be original. It is better to use something that already works (curly braces) and stick with it. <br>
You will notice that some of the default datatypes are shortened, like float to flt. <br>
That was done to keep the base datatypes the same size for consistency, and to save some time typing them.</p>

<h2>Project State</h2>
<p>This project was mainly made for fun. Many important language features like functions, dynamic memory and arrays are missing. <br>
The code quality isn't perfect in some places, documentation and comments are very much needed. </p>
<p>If you look through the code you will notice that the expected C# naming conventions were not followed. <br>
I believe that data and fucntions should not be called the same way. <br>
Methods are written in PascalCase and fields in camelCase. This is intetional.</p>

<h2>Performance Benchmarks</h2>

<p>The VM can still be optimized, but even in this state we already beat python in counting numbers.</p>

<p>CPU: AMD Athlon X4 950</p>

<p>C (compiled with -O0)</p>
<code>
  #include <stdio.h>

int main(void)
{

	int a = 0;
	while(a < 100000000){a++;}
	printf("%d",a);

}

</code>

Time: 
real	0m0.229s
user	0m0.222s
sys	0m0.004s

<p>Python 3.12.3</p>

<code>
a = 0
while(a<100000000):
	a+=1
print(a)
</code>
Time:
real	0m8.596s
user	0m8.499s
sys	0m0.048s

<p>Typn</p>

<code>
int:count = 0

while(count<100000000)
{
 count += 1
}

print(count)

</code>
Time:
real	0m2.517s
user	0m2.454s
sys	0m0.029s


