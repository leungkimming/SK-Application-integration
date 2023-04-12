using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI.ChatCompletion;

class Program {
    static void Main(string[] args) {
        Console.WriteLine("======== SK with ChatGPT ========");

        IKernel kernel = new KernelBuilder().WithLogger(ConsoleLogger.Log).Build();

        // Add your chat completion service
        kernel.Config.AddOpenAIChatCompletionService("chat", "gpt-3.5-turbo", Env.Var("OpenAIKey"));

        IChatCompletion chatGPT = kernel.GetService<IChatCompletion>();
        var chat = (OpenAIChatHistory)chatGPT.CreateNewChat("I want to test planner skill");

        string knowledge = @"Scenario: Given the following functions and examples of how to use them:

(1) When asked about the quantity on hand of an stock item, reply 'we have {{CheckStock(stock item)}} stock item in stock.'
Below are some examples:

Question: what is the stock level of Tea bag?
Reply: we have {{CheckStock('Tea bag')}} Tea bags in stock

Question: how many bicycles are there in stock?
Reply: we have {{CheckStock('bicycle')}} bicycles in stock

(2) When asked to send a memo to a person, reply '{{SendMemo(message,person)}} '
Below are some examples:

Question: please send memo to Bob about the stock level of bicycle
Reply: {{SendMemo('we have {{CheckStock('bicycle')}} bicycles in stock','Bob')}}

Question: please send memo to Peter telling him to attend a meeting tomorrow
Reply: {{SendMemo('Please attend a meeting tomorrow','Peter')}}

Please use the provided background knowledge to answer the questions.";

        int messages_read = 1;
        Console.WriteLine("Chat background knowledge:");
        Console.WriteLine(knowledge);
        Console.WriteLine("-------------------------------------------");
        chat.AddUserMessage(knowledge);

        Console.Write("Studying...");
        string reply = chatGPT.GenerateMessageAsync(chat, new ChatRequestSettings()).Result;
        chat.AddAssistantMessage(reply);
        messages_read = print_message(chat, messages_read);
        Console.Write("user: ");
        string ask = Console.ReadLine();
        while (ask.ToLower() != "end") {
            chat.AddUserMessage(ask);
            Console.Write("Thinking...");
            reply = chatGPT.GenerateMessageAsync(chat, new ChatRequestSettings()).Result;
            string Parsedreply = ParseAndExecuteFunction(reply);
            if (reply != Parsedreply) {
                chat.AddUserMessage($"Background information: {Parsedreply}");
                chat.AddSystemMessage($"{Parsedreply}");
            } else {
                chat.AddAssistantMessage(reply);
            }
            messages_read = print_message(chat, messages_read);

            Console.Write("user: ");
            ask = Console.ReadLine();
        }
        Console.WriteLine("END Chat");
    }

    public static int print_message(OpenAIChatHistory chat, int last_message_no) {
        for (int i = last_message_no + 1; i < chat.Messages.Count; i++) {
            if (chat.Messages[i].AuthorRole != "user") {
                Console.WriteLine($"\n{chat.Messages[i].AuthorRole}: {chat.Messages[i].Content}");
                Console.WriteLine("------------------------");
            }
        }
        return chat.Messages.Count;
    }
    public static string ParseAndExecuteFunction(string input) {
        int start = input.IndexOf("{{");
        int more = input.IndexOf("{{", start + 1);
        while (more > 0) {
            start = more;
            more = input.IndexOf("{{", start + 1);
        }
        int end = input.IndexOf("}}", start + 1);
        if (start == -1 || end == -1 || start > end) {
            return input; // No more function calls, return original string
        }
        string functionCall = input.Substring(start + 2, end - start - 2).Replace("'", "");
        string[] parts = functionCall.Split(new char[] { '(', ',', ')' }, StringSplitOptions.RemoveEmptyEntries);
        string functionName = parts[0];
        string[] parameters = new string[parts.Length - 1];
        Array.Copy(parts, 1, parameters, 0, parameters.Length);

        string functionOutput = CallFunction(functionName, parameters);

        string nestedInput = input.Substring(start, end - start + 2); // Get the nested input string
        string nestedOutput = ParseAndExecuteFunction(functionOutput); // Parse and execute the nested input string
        string output = input.Replace(nestedInput, nestedOutput); // Replace the nested input string with its output

        return ParseAndExecuteFunction(output); // Recursively parse any remaining function calls
    }

    public static string CallFunction(string functionName, string[] parameters) {
        // Dynamically call the specified function using System.Reflection
        Type type = typeof(Program); // Use this class as the type to search for the method
        MethodInfo method = type.GetMethod(functionName); // Get the method with the specified name
        if (method == null) {
            throw new ArgumentException($"Function {functionName} is not supported");
        }
        object[] args = parameters; // Convert parameters to object array
        object result = method.Invoke(null, args); // Invoke the method with the given parameters
        return result.ToString();
    }

    public static string CheckStock(string item) {
        Random random = new Random();
        return random.Next(1, 201).ToString();
    }

    public static string SendMemo(string message, string recipient) {
        // Sample implementation of SendMemo function
        return $"email send to {recipient}@Company.com - \"{message}\"";
    }

}