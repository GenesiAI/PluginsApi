using System.Collections;
using AiPlugin.Domain.Chat;
using AiPlugin.Domain.Plugin;
using Azure;
using Microsoft.EntityFrameworkCore;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.ChatFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AiPlugin.Application.OpenAI;

public class ChatService : IChatService
{
    private readonly GPTSettings gPTSettings;
    private readonly ILogger<ChatService> logger;

    public ChatService(GPTSettings gPTSettings, ILogger<ChatService> logger)
    {
        this.gPTSettings = gPTSettings;
        this.logger = logger;
    }

    /// <summary>
    /// Ask ChatGPT API to use the right section and reply with data.
    /// </summary>
    /// <param name="rawChat"></param>
    /// <see cref="https://platform.openai.com/docs/api-reference/chat/create"/> discover more at OpenAI API docs
    /// https://platform.openai.com/docs/guides/gpt/function-calling
    /// <returns></returns>
    public async Task<Message> ReplyChatGPT(TestChat rawChat)
    {
        var request = new ChatRequest()
        {
            Model = OpenAI_API.Models.Model.ChatGPTTurbo,
            //valid values as of september 2023 of our interest:
            //gpt-4, gpt-4-32k, gpt-3.5-turbo, gpt-3.5-turbo-16k
            Functions = PluginToFunction(rawChat.AiPlugin),
            Messages = RawMessagesToMessages(rawChat.Messages)
        };

        var api = new OpenAIAPI(gPTSettings.ApiKey);
        var response = await api.Chat.CreateChatCompletionAsync(request);

        if (response.Choices.Count > 1)
        {
            logger.LogWarning("More than one choice returned from OpenAI API. Using the first one.");
        }

        var choice = response.Choices.FirstOrDefault();
        if (choice == null)
        {
            throw new InvalidOperationException("Got no choice from external API.");
        }

        if (choice!.Message.FunctionCall == null)
        {
            if (choice.Message.Role != "assistant")
            {
                throw new InvalidOperationException("Got something strange from external API. The role is not assistant.");
            }
            return new Message() { Content = choice.Message.Content, Role = choice.Message.Role };
        }
        else
        {
            var functionName = choice.Message.FunctionCall.Name;

            var sectionRequiredByAI = rawChat
                .AiPlugin?
                .Sections?
                .FirstOrDefault(section => section.Name == functionName);

            if (sectionRequiredByAI == null)
            {
                logger.LogWarning("Function {functionName} not found in plugin {pluginName}, the AI might have produced the wrong section name", functionName, rawChat.AiPlugin?.NameForHuman);
            }

            request.Messages.Add(choice!.Message); //the AI response
            request.Messages.Add(new ChatMessage() //the function call
            {
                Role = ChatMessageRole.FromString("function"),
                Name = sectionRequiredByAI?.Name ?? "Function not found",
                Content = sectionRequiredByAI?.Content ?? "Function not found"
            });
            request.FunctionCall = new FunctionCall()
            {
                Name = "none" //this would stop it from calling another function, 
                //todo manage recursion and allow the ai to call more than one function 
            };
            var secondresponse = await api.Chat.CreateChatCompletionAsync(request);

            if (secondresponse.Choices.Count > 1)
            {
                logger.LogWarning("More than one choice returned from OpenAI API. Using the first one.");
            }

            var secondChoice = secondresponse.Choices.FirstOrDefault();
            if (secondChoice == null)
            {
                throw new InvalidOperationException("Got no choice from external API.");
            }

            if (secondChoice!.Message.FunctionCall == null)
            {
                if (choice.Message.Role != "assistant")
                {
                    throw new InvalidOperationException("Got something strange from external API. The role is not assistant.");
                }
                return new Message() { Content = secondChoice.Message.Content, Role = secondChoice.Message.Role };
            }
            else
            {
                //todo manage recursion
                //todo allow the ai to call more than one function
                throw new NotImplementedException("The AI returned a function call two times in a row. is it looping?");
            }
        }
    }


    private IList<ChatMessage> RawMessagesToMessages(IEnumerable<Message> sourceMessages)
    {
        var messages = new List<ChatMessage>();
        foreach (var message in sourceMessages)
        {
            switch (message.Role)
            {
                case "user":
                    messages.Add(new ChatMessage(ChatMessageRole.User, message.Content));
                    break;

                case "assistant":
                    messages.Add(new ChatMessage(ChatMessageRole.Assistant, message.Content));
                    break;

                default:
                    throw new InvalidOperationException("You can't do that");
            }
        }
        return messages;
    }

    private List<Function>? PluginToFunction(Plugin aiPlugin)
    {
        return aiPlugin?.Sections?.Select(section => new Function()
        {
            Description = section.Description,
            Name = section.Name,
            Parameters = """{"type": "object", "properties": {}}"""
        }).ToList();
    }
}
