namespace LizardNetRadio.Bot.Service;

using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Castle.Core.Logging;
using Stwalkerster.Bot.CommandLib.Exceptions;

public class AmazonPollyTTSService : ITextToSpeechService
{
    private readonly ILogger logger;
    private readonly AmazonPollyClient pollyClient;
    private readonly Random random = new();

    private readonly List<string> voices;
    private readonly string bucket;

    public AmazonPollyTTSService(ILogger logger, GlobalConfiguration globalConfiguration)
    {
        this.logger = logger;
        
        var stsClient = new AmazonSecurityTokenServiceClient();
        var callerIdentityAsync = stsClient.GetCallerIdentityAsync(
            new GetCallerIdentityRequest(),
            CancellationToken.None);
        callerIdentityAsync.Wait();

        this.logger.InfoFormat(
            "Using account {0} as {1}",
            callerIdentityAsync.Result.Account,
            callerIdentityAsync.Result.Arn);
        
        this.pollyClient = new AmazonPollyClient();

        this.voices = globalConfiguration.AwsConfiguration.Voices;
        this.bucket = globalConfiguration.AwsConfiguration.BucketName;
    }

    public AmazonPollyTTSService(string bucket)
    {
        this.bucket = bucket;
    }

    public async Task<Uri> SpeakAsync(string text)
    {
        var voiceId = this.voices[this.random.Next(this.voices.Count)];
        
        var request = new StartSpeechSynthesisTaskRequest
        {
            Text = "<speak>" + text + "<break strength=\"x-strong\"/></speak>",
            Engine = Engine.Neural,
            VoiceId = voiceId,
            OutputFormat = "mp3",
            TextType = TextType.Ssml,
            OutputS3BucketName = this.bucket
        };
        var startResult = await this.pollyClient.StartSpeechSynthesisTaskAsync(request, CancellationToken.None);

        var synthesisTask = startResult.SynthesisTask;
        
        var attempts = 0;
        while (synthesisTask.TaskStatus != TaskStatus.Completed && synthesisTask.TaskStatus != TaskStatus.Failed && attempts <= 20)
        {
            await Task.Delay(1000, CancellationToken.None);
            
            var get = await this.pollyClient.GetSpeechSynthesisTaskAsync(
                new GetSpeechSynthesisTaskRequest
                {
                    TaskId = synthesisTask.TaskId
                },
                CancellationToken.None);
            
            synthesisTask = get.SynthesisTask;
            attempts++;
        }
        
        if (synthesisTask.TaskStatus == TaskStatus.Failed)
        {
            this.logger.Error(synthesisTask.TaskStatusReason);
            throw new CommandErrorException(synthesisTask.TaskStatusReason);
        }

        this.logger.InfoFormat("Speech synthesis {2} completed using voice {0} for text {1}", voiceId, text, synthesisTask.TaskId);
        
        return new Uri(synthesisTask.OutputUri);
    }
}