import { App } from '@microsoft/teams.apps';
import { DevtoolsPlugin } from '@microsoft/teams.dev';
import { ChatPrompt } from "@microsoft/teams.ai";
import { OpenAIChatModel } from "@microsoft/teams.openai";
import { MessageActivity } from '@microsoft/teams.api';
import { McpClientPlugin } from '@microsoft/teams.mcpclient';

const app = new App({
  plugins: [new DevtoolsPlugin()],
});

app.on('message', async ({ send, activity }) => {

  const model = new OpenAIChatModel({
    apiKey: process.env.AZURE_OPENAI_API_KEY || process.env.OPENAI_API_KEY,
    endpoint: process.env.AZURE_OPENAI_ENDPOINT,
    apiVersion: process.env.AZURE_OPENAI_API_VERSION,
    model: process.env.AZURE_OPENAI_MODEL_DEPLOYMENT_NAME!,
  });

  const mcpServerUrl = process.env['services__MCP-SSE-Server__http__0'] || 'http://localhost:5248';

  const prompt = new ChatPrompt({
    instructions: "You are a friendly assistant who can help employees with their questions about vacation days",
    model,
  },
    [new McpClientPlugin()])
    .usePlugin('mcpClient', { url: mcpServerUrl });

  const response = await prompt.send(activity.text);
  if (response.content) {
    console.log('MCP server:', mcpServerUrl);
    const activity = new MessageActivity(response.content).addAiGenerated();
    await send(activity);
  }
});

(async () => {
  await app.start(+(process.env.PORT || 3000));
})();
