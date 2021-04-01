# Complex Graph

# Introduction

This is a tool that is supposed to make visualization of complex-valued
functions be easier to understand. The main idea is to show positions of
the complex plane points before and after applying the function. Such an
approach is planned to performed by filling some area in preimage complex
plane with colors of different hue and lightness (hue changes along the
real direction and lightness along the imaginary one). Then each point
in image area will be marked by the corresponding color of its preimage.
Thus one can realize how exactly the function changes the complex plane.

# Build

The project is written with using .NET 5.0. You can download dotnet
[here](https://dotnet.microsoft.com/download). Then you can build the
project with running
> dotnet build -c Release ComplexGraph.csproj

# Usage

The program is disigned as CLI. After building use the following EXE file
> src\bin\Release\net5.0\ComplexGraph.exe

Run it with `--help` key to get the full reference of its functionality.

# Examples

## Square root
![sqrt](examples/sqrt.png)

## Sine
![sin](examples/sin.png)

## Cosine
![cos](examples/cos.png)

## Tangent
![tan](examples/tan.png)

## Exponent
![exp](examples/exp.png)

## Natural logarithm
![ln](examples/ln.png)

## Powers
![pows](examples/pows.gif)

## Exponent in different areas
![exps](examples/exps.gif)

# **The author is not responsible for the mental health that can be damaged during reading this code**
