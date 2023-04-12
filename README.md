# Example of Semantic Kernel integration with Warehouse Application

## This example uses below Nuget
[![SemanticKernel Nuget package](https://img.shields.io/nuget/vpre/Microsoft.SemanticKernel)](https://www.nuget.org/packages/Microsoft.SemanticKernel/)

## Please set your OpenAI API Key to Environment variable OpenAIKey before running the program

## Introduction
* This program starts a chat with OpenAI gpt-3.5-turbo API service using your API key in the Environment variable 'OpenAIKey'.
* A scenario is provided to gpt-3.5-turbo as a background knowledge.
* In the scenario, gpt-3.5-turbo was taught what Warehouse function calls can be used to fulfil users' queries
* For example, the CheckStock function can be represented below:
* * 	we have {{CheckStock('bicycle')}} bicycles in stock
* Then, the program will parse the replies from gpt-3.5-turbo and actually call the C# function
* The output of the function will replace the {{}} before sending it back to users
* Note that the quantity in stock is only a random number
* These functions are for you to implement your application's real logic or call your real application's API
