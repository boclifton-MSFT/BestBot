using System;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AzureFunctions;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using OpenAI.Chat;

namespace BestPracticesMcp.Agents;

/// <summary>
/// An agent responsible for updating the BestBot MCP server with new programming languages and frameworks.
/// This agent listens for specific triggers related to adding languages and frameworks, and processes requests accordingly.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1852:Type can be sealed", Justification = "Instantiated by Functions host via reflection; suppress analyzer to avoid sealing suggestion")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Functions host via reflection")]
internal class UpdateAgent
{
    private const string AgentName = "UpdateAgent";
    private const string Instructions = """
    You are an agent responsible for updating the BestBot MCP server with new programming languages and frameworks.
    You will receive requests to add new languages and frameworks, and you will process these requests by validating the input, updating the server, and confirming the update. 
    You should ensure that the information provided is accurate and complete before making any updates.
    """;

    public AIAgent CreateAgent(AzureOpenAIClient openAIClient)
    {
        string deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT environment variable is not set.");

        AIAgent agent = openAIClient
            .GetChatClient(deploymentName)
            .AsAIAgent(
                instructions: Instructions,
                name: GetName(),
                tools: [
                    AIFunctionFactory.Create(GetName)
                ]);
        // Agent configuration and tool registration would go here
        return agent;
    }

    private string GetName() => AgentName;

    // Agent logic for handling language/framework addition would go here
}